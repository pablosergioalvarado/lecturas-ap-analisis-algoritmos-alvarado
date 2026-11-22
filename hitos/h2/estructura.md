# H2 — Selección y Justificación de la Estructura de Datos

**Caso integrador:** Resolución de fechas de lote (`FacLoteFechaHasta`) durante el mapeo de períodos en `GET /api/AlumbradoPublico/periodos` (`FacLoteFechaResolver`).

---

## 1. Estructura de datos elegida

Para resolver en memoria la asociación entre los $n$ períodos (`PeriodoLecturasAp`) y los $k$ lotes de facturación (`FacLote`, con $k \le n$) recuperados en una sola consulta previa, elegimos una **Tabla Hash (HashMap)** implementada mediante **`Dictionary<(int Idruta, int Anio, int Nroper), string?>`** en C# (complementada con la lista secuencial `List<PeriodoLecturasApDto>` para almacenar los $n$ DTOs de salida respetando su orden cronológico).

### Por qué se ajusta a las operaciones más frecuentes de nuestro caso
Según el inventario de `inventario.md`, durante el mapeo de `GET /periodos`:
1. **La operación prioritaria y más frecuente es buscar por clave exacta (`(Idruta, Anio, Nroper)`):** Se ejecuta **$n$ veces por cada petición** ($30$ veces al consultar un suministro, $52$ veces al consultar el peor mes `2026/11` y hasta $1.000$ veces en una página máxima) para localizar el lote cuyo `(Idruta, Anio, Nroper)` coincida con `(Suministro.Idruta, Periodo.Anio, Periodo.Mes)`.
2. **Relación 1 a 1 sin duplicados:** En `ERP_ELFEC.FAC_LOTES` comprobamos que existen **$0$ claves `(IDRUTA, ANIO, NROPER)` duplicadas** en toda la tabla ($445.909$ filas), por lo que cada clave se asocia a un único valor `FechaHasta` formateado (`string?`).
3. **Ausencia de búsquedas por rango o de menor/siguiente sobre los lotes:** `FacLoteFechaResolver` solo consulta por igualdad exacta (`==`), nunca por intervalos de rutas o fechas, ni necesita recorrer los lotes `FacLote` en orden.

Una tabla hash indexada por la tupla de valor `(int Idruta, int Anio, int Nroper)` permite insertar los $k$ lotes en **$O(1)$ promedio por lote** ($O(k)$ total en promedio) y responder cada una de las $n$ búsquedas de `FacLoteFechaResolver` en **$O(1)$ promedio** ($O(n)$ para las $n$ búsquedas), logrando un costo total en memoria de **$O(k + n)$ en promedio** (que equivale a **$O(n)$** dado que $k \le n$).

---

## 2. Tabla de operaciones relevantes y su costo $O$

Comparamos las operaciones que realmente ejecuta nuestro proceso sobre las tres estructuras pertinentes vistas hasta las Clases 8 y 9:
1. **Alternativa ingenua (`List<FacLote>` / Arreglo desordenado con búsqueda lineal):** Almacena los $k$ lotes en una lista secuencial y los recorre linealmente en cada período.
2. **Alternativa con Árbol Binario de Búsqueda (Clase 9 — BST / Árbol balanceado `SortedDictionary`):** Indexa los $k$ lotes ordenados por la clave `(Idruta, Anio, Nroper)`.
3. **Estructura elegida (`Dictionary<(int Idruta, int Anio, int Nroper), string?>` — Tabla Hash):** Indexa los $k$ lotes mediante una función de dispersión (*hash*).

