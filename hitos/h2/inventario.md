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

## 3. Estructuras de datos identificadas (Clases 8 y 9)

| Estructura | Uso en el proceso | Observación |
|---|---|---|
| Colección `IReadOnlyList` / `List<T>` | Almacenar períodos y DTOs | Colección secuencial en memoria, respaldada por un arreglo dinámico. |
| Arreglo | Alternativa para almacenar lotes | Búsqueda por clave $O(k)$ si no está ordenado por esa clave. |
| Lista enlazada | Alternativa estudiada (Clase 8) | Búsqueda $O(k)$; no aprovechamos su inserción o eliminación intermedia $O(1)$. |
| Tabla hash (`Dictionary`) | Alternativa para buscar lotes por clave (Clase 8) | Búsqueda e inserción $O(1)$ promedio y $O(k)$ peor caso por colisiones. No mantiene orden. |
| Árbol binario de búsqueda (BST / BST balanceado) | Alternativa ordenada por clave (Clase 9) | Un BST balanceado garantiza $O(\log k)$ en búsqueda, inserción, borrado y menor/siguiente; sin balancear degenera a $O(k)$ en el peor caso (p. ej., si los lotes llegan ordenados por `Idruta`). |

## 4. Operaciones relevantes y comparación de costos

| Operación | Aplica al cruce de `FacLote` | Frecuencia | Arreglo (sin orden) | Lista enlazada | Tabla hash (`Dictionary`) | BST sin balance | BST balanceado |
|---|---|---:|---|---|---|---|---|
| **Buscar `FacLote` por clave `(Idruta, Anio, Nroper)`** | **Sí (dominante)** | $n$ veces | $O(k)$ | $O(k)$ | **$O(1)$ prom.** / $O(k)$ peor | $O(\log k)$ prom. / $O(k)$ peor | $O(\log k)$ |
| **Insertar un lote en memoria** (se ejecuta $k$ veces) | **Sí** | $k$ veces | $O(1)$ amort. al final | $O(1)$ con ref. | **$O(1)$ prom.** / $O(k)$ peor | $O(\log k)$ prom. / $O(k)$ peor | $O(\log k)$ |
| **Recorrer períodos y generar DTOs** | **Sí (sobre salida)** | $n$ elementos | $O(n)$ | $O(n)$ | $O(n)$ sin orden | $O(n)$ in-order | $O(n)$ in-order |
| **Ordenar / recorrer DTOs en orden `(Anio DESC, Mes DESC)`** | **Sí (sobre salida)** | $1$ vez | $O(n \log n)$ al ordenar; $O(n)$ si ya viene ordenado | $O(n \log n)$ | No mantiene orden | $O(n)$ in-order | $O(n)$ in-order |
| **Borrar elementos** | **No aplica** (consulta de solo lectura) | $0$ | $O(k)$ | $O(1)$ con ref. + $O(k)$ buscar | $O(1)$ prom. / $O(k)$ peor | $O(\log k)$ prom. / $O(k)$ peor | $O(\log k)$ |
| **Obtener menor, mayor o siguiente** | **No aplica** (solo se busca el lote exacto) | $0$ | $O(k)$ | $O(k)$ | $O(k)$ | $O(\log k)$ prom. / $O(k)$ peor | $O(\log k)$ |
| **Consultar un rango de claves** | **No aplica** (filtros por igualdad `==`) | $0$ | $O(k)$ | $O(k)$ | $O(k)$ | $O(\log k + m)$ prom. / $O(k)$ peor | $O(\log k + m)$ |

*(Nota: $m$ es la cantidad de elementos dentro del rango consultado. Para construir toda la estructura con los $k$ lotes y resolver las $n$ búsquedas del mapeo, el costo total en memoria es $O(k + n \cdot k) = O(n \cdot k)$ con arreglo desordenado o lista enlazada, $O((k + n) \log k)$ con BST balanceado y **$O(k + n)$ en promedio** con tabla hash).*

## 5. Algoritmos, complejidad y limitaciones actuales

- **Limitación actual:** `GetPeriodosAsync` almacena los $n$ períodos en una lista secuencial (`List<PeriodoLecturasAp>`), pero `FacLoteFechaResolver` no utiliza ninguna estructura en memoria para los lotes: en cada iteración consulta individualmente la base de datos (`SingleOrDefaultWithSpec`) y descarta el `FacLote` obtenido.
- **Búsqueda lineal (`List<FacLote>` / arreglo o lista enlazada):** $O(k)$ por período $\to O(n \cdot k)$ total. Recorre los lotes uno por uno por carecer de índice por clave.
- **Búsqueda en árbol (BST / BST balanceado):** $O(\log k)$ por período en un BST balanceado ($\to O((k + n) \log k)$ total), pero $O(k)$ en el peor caso si el BST no está balanceado y los lotes ingresan ordenados.
- **Búsqueda hash (`Dictionary`):** $O(1)$ promedio por período ($\to O(k + n)$ promedio total), con peor caso $O(k)$ por búsqueda si ocurren colisiones.
- **Recorrido secuencial y ordenamiento de salida:** $O(n)$ para recorrer los períodos y generar los DTOs, y $O(n \log n)$ al ordenar por año y mes descendentes.

*H1 identificó el problema de las búsquedas repetidas; H2 analiza las estructuras en memoria que permiten resolver ese cruce eficientemente.*

## 6. Conclusión

El proceso utiliza colecciones secuenciales para manejar los períodos y construir los DTOs, siendo la búsqueda repetida de `FacLote` por la clave `(Idruta, Anio, Nroper)` ($n$ veces) la operación crítica para el rendimiento.

Un arreglo desordenado o una lista enlazada requieren búsqueda lineal $O(k)$ por período ($O(n \cdot k)$ total), y la lista enlazada no aporta ventajas porque el proceso no realiza eliminaciones ni inserciones intermedias. Un BST balanceado reduce cada búsqueda a $O(\log k)$ y permite operaciones de rango o menor/siguiente que nuestro cruce no utiliza, mientras que una tabla hash (`Dictionary`) resuelve la búsqueda por clave exacta en $O(1)$ promedio con un costo total esperado de $O(k + n)$.
