# Hito 2 — Inventario de datos y estructuras

## 1. Proceso analizado

Endpoint: `GET /api/AlumbradoPublico/periodos`

El problema identificado en H1 está en `FacLoteFechaResolver`: por cada período se realiza una búsqueda de su correspondiente `FacLote` para obtener `FechaHasta`.

La búsqueda utiliza la clave `(Idruta, Anio, Nroper)`.

## 2. Datos y volúmenes

| Dato | Estructura / origen | Volumen |
|---|---|---:|
| `PeriodoLecturasAp` | Registros Oracle y colección de períodos | 1.534 históricos |
| `Suministro` | Entidad relacionada | 52 suministros, 51 rutas |
| `FacLote` | Registros Oracle | 445.909 históricos |
| `FacLote` para el cruce | Colección de lotes necesarios | k = 30, 51 o hasta 1.000 |
| `PeriodoLecturasApDto` | Colección de salida | n = 30, 52 o hasta 1.000 |

**Volúmenes de referencia:**
- Suministro 136907: n = 30, k = 30.
- Noviembre 2026: n = 52, k = 51.
- Página máxima: n = 1.000, k ≤ 1.000.

`n` representa los períodos procesados; `k`, los lotes necesarios para resolverlos. No se cargan los 445.909 lotes históricos en memoria.

## 3. Estructuras de datos identificadas

| Estructura | Uso en el proceso | Observación |
|---|---|---|
| Colección `IReadOnlyList` / `List<T>` | Almacenar períodos y DTOs | Colección secuencial, similar a un arreglo dinámico. |
| Arreglo | Alternativa para almacenar lotes | Búsqueda por clave O(k) si no está ordenado. |
| Lista enlazada | Alternativa estudiada | Búsqueda O(k); no aprovechamos sus ventajas de inserción o eliminación intermedia. |
| Tabla hash (`Dictionary`) | Alternativa para buscar lotes por clave | Búsqueda O(1) promedio. No está implementada actualmente en este cruce. |

## 4. Operaciones relevantes

| Operación | Frecuencia | Complejidad / situación |
|---|---:|---|
| Buscar `FacLote` por clave | n veces | O(k) secuencial; O(1) promedio con hash. |
| Insertar lotes en memoria | k veces | O(k) total en las estructuras consideradas, en condiciones habituales. |
| Recorrer períodos y generar DTOs | n elementos | O(n). |
| Ordenar DTOs por año y mes | Una vez | O(n log n) habitual en ordenamiento por comparación. |
| Eliminar elementos | 0 | No aplica. |
| Encontrar mínimo/máximo | 0 | No aplica. |
| Consultar rangos en memoria | 0 | No aplica; se utilizan filtros por igualdad. |

## 5. Algoritmos y complejidad identificados

- **Búsqueda lineal:** O(k). Recorre los lotes uno por uno. Es el comportamiento de buscar por clave en una colección secuencial sin índice.
- **Búsqueda hash:** O(1) promedio. Utiliza una función hash para localizar el lote mediante su clave. Peor caso O(k) por colisiones.
- **Recorrido secuencial:** O(n). Procesa cada período para generar su DTO.
- **Ordenamiento:** O(n log n) habitual. Organiza los resultados por año y mes descendentes.

*H1 identificó el problema de las búsquedas repetidas; H2 analiza las estructuras que pueden mejorar ese procesamiento.*

## 6. Conclusión

El proceso utiliza colecciones secuenciales para manejar los períodos y construir los DTOs. La búsqueda de `FacLote` es la operación que merece mayor atención.

Un arreglo desordenado o una lista enlazada requieren búsqueda lineal O(k). Una tabla hash permite búsquedas O(1) promedio, con un costo esperado de O(k + n) para construirla y realizar las búsquedas.

La lista enlazada no aporta ventajas relevantes porque no necesitamos eliminar ni insertar elementos intermedios.

**Pendiente:** medir tiempos y memoria con n = 30, 52 y 1.000 para evaluar las estructuras antes de formalizar la elección.
