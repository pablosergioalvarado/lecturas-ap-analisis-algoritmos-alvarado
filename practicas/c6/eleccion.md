# C6 — Elección de un algoritmo para Lecturas AP

## 1. El problema que queremos resolver

En el sistema Lecturas AP identificamos una operación lenta al consultar el listado de períodos de lectura:

`GET /api/AlumbradoPublico/periodos`

Esta operación obtiene los períodos registrados y complementa cada uno con la fecha hasta la cual fue facturado en el sistema comercial.

Al analizar el backend encontramos que, por cada período obtenido, se realiza una consulta individual a Oracle para buscar su lote de facturación.

Es decir, si tenemos 30 períodos, el sistema realiza 30 búsquedas adicionales, una detrás de otra.

**El problema principal no es la cantidad de períodos, sino la forma repetitiva en que se buscan sus datos relacionados.**

## 2. Evidencia: ¿qué volumen de datos tenemos?

Según las estadísticas y consultas de producción documentadas en la investigación:

| Tabla | Registros |
|---|---:|
| PERIODO_LECTURAS_AP | 1.118 |
| LECTURAS_AP | 173.080 |
| CONSUMO_NO_MEDIDO_AP | 936 |
| ERP_ELFEC.FAC_LOTES | 445.909 |

Otros datos relevantes:

- Un suministro consultado tiene 30 períodos históricos.
- Un mes completo (noviembre de 2026) tiene 52 períodos.
- El período individual con mayor cantidad de lecturas tiene 3.291 registros.
- El backend permite consultar hasta 1.000 períodos por página.

Estos datos nos permiten dimensionar el problema y estudiar cómo crecería el costo si aumenta la cantidad de información.

*Nota: las estadísticas corresponden a la evidencia histórica de producción recopilada en la auditoría del 7 de noviembre de 2026.*

## 3. ¿Dónde está la lentitud?

Identificamos tres operaciones relevantes:

| Operación | Qué hace | Problema |
|---|---|---|
| FacLoteFechaResolver | Busca el lote de facturación de cada período | Ejecuta una consulta por cada período |
| UPDATE_ESTADO_PERIODO_LECTURAS_AP | Actualiza estados de períodos | Procesa registros mediante un cursor |
| CALCULAR_CONSUMO_NO_MEDIDO_AP | Calcula consumos | Realizaba búsquedas repetidas sobre las lecturas |

Nos concentraremos en `FacLoteFechaResolver`, porque es el problema principal y se ejecuta en todas las consultas del listado.

En el código encontramos este comportamiento:

1. Se obtiene una lista de períodos.
2. AutoMapper recorre los períodos.
3. Por cada período ejecuta una búsqueda individual en `FAC_LOTES`.
4. Cada búsqueda abre una conexión física a Oracle, debido a la configuración `Pooling=false`.

Esto genera el conocido problema N+1: una consulta principal seguida de muchas consultas adicionales.

## 4. Mediciones reales

Las mediciones históricas registradas en Chrome DevTools muestran lo siguiente:

| Escenario | Períodos | Consultas SQL | Tiempo |
|---|---:|---:|---:|
| Consulta por suministro | 30 | 32 | 3,10 s |
| Consulta por suministro (otra paginación) | 30 | 32 | 3,11 s |
| Consulta por año y mes | 52 | 56 o más | 7,49 s |

¿Por qué los dos primeros casos tardan prácticamente lo mismo? Porque ambos procesan los mismos 30 períodos, independientemente del tamaño de página solicitado.

El tercer escenario también ejecutaba procedimientos PL/SQL adicionales. La investigación identificó que el cálculo de consumo no medido realizaba 52 recorridos completos sobre una tabla de 173.080 lecturas, lo que representaba aproximadamente 4,3 segundos en la medición histórica.

**Conclusión de las mediciones:** el procesamiento repetitivo tiene un impacto importante incluso cuando trabajamos con cantidades relativamente pequeñas de períodos.

*Importante: los 7,49 segundos corresponden a la versión y ejecución históricas medidas. El procedimiento de cálculo de consumo no medido ya no aparece invocado desde el GET en la revisión actual del código. No debemos atribuir toda esa demora al código actual.* (En la actualidad, un ticket de mesa de servicios, hizo que este proceso de calculo de consumo no medido, fuera retirado temporalmente)

## 5. Análisis algorítmico: ¿qué complejidad tiene?

Definimos:

- n = cantidad de períodos que debemos procesar.
- k = cantidad de lotes de facturación que necesitamos consultar.

### Algoritmo actual: búsqueda repetida

Por cada período se realiza una consulta individual para encontrar su lote.

Desde el punto de vista del algoritmo, se realizan n búsquedas.