| Operación en nuestro proceso (`n` períodos, `k` lotes con $k \le n$) | Alternativa Ingenua: Lista / Arreglo sin índice (`List<FacLote>`) | Alternativa Clase 9: Árbol Binario de Búsqueda (BST sin balance / Balanceado) | Estructura Elegida: Tabla Hash (`Dictionary<(int, int, int), string?>`) |
|---|---|---|---|
| **1. Insertar un lote `FacLote` al construir la estructura ($1$ vez por lote)** | **$O(1)$ amortizado** al final ($O(k)$ en el peor caso cuando redimensiona el arreglo interno). | - **BST sin balance:** $O(\log k)$ promedio, pero **$O(k)$ en el peor caso** si los lotes llegan ordenados.<br>- **BST autobalanceado:** **$O(\log k)$** en promedio y peor caso. | **$O(1)$ promedio**.<br>**$O(k)$ en el peor caso** (si todas las claves colisionan en el mismo bucket o al redimensionar). |
| **2. Construir la estructura completa con los $k$ lotes ($k$ inserciones)** | **$O(k)$** | - **BST sin balance:** $O(k \log k)$ promedio, **$O(k^2)$ peor caso**.<br>- **BST autobalanceado:** **$O(k \log k)$** en promedio y peor caso. | **$O(k)$ en promedio**.<br>**$O(k^2)$ en el peor caso** (colisión total en todas las inserciones). |
| **3. Buscar `FechaHasta` por clave exacta `(Idruta, Anio, Nroper)` ($1$ búsqueda)** | **$O(k)$** en caso promedio y peor caso (búsqueda lineal elemento por elemento; $O(1)$ solo en el mejor caso si está en la posición 0). | - **BST sin balance:** $O(\log k)$ promedio, pero **$O(k)$ en el peor caso** (árbol degenerado en lista).<br>- **BST autobalanceado:** **$O(\log k)$** en promedio y peor caso. | **$O(1)$ en promedio** (bajo distribución uniforme y factor de carga acotado).<br>**$O(k)$ en el peor caso** (si todas las $k$ claves cayeron en el mismo bucket). |
| **4. Resolver las $n$ búsquedas del mapeo ($n$ ejecuciones de la búsqueda)** | **$O(n \cdot k)$** en promedio y peor caso (equivale a **$O(n^2)$** cuando $k = n$). | - **BST sin balance:** $O(n \log k)$ promedio, **$O(n \cdot k)$ peor caso**.<br>- **BST autobalanceado:** **$O(n \log k)$** en promedio y peor caso. | **$O(n)$ en promedio** ($n \times O(1)$).<br>**$O(n \cdot k)$ en el peor caso** (colisión total de hash). |
| **5. Costo total del cruce en memoria (Construcción + $n$ búsquedas)** | **$O(k + n \cdot k) = O(n \cdot k)$** | - **BST sin balance:** $O((n + k) \log k)$ promedio, **$O(k^2 + n \cdot k)$ peor caso**.<br>- **BST autobalanceado:** **$O((n + k) \log k)$** en promedio y peor caso. | **$O(k + n)$ en promedio** (lineal).<br>**$O(k^2 + n \cdot k) = O(n \cdot k)$ en el peor caso**. |
| **6. Agregar, recorrer y ordenar los $n$ DTOs de salida por `(Anio DESC, Mes DESC)`** | Se usa `List<PeriodoLecturasApDto>`: **$O(1)$ amortizado** al insertar al final, **$O(n)$** de recorrido y **$O(n \log n)$** al ordenar por comparación (o $O(n)$ si se preserva el orden SQL). | No se usa para la lista de salida (solo se evalúa como índice de lotes). | Para la salida se conserva `List<PeriodoLecturasApDto>` ($O(n)$ recorrido / $O(n \log n)$ ordenamiento), ya que la tabla hash no mantiene orden. |
| **7. Borrar, obtener menor/siguiente y consultar rangos sobre lotes** | **No aplican en el proceso** ($O(k)$ si se ejecutaran). | **No aplican en el proceso** ($O(\log k)$ o $O(\log k + m)$ en BST balanceado; ventaja desaprovechada en este caso). | **No aplican en el proceso** ($O(1)$ promedio para borrar; $O(k)$ para menor/siguiente o rango). |

