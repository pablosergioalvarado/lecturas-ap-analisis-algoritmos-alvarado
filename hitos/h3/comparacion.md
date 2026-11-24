# H3 — Comparación Experimental: Fuerza Bruta vs. Indexación (`Dictionary`)

**Caso integrador:** Asociación de períodos de lectura (`PeriodoLecturasAp`) con registros de `FACLOTES` mediante la clave compuesta `(IdRuta, Anio, Nroper)` en el proceso `GET /api/AlumbradoPublico/periodos`.

---

## 1. Descripción del problema

En el módulo de Alumbrado Público, al listar períodos de lectura (`GET /api/AlumbradoPublico/periodos`), cada período debe asociarse con su correspondiente registro comercial en `ERP_ELFEC.FAC_LOTES` para obtener la fecha de cierre del lote (`FechaHasta`). La correspondencia entre ambas entidades se determina mediante la clave compuesta de tres enteros:

```
(IdRuta, Anio, Nroper)  <==>  (Periodo.IdRuta, Periodo.Anio, Periodo.Mes)
```

Cuando se tienen $n$ períodos y $k$ registros de `FACLOTES` cargados en memoria, el problema consiste en encontrar de forma eficiente el lote exacto que corresponde a cada período sin realizar búsquedas repetitivas innecesarias.

---

## 2. Descripción de ambas estrategias

Se implementaron dos estrategias en [`hitos/h3/Comparacion.cs`](./Comparacion.cs) que resuelven exactamente la misma tarea sobre los mismos datos de entrada:

### A. Fuerza bruta (`AsociarFuerzaBruta`)
- Recorre secuencialmente la lista de $n$ períodos.
- Para cada período, realiza un recorrido lineal completo sobre la colección de $k$ registros de `FACLOTES` comparando `IdRuta`, `Anio` y `Nroper` hasta hallar la coincidencia exacta y extraer `FechaHasta`.
- No utiliza ninguna estructura auxiliar de indexación.

### B. Indexación con `Dictionary` (`AsociarConIndexacion`)
- **Fase 1 (Construcción del índice):** Recorre una sola vez los $k$ registros de `FACLOTES` e inserta cada uno en un `Dictionary<(int IdRuta, int Anio, int Nroper), FacLoteRegistro>` usando la clave compuesta.
- **Fase 2 (Consulta al índice):** Recorre los $n$ períodos y localiza su lote correspondiente mediante `TryGetValue((periodo.IdRuta, periodo.Anio, periodo.Mes), out var encontrado)`.
- **Inclusión del costo de indexación:** La construcción del `Dictionary` se mide **dentro** del tiempo cronometrado de la estrategia B porque en cada petición HTTP real el índice debe prepararse a partir de los lotes recuperados antes de ejecutar las búsquedas; omitir ese tiempo subestimaría el costo real del algoritmo.

---

## 3. Complejidad teórica

Sea $n$ la cantidad de períodos de lectura y $k$ la cantidad de registros de `FACLOTES`:

- **Fuerza bruta:** **$O(n \times k)$**
  - Por cada uno de los $n$ períodos, la búsqueda secuencial evalúa en promedio $k/2$ registros y en el peor caso $k$ registros. Cuando $k = n$, el costo total crece de forma cuadrática $O(n^2)$.
- **Indexación (`Dictionary`):** **$O(k + n)$ esperado**
  - Construir el índice insertando los $k$ lotes toma $O(k)$ esperado ($O(1)$ esperado por inserción).
  - Resolver las $n$ búsquedas posteriores en el diccionario toma $n \times O(1) = O(n)$ esperado.
  - En consecuencia, el costo total esperado es **$O(k + n)$** (lineal cuando $k = n$). En el peor caso teórico por colisiones totales de hash, el costo degenera a $O(k^2 + n \cdot k)$.

---

## 4. Protocolo de medición