Si cada búsqueda utiliza un índice de Oracle, su costo de búsqueda puede aproximarse a O(log M), donde M representa la cantidad de registros de la tabla de lotes.

Sin embargo, cada búsqueda implica también una consulta remota y una apertura de conexión.

Por eso, el costo total no depende solamente de las comparaciones: también depende de los viajes a la base de datos.

**Complejidad:** O(n) búsquedas remotas, con un costo adicional de acceso a Oracle por cada búsqueda.

### Candidato A: Dictionary (tabla hash)

La propuesta consiste en traer los lotes necesarios en una sola consulta y organizarlos en un Dictionary de C#.

Después, por cada período buscamos su lote utilizando una clave compuesta:

`(IdRuta, Anio, Mes)`

Cada búsqueda se realiza mediante `TryGetValue`.

- Construir el diccionario: O(k) promedio.
- Recorrer los períodos: O(n) promedio.
- Buscar cada lote: O(1) promedio.

**Complejidad total promedio: O(n + k).**

En el peor caso teórico, las colisiones de hash pueden degradar el rendimiento hasta O(n × k).

### Candidato B: ordenamiento y búsqueda binaria

Otra posibilidad es traer los lotes en una sola consulta, ordenarlos y luego buscar cada uno mediante búsqueda binaria.

- Ordenar los lotes: O(k log k).
- Buscar cada período: O(log k).
- Repetir la búsqueda para n períodos.

**Complejidad total: O(k log k + n log k).**

La búsqueda binaria requiere que los datos estén previamente ordenados.

## 6. Comparación de alternativas

| Criterio | Actual | Candidato A | Candidato B |
|---|---|---|---|
| Estrategia | Buscar uno por uno en Oracle | Dictionary | Búsqueda binaria |
| Accesos adicionales a Oracle | n | 1 en bloque | 1 en bloque |
| Complejidad promedio en memoria | Depende de cada búsqueda remota | O(n + k) | O(k log k + n log k) |
| Peor caso | Depende del acceso a BD | O(n × k) | O(k log k + n log k) |
| Memoria auxiliar | Baja | O(k) | O(k) |
| Ventaja principal | Implementación sencilla | Búsqueda rápida por clave exacta | Peor caso garantizado eficiente |
| Desventaja principal | Muchas consultas remotas | Posibles colisiones de hash | Necesita ordenar previamente |

## 7. Nuestra elección

**Elegimos el Candidato A: precarga de datos e indexación mediante Dictionary.**

¿Por qué?

1. La búsqueda que necesitamos es por igualdad exacta de tres valores: ruta, año y mes.
2. La investigación encontró que la combinación de esos campos no presenta duplicados en los 445.909 registros históricos revisados de FAC_LOTES.
3. No necesitamos ordenar los lotes para realizar estas búsquedas.
4. El Dictionary permite encontrar cada lote en tiempo O(1) promedio.
5. El trabajo total crece linealmente: O(n + k) promedio.
6. Evitamos realizar una consulta individual a Oracle por cada período.

El Candidato B también es eficiente, pero introduce un ordenamiento previo que no necesitamos para este tipo de búsqueda.

La elección no se basa únicamente en cuál tiene menor complejidad, sino en cuál responde mejor a la operación que realmente necesita nuestro sistema.

## 8. ¿Cómo comprobaremos la mejora?

Una vez que implementemos la alternativa elegida, realizaremos pruebas en un entorno local o de pruebas, utilizando los mismos datos.

Compararemos:

- Tiempo de respuesta antes y después.
- Cantidad de consultas SQL.
- Comportamiento con 30, 52, 1.000 y mayores cantidades de registros.
- Consumo de memoria.

La expectativa es reducir las consultas adicionales de n a una consulta en bloque, manteniendo solamente las consultas principales de conteo y listado.

La reducción de tiempo deberá comprobarse experimentalmente. Todavía no es un resultado medido.

## 9. Conclusión

El análisis de Lecturas AP nos permitió comprobar que un proceso lento no necesariamente se debe a tener demasiados datos, sino también a la manera en que los procesamos.

Encontramos que el sistema realizaba búsquedas repetidas en Oracle para cada período, generando múltiples consultas y conexiones consecutivas.

Aplicando los conceptos de complejidad algorítmica, comparamos dos alternativas: ordenamiento con búsqueda binaria e indexación mediante Dictionary.

Elegimos Dictionary porque nuestra necesidad es buscar por igualdad exacta, tiene un costo promedio O(n + k) y evita búsquedas remotas repetidas.

**La principal lección de esta práctica es que debemos analizar el costo de un algoritmo antes de ejecutarlo y elegir la solución considerando los datos reales del problema.**