---

## 3. Comparación concreta frente a la alternativa ingenua y a los árboles de búsqueda

### 3.1. Tabla Hash (`Dictionary`) vs. Alternativa Ingenua (`List<FacLote>`)
- **En la solución ingenua (`List<FacLote>`):** Aunque se traigan los $k$ lotes de Oracle en una sola consulta y se guarden en un `List<FacLote>`, al no tener un índice en memoria por la clave `(Idruta, Anio, Nroper)`, cada una de las $n$ llamadas del mapper debe recorrer linealmente la lista comparando `Idruta`, `Anio` y `Nroper` hasta hallar la coincidencia. En promedio recorre $k/2$ elementos por período y en el peor caso $k$ elementos, realizando **$O(n \cdot k)$ comparaciones** (que crece cuadráticamente como **$O(n^2)$** cuando $k$ crece proporcionalmente a $n$: al duplicar $n$ y $k$, el trabajo se cuadruplica).
- **En la estructura elegida (`Dictionary`):** Primero se recorren una sola vez los $k$ lotes para insertarlos en la tabla hash ($O(k)$ promedio). Luego, al recorrer los $n$ períodos, cada consulta `TryGetValue((idRuta, anio, mes), out fecha)` calcula el código hash de la tupla de 3 enteros y accede directamente al bucket correspondiente en **$O(1)$ promedio**. Al duplicar $n$ y $k$, el trabajo total crece solo linealmente (**$2\times$** en lugar de **$4\times$**).

### 3.2. Por qué no elegimos un Árbol Binario de Búsqueda (Clase 9)
- Un **BST sin balancear** es riesgoso en nuestro caso real porque `FacLoteSpecification` (`Core/Specifications/FacLotes/FacLoteSpecification.cs`, línea 16) incluye `AddOrderBy(x => x.Idruta)`: al insertar lotes que ya vienen ordenados desde Oracle, el BST sin balance se degenera en una cadena de altura $k$, degradando la búsqueda a **$O(k)$** (igual que la lista ingenua).
- Un **BST autobalanceado** (como `SortedDictionary` en .NET, basado en un árbol Rojo-Negro) evita esa degeneración y garantiza **$O(\log k)$ en el peor caso**, además de permitir recorridos ordenados, obtener el menor/siguiente o consultar rangos en $O(\log k)$. Sin embargo, en `FacLoteFechaResolver` **nunca pedimos rangos ni el lote siguiente/anterior**: solo necesitamos búsqueda por clave exacta sobre los lotes. Por ello, usar una tabla hash (`Dictionary`) evita las $\sim \log_2(k)$ comparaciones de nodos y saltos de punteros por período, reduciendo la búsqueda al caso promedio **$O(1)$**.

---

## 4. Justificación breve según el patrón real de uso

En `GET /api/AlumbradoPublico/periodos`, la estructura de lotes se construye al inicio del mapeo de la petición ($k = 30$ lotes por suministro, $k = 51$ lotes en un mes completo y hasta $k \le 1.000$ lotes en una página máxima), recibe inmediatamente **$n$ búsquedas por clave exacta `(Idruta, Anio, Nroper)`** sin ninguna eliminación ni consulta de rango, y se descarta al finalizar la respuesta HTTP. Para este patrón de **una sola fase de carga ($k$ inserciones) seguida de $n$ búsquedas exactas por clave de tres enteros sin duplicados**, la combinación de:
1. **`Dictionary<(int Idruta, int Anio, int Nroper), string?>`** para indexar y buscar los $k$ lotes en **$O(k + n)$ promedio**, y
2. **`List<PeriodoLecturasApDto>`** (arreglo dinámico predimensionado con capacidad $n$) para almacenar y devolver los $n$ períodos manteniendo el orden `(Anio DESC, Mes DESC)` en **$O(n)$**,

es la elección más simple, directa y asintóticamente adecuada para nuestro caso integrador.