- **Datos de prueba determinísticos:** La función `GenerarDatosDeterministicos(n)` genera $n$ períodos y $k = n$ registros de `FACLOTES` con correspondencia unívoca 1 a 1 mediante claves compuestas únicas `(IdRuta, Anio, Nroper)`. Para evitar aleatoriedad (`Random`) y garantizar resultados reproducibles, el orden de `FACLOTES` se permuta con una fórmula modular coprima determinística: `(i * 199 + 7) % n`. La generación de datos queda **fuera** del cronómetro.
- **Tamaños evaluados (duplicación progresiva):** $n = k \in \{500, 1000, 2000, 4000\}$.
- **Calentamiento previo:** Antes de medir, se ejecutan ambas estrategias sobre $250$ registros para que el compilador JIT compile los métodos en igualdad de condiciones.
- **Repeticiones y promedio:** Para cada tamaño y estrategia se fuerza la limpieza del recolector de basura (`GC.Collect()`), se mide con `Stopwatch` en **3 repeticiones independientes** y se calcula el tiempo promedio en milisegundos (`ms`).
- **Comando de ejecución desde la raíz del repositorio:**

```powershell
dotnet run --project hitos/h3/Comparacion.csproj -c Release
```

---

## 5. Resultados experimentales

Tabla generada a partir de la ejecución real en modo `Release` de [`hitos/h3/Comparacion.csproj`](./Comparacion.csproj):

| Tamaño ($n = k$) | Fuerza bruta (ms) | Factor respecto al anterior | Indexación (ms) | Factor respecto al anterior |
|---:|---:|---:|---:|---:|
| 500 | 0.1453 | — | 0.0788 | — |
| 1000 | 0.4134 | 2.85x | 0.1386 | 1.76x |
| 2000 | 1.5047 | 3.64x | 0.2948 | 2.13x |
| 4000 | 5.4735 | 3.64x | 0.6080 | 2.06x |

### Detalle de las 3 corridas por tamaño (ms)

- **$n = 500$:** Fuerza bruta `[0.1708, 0.1380, 0.1270]` (prom. `0.1453`) | Indexación `[0.0730, 0.0902, 0.0732]` (prom. `0.0788`)
- **$n = 1.000$:** Fuerza bruta `[0.4102, 0.4277, 0.4024]` (prom. `0.4134`) | Indexación `[0.1404, 0.1376, 0.1379]` (prom. `0.1386`)
- **$n = 2.000$:** Fuerza bruta `[1.5310, 1.4786, 1.5045]` (prom. `1.5047`) | Indexación `[0.2975, 0.2942, 0.2926]` (prom. `0.2948`)
- **$n = 4.000$:** Fuerza bruta `[5.6735, 5.4704, 5.2765]` (prom. `5.4735`) | Indexación `[0.6142, 0.6180, 0.5918]` (prom. `0.6080`)

---

## 6. Verificación de equivalencia

Antes de iniciar el cronómetro para el calentamiento ($n = 250$) y antes de medir cada tamaño ($n = 500, 1.000, 2.000, 4.000$), `VerificarResultados(...)` ejecutó ambas estrategias y validó automáticamente que:
1. La cantidad de asociaciones obtenidas fuera exactamente igual a $n$ en ambas implementaciones.
2. Todos los campos de cada asociación (`PeriodoId`, `IdLote`, `IdRuta`, `Anio`, `Mes` y `FechaHasta`) fueran estrictamente idénticos elemento por elemento.

Todas las verificaciones finalizaron con estado **IDÉNTICAS**, sin discrepancias.

---

## 7. Conclusión

Al duplicar el tamaño de entrada ($500 \to 1.000 \to 2.000 \to 4.000$), la estrategia de fuerza bruta incrementó su tiempo en factores de $2,85\times$, $3,64\times$ y $3,64\times$, aproximándose a un crecimiento cuadrático, mientras que la indexación con `Dictionary` mantuvo factores cercanos al doble ($1,76\times$, $2,13\times$ y $2,06\times$) aun incluyendo el costo de construir el índice, logrando ser alrededor de $9\times$ más rápida en $n = 4.000$ (`0,6080 ms` frente a `5,4735 ms`).
Las desviaciones respecto a los factores teóricos ideales ($4\times$ y $2\times$), especialmente en $n = 500$, se deben al ruido propio de las mediciones de corta duración, el formateo de fechas, el comportamiento de la caché de la máquina, el compilador JIT y la recolección de basura (GC).
Por estas limitaciones experimentales, los resultados empíricos respaldan de forma práctica la ventaja de la indexación frente a la fuerza bruta, pero no constituyen una demostración matemática de las cotas asintóticas.
