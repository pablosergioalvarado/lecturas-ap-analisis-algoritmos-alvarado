# Práctica C5 — Torneo de Algoritmos de Ordenamiento

**Asignatura:** Análisis de Algoritmos — Ingeniería de Sistemas (UAB)  
**Estudiante:** Pablo Sergio Alvarado Gutiérrez  
**Lenguaje y Entorno:** C# sobre .NET 8.0 (`net8.0`, configuración `Release`)  
**Repositorio:** `lecturas-ap-analisis-algoritmos-alvarado` (`practicas/c5`)

---

## 1. Objetivo del Experimento

Comparar experimentalmente el rendimiento y la tasa de crecimiento temporal de cuatro algoritmos de ordenamiento al procesar cantidades crecientes de datos ($n \in \{10.000, 20.000, 40.000, 80.000\}$), contrastando un algoritmo de complejidad cuadrática $O(n^2)$ (**Insertion Sort**) frente a dos algoritmos basados en la estrategia *Divide y Vencerás* de complejidad $O(n \log n)$ (**MergeSort** y **QuickSort**), utilizando además el método nativo **`Array.Sort`** de la plataforma .NET como referencia adicional.

---

## 2. Descripción de los Datos

- **Tipo de datos:** Arreglos unidimensionales de números enteros (`int[]`).
- **Generación reproducible:** Se emplea el generador pseudoaleatorio `new Random(2026)` con semilla fija `2026`. De este modo, para cada tamaño $n$, la secuencia generada es determinista y reproducible en cualquier ejecución.
- **Rango de valores:** Enteros pseudoaleatorios distribuidos uniformemente en el intervalo $[0, 999.999]$ (`random.Next(0, 1_000_000)`).
- **Tamaños evaluados ($n$):** $10.000$, $20.000$, $40.000$ y $80.000$ elementos (duplicando progresivamente el tamaño de la entrada: $n \to 2n \to 4n \to 8n$).
- **Aislamiento de la entrada:** Para cada tamaño $n$, el arreglo `datosOriginales` se genera una única vez y permanece inmutable. Cada algoritmo y cada corrida individual recibe una copia independiente obtenida mediante `(int[])datosOriginales.Clone()`.

---

## 3. Descripción de los Cuatro Competidores

1. **Insertion Sort (Ordenamiento por Inserción) — Competidor Cuadrático Obligatorio:**
   - Recorre el arreglo desde el índice `1` hasta `n - 1`. En cada paso toma el elemento `actual = arreglo[i]` y desplaza hacia la derecha todos los elementos mayores de la sublista izquierda `arreglo[0..i-1]` hasta insertar `actual` en su posición ordenada.
   - Opera *in-place* ($O(1)$ memoria adicional) y es un algoritmo *estable*, pero en datos desordenados requiere un número de comparaciones y desplazamientos cuadrático respecto a $n$: **$O(n^2)$**.

2. **MergeSort (Ordenamiento por Mezcla) — Competidor $O(n \log n)$ Garantizado:**
   - Aplica el paradigma *Divide y Vencerás* en tres etapas:
     1. **Dividir:** Calcula el punto medio `medio = inicio + (fin - inicio) / 2` (fórmula que previene desbordamiento de enteros) y parte el segmento en dos mitades.
     2. **Resolver:** Ordena recursivamente la mitad izquierda `[inicio..medio]` y la mitad derecha `[medio + 1..fin]`.
     3. **Combinar (`Mezclar`):** Intercala ambas mitades ordenadas comparando sus elementos frontales y escribiendo el menor en un arreglo `auxiliar`, copiando finalmente el tramo ordenado de vuelta al arreglo principal.
   - Garantiza complejidad temporal **$O(n \log n)$** en todos los casos (mejor, promedio y peor) y es *estable*, a cambio de requerir **$O(n)$** de memoria auxiliar.

3. **QuickSort (Ordenamiento Rápido) — Competidor $O(n \log n)$ Promedio In-Place:**
   - Aplica *Divide y Vencerás* mediante particionamiento en el lugar (*in-place*):
     1. **Selección del pivote y partición:** Utiliza el esquema de partición de Lomuto seleccionando el **último elemento del segmento (`arreglo[fin]`) como pivote**. Recorre el subarreglo ubicando los elementos estrictamente menores que el pivote a la izquierda del índice frontera `i` y los mayores o iguales a la derecha, colocando finalmente el pivote en su posición definitiva `i`.
     2. **Resolver:** Invoca recursivamente `Rapido` sobre las subparticiones izquierda `[inicio..i - 1]` y derecha `[i + 1..fin]`.
   - En el caso promedio (como ocurre con datos generados aleatoriamente), las particiones son equilibradas y alcanza **$O(n \log n)$** con excelente localidad de caché y sin requerir arreglos auxiliares. Sin embargo, si el arreglo de entrada ya estuviera ordenado o inversamente ordenado, elegir `arreglo[fin]` produciría particiones degeneradas de tamaño $n - 1$ y $0$, degradando el tiempo a **$O(n^2)$** y la profundidad de la pila recursiva a **$O(n)$**.

4. **`Array.Sort` — Referencia Adicional Nativa de .NET:**
   - Implementación industrial provista por la biblioteca estándar de .NET, basada en **IntroSort** (*Introspective Sort*): un algoritmo híbrido que combina `QuickSort` (con selección de pivote por mediana de tres), `Insertion Sort` (para umbrales de partición pequeños $\le 16$ elementos) y `HeapSort` (al cual conmuta si la profundidad de recursión excede $2\lfloor\log_2 n\rfloor$ para garantizar $O(n \log n)$ incluso en el peor caso). Se incluye como referencia comparativa y no sustituye a los tres algoritmos implementados manualmente.

---

## 4. Metodología Experimental

El protocolo implementado en [Program.cs](Program.cs) sigue trece reglas metodológicas diseñadas para asegurar reproducibilidad y equidad entre competidores:

1. **Calentamiento global previo (`RealizarCalentamientoGlobalJit`):** Antes de iniciar la tabla del torneo, se ejecutan los cuatro algoritmos sobre datos representativos ($n = 10.000$ durante 10 iteraciones) seguidos de una breve pausa (`Thread.Sleep(250)`) y recolección de basura. Esto permite que el compilador JIT por niveles (*Tiered Compilation*) de .NET 8 promueva los métodos al nivel optimizado (*Tier 1*) antes de cronometrar el primer tamaño.
2. **Calentamiento específico por algoritmo y tamaño:** Dentro de `MedirPromedio`, cada algoritmo realiza una ejecución de calentamiento sobre una copia del arreglo del tamaño actual antes de las tres corridas medidas.
3. **Mismos datos originales por tamaño:** Todos los algoritmos reciben copias exactas del mismo arreglo `datosOriginales` generado con `Random(2026)`.
4. **Copia independiente en cada corrida:** Cada ejecución opera sobre `(int[])datosOriginales.Clone()`.
5. **Tres mediciones independientes (`C1`, `C2`, `C3`):** Se registran e imprimen los tres tiempos individuales por algoritmo y tamaño.
6. **Cálculo del promedio:** Se calcula la media aritmética $\text{Promedio} = (C_1 + C_2 + C_3) / 3$.
7. **Exclusión de generación y copia del cronómetro:** Tanto `GenerarDatos` como `Clone()` se ejecutan antes de `Stopwatch.StartNew()`.
8. **Exclusión de las validaciones del cronómetro:** Todas las verificaciones se ejecutan después de `cronometro.Stop()`.
9. **Asignaciones propias dentro de la medición:** La reserva `int[] auxiliar = new int[arreglo.Length];` de `MergeSort` se mantiene dentro de `MergeSort(arreglo)`, pues forma parte del costo espacial y temporal real del algoritmo.
10. **Cronometraje de alta resolución:** Se emplea `Stopwatch` y `cronometro.Elapsed.TotalMilliseconds`.
11. **Sin recolección de basura dentro del cronómetro:** Nunca se invoca `GC` durante el intervalo medido.
12. **Homogeneización previa de memoria (`GC.Collect` antes de medir):** Antes de iniciar `Stopwatch.StartNew()` en cada corrida, se ejecuta `GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();` para reducir la probabilidad de que objetos huérfanos de una corrida previa (como el arreglo `auxiliar` o clones anteriores) disparen una pausa de recolección a mitad de la corrida siguiente.
13. **Triple verificación de corrección tras cada corrida:**
    - `VerificarOrdenamiento`: Comprueba que `arreglo[i - 1] <= arreglo[i]` en todo el arreglo.
    - `VerificarEquivalencia`: Comprueba elemento por elemento que la salida sea idéntica a `referenciaOrdenada`.
    - `VerificarIntegridadOriginales`: Comprueba que `datosOriginales` no haya sido modificado.

> **Nota metodológica sobre las limitaciones del entorno:** Aunque el calentamiento global del JIT y la limpieza previa del recolector de basura reducen sustancialmente el ruido experimental, las mediciones con `Stopwatch` registran tiempo de reloj (*wall-clock time*) sobre un sistema operativo multitarea (Windows). Por tanto, las mediciones **no están completamente libres de interferencias externas** tales como cambios de contexto del planificador de hilos del sistema operativo, variaciones térmicas o de frecuencia del procesador (*Turbo Boost* / DVFS) o actividad en segundo plano.

---

## 5. Código y Explicación de las Implementaciones

El código fuente íntegro se encuentra en [Program.cs](Program.cs). A continuación se destacan los tres algoritmos implementados manualmente y su verificación:

```csharp
// 1. INSERTION SORT: O(n²) temporal, O(1) espacial.
private static void InsertionSort(int[] arreglo)
{
    for (int i = 1; i < arreglo.Length; i++)
    {
        int actual = arreglo[i];
        int j = i - 1;

        while (j >= 0 && arreglo[j] > actual)
        {
            arreglo[j + 1] = arreglo[j];
            j--;
        }

        arreglo[j + 1] = actual;
    }
}

// 2. MERGESORT: O(n log n) temporal en todos los casos, O(n) espacial.
private static void MergeSort(int[] arreglo)
{
    int[] auxiliar = new int[arreglo.Length];
    DividirYOrdenar(arreglo, auxiliar, 0, arreglo.Length - 1);
}

private static void DividirYOrdenar(int[] arreglo, int[] auxiliar, int inicio, int fin)
{
    if (inicio >= fin)
        return;

    int medio = inicio + (fin - inicio) / 2;
    DividirYOrdenar(arreglo, auxiliar, inicio, medio);
    DividirYOrdenar(arreglo, auxiliar, medio + 1, fin);
    Mezclar(arreglo, auxiliar, inicio, medio, fin);
}

// 3. QUICKSORT: O(n log n) promedio, O(n²) peor caso, pivote en arreglo[fin].
private static void QuickSort(int[] arreglo)
{
    Rapido(arreglo, 0, arreglo.Length - 1);
}

private static void Rapido(int[] arreglo, int inicio, int fin)
{
    if (inicio >= fin)
        return;

    int pivote = arreglo[fin];
    int i = inicio;

    for (int j = inicio; j < fin; j++)
    {
        if (arreglo[j] < pivote)
        {
            (arreglo[i], arreglo[j]) = (arreglo[j], arreglo[i]);
            i++;
        }
    }

    (arreglo[i], arreglo[fin]) = (arreglo[fin], arreglo[i]);

    Rapido(arreglo, inicio, i - 1);
    Rapido(arreglo, i + 1, fin);
}
```

### Verificación de ausencia de errores de índices y recursión
- En **`InsertionSort`**, el bucle externo inicia en `i = 1` y el bucle interno verifica `j >= 0` antes de evaluar `arreglo[j] > actual`, escribiendo en `arreglo[j + 1]` (que como máximo es `i < arreglo.Length`), por lo que no puede desbordar los límites.
- En **`MergeSort`**, el caso base `if (inicio >= fin) return;` detiene la recursión en subarreglos de longitud $\le 1$. El cálculo `medio = inicio + (fin - inicio) / 2` garantiza que `inicio <= medio < fin` siempre que `inicio < fin`, asegurando que ambas llamadas recursivas reduzcan estrictamente el tamaño del subproblema y evitando desbordamiento aritmético.
- En **`QuickSort`**, el caso base `if (inicio >= fin) return;` detiene la recursión cuando la partición tiene 0 o 1 elementos (incluyendo cuando `i == inicio`, donde la llamada izquierda recibe `inicio` y `inicio - 1` y retorna inmediatamente). Tras colocar el pivote en `arreglo[i]`, el índice `i` queda excluido de ambas llamadas recursivas (`[inicio..i-1]` e `[i+1..fin]`), garantizando terminación finita.

---

## 6. Resultados Históricos Originales

Se preserva intacta en el repositorio la captura histórica [Tabla de Mediciones.PNG](Tabla%20de%20Mediciones.PNG), cuyos valores promedio registrados fueron:

| $n$ | Insertion Sort (ms) | MergeSort (ms) | QuickSort (ms) | Array.Sort (ms) |
|---:|---:|---:|---:|---:|
| **10.000** | 110,392 | 19,005 | 41,608 | 4,404 |
| **20.000** | 328,900 | 3,986 | 2,804 | 2,691 |
| **40.000** | 884,671 | 8,031 | 4,722 | 3,697 |
| **80.000** | 3.463,199 | 33,489 | 21,307 | 10,560 |

### Diferenciación entre Observaciones Directas e Hipótesis Técnicas sobre la Tabla Histórica

- **Observaciones verificables en los datos y el código original:**
  1. En la captura histórica, al duplicar $n$ de $10.000$ a $20.000$, los tiempos promedio registrados para `MergeSort` ($19,005\text{ ms} \to 3,986\text{ ms}$), `QuickSort` ($41,608\text{ ms} \to 2,804\text{ ms}$) y `Array.Sort` ($4,404\text{ ms} \to 2,691\text{ ms}$) disminuyeron en lugar de aumentar.
  2. La inspección del código fuente original confirmó que **sí se clonaba el arreglo antes de cada corrida** (`datosOriginales.Clone()`), que el cronómetro excluía la copia y la validación (`VerificarOrdenamiento`), y que la conversión a milisegundos (`TotalMilliseconds`) era correcta.
  3. También se observa en el código original que no existía una fase de calentamiento global previa al primer tamaño ($n = 10.000$) ni llamadas a `GC.Collect()` entre corridas, y que solo se imprimía el promedio sin mostrar `C1`, `C2` y `C3`.

- **Hipótesis técnicas plausibles sobre el origen de la anomalía en $n = 10.000$:**
  1. *Hipótesis de compilación JIT por niveles (Tiered Compilation en .NET 8):* En .NET 8, los métodos se compilan inicialmente en un nivel rápido no optimizado (*Tier 0*) y son recompilados en segundo plano a código nativo optimizado (*Tier 1*) tras alcanzar un umbral de ejecuciones. Es plausible que las corridas de $n = 10.000$ hayan coincidido con ese proceso de recompilación y que, al llegar a $n = 20.000$, los métodos ya se ejecutaran íntegramente en *Tier 1*.
  2. *Hipótesis de recolección de basura (GC) y arranque del proceso:* La memoria reservada durante las corridas de `Insertion Sort` y los arreglos `auxiliar` de `MergeSort` en $n = 10.000$ pudieron activar recolecciones automáticas del *Garbage Collector* o coincidir con fluctuaciones transitorias de carga del sistema operativo y escalado de frecuencia de la CPU tras compilar el proyecto.

---

## 7. Resultados Experimentales Nuevos (`dotnet run -c Release`)

Al ejecutar el protocolo mejorado en modo `Release`, se obtuvieron las siguientes mediciones verificables:

### Tabla 1: Tiempos Individuales (`C1`, `C2`, `C3`) y Promedio por Algoritmo y Tamaño

| $n$ | Algoritmo | C1 | C2 | C3 | Promedio |
|---:|---|---:|---:|---:|---:|
| **10.000** | Insertion Sort | 66,960 ms | 52,911 ms | 55,246 ms | **58,372 ms** |
| **10.000** | MergeSort | 1,668 ms | 1,546 ms | 1,549 ms | **1,588 ms** |
| **10.000** | QuickSort | 1,105 ms | 1,472 ms | 1,049 ms | **1,209 ms** |
| **10.000** | Array.Sort | 0,806 ms | 0,792 ms | 0,847 ms | **0,815 ms** |
| **20.000** | Insertion Sort | 187,162 ms | 277,008 ms | 279,141 ms | **247,770 ms** |
| **20.000** | MergeSort | 6,789 ms | 3,267 ms | 4,623 ms | **4,893 ms** |
| **20.000** | QuickSort | 2,289 ms | 2,110 ms | 2,017 ms | **2,139 ms** |
| **20.000** | Array.Sort | 1,840 ms | 1,897 ms | 1,801 ms | **1,846 ms** |
| **40.000** | Insertion Sort | 956,241 ms | 757,570 ms | 735,032 ms | **816,281 ms** |
| **40.000** | MergeSort | 10,827 ms | 10,340 ms | 7,326 ms | **9,498 ms** |
| **40.000** | QuickSort | 5,384 ms | 7,951 ms | 5,552 ms | **6,296 ms** |
| **40.000** | Array.Sort | 6,034 ms | 4,294 ms | 10,620 ms | **6,982 ms** |
| **80.000** | Insertion Sort | 1.908,586 ms | 1.984,751 ms | 2.007,599 ms | **1.966,979 ms** |
| **80.000** | MergeSort | 14,868 ms | 15,105 ms | 10,811 ms | **13,595 ms** |
| **80.000** | QuickSort | 7,123 ms | 7,651 ms | 7,590 ms | **7,455 ms** |
| **80.000** | Array.Sort | 6,215 ms | 7,743 ms | 6,049 ms | **6,669 ms** |

### Tabla 2: Resumen Comparativo de Promedios (ms)

| $n$ | Insertion Sort | MergeSort | QuickSort | Array.Sort |
|---:|---:|---:|---:|---:|
| **10.000** | 58,372 ms | 1,588 ms | 1,209 ms | 0,815 ms |
| **20.000** | 247,770 ms | 4,893 ms | 2,139 ms | 1,846 ms |
| **40.000** | 816,281 ms | 9,498 ms | 6,296 ms | 6,982 ms |
| **80.000** | 1.966,979 ms | 13,595 ms | 7,455 ms | 6,669 ms |

---

## 8. Comparación de Tiempos Individuales y Promedios

El registro explícito de `C1`, `C2` y `C3` permite observar por qué en ingeniería experimental no basta con una sola medición aislada:
- En la mayoría de los casos (por ejemplo, `QuickSort` en $n = 20.000$ con $2,289\text{ ms}$, $2,110\text{ ms}$ y $2,017\text{ ms}$, o `MergeSort` en $n = 10.000$ con $1,668\text{ ms}$, $1,546\text{ ms}$ y $1,549\text{ ms}$), las tres corridas son altamente consistentes entre sí.
- Sin embargo, en corridas puntuales (como `C3` de `Array.Sort` en $n = 40.000$, que subió momentáneamente a $10,620\text{ ms}$ frente a $6,034\text{ ms}$ y $4,294\text{ ms}$ en `C1` y `C2`, o `C1` de `MergeSort` en $n = 20.000$ con $6,789\text{ ms}$), se aprecia el efecto real de las interrupciones del sistema operativo sobre procesos que duran apenas unos pocos milisegundos.
- Con respecto a los resultados históricos, la incorporación del calentamiento global previo eliminó la distorsión inicial de $n = 10.000$: en la nueva serie, `Insertion Sort`, `MergeSort` y `QuickSort` crecen de forma estrictamente monótona al pasar de $10.000 \to 20.000 \to 40.000 \to 80.000$.

---

## 9. Análisis de Crecimiento Experimental (`T(2n) / T(n)`)

Calculando los cocientes `T(2n) / T(n)` directamente a partir de los promedios medidos en la Tabla 1:

- **Insertion Sort:**
  - $20\text{k} / 10\text{k} = 247,770 / 58,372 = \mathbf{4,24}$
  - $40\text{k} / 20\text{k} = 816,281 / 247,770 = \mathbf{3,29}$
  - $80\text{k} / 40\text{k} = 1.966,979 / 816,281 = \mathbf{2,41}$
- **MergeSort:**
  - $20\text{k} / 10\text{k} = 4,893 / 1,588 = \mathbf{3,08}$
  - $40\text{k} / 20\text{k} = 9,498 / 4,893 = \mathbf{1,94}$
  - $80\text{k} / 40\text{k} = 13,595 / 9,498 = \mathbf{1,43}$
- **QuickSort:**
  - $20\text{k} / 10\text{k} = 2,139 / 1,209 = \mathbf{1,77}$
  - $40\text{k} / 20\text{k} = 6,296 / 2,139 = \mathbf{2,94}$
  - $80\text{k} / 40\text{k} = 7,455 / 6,296 = \mathbf{1,18}$
- **Array.Sort:**
  - $20\text{k} / 10\text{k} = 1,846 / 0,815 = \mathbf{2,27}$
  - $40\text{k} / 20\text{k} = 6,982 / 1,846 = \mathbf{3,78}$
  - $80\text{k} / 40\text{k} = 6,669 / 6,982 = \mathbf{0,96}$ *(influenciado por el pico de `C3` en $40\text{k}$)*

### Tabla de Ratios de Crecimiento `T(2n) / T(n)`

| Algoritmo | 20k/10k | 40k/20k | 80k/40k | Crecimiento Acumulado (80k/10k, $\times 8n$) |
|---|---:|---:|---:|---:|
| **Insertion Sort** | x4,24 | x3,29 | x2,41 | **x33,70** |
| **MergeSort** | x3,08 | x1,94 | x1,43 | **x8,56** |
| **QuickSort** | x1,77 | x2,94 | x1,18 | **x6,17** |
| **Array.Sort** | x2,27 | x3,78 | x0,96 | **x8,18** |

---

## 10. Comparación con las Complejidades Teóricas

Al duplicar el tamaño de entrada ($n \to 2n$):
- **Crecimiento teórico de $O(n^2)$:**
  $$\frac{T(2n)}{T(n)} = \frac{(2n)^2}{n^2} = \mathbf{4,00}$$
  Y al multiplicar $n$ por $8$ ($10.000 \to 80.000$), el factor teórico es $8^2 = \mathbf{64,00}$.
- **Crecimiento teórico de $O(n \log_2 n)$:**
  $$\frac{T(2n)}{T(n)} = \frac{2n \log_2(2n)}{n \log_2(n)} = 2\left(1 + \frac{1}{\log_2 n}\right) \approx \mathbf{2,13\text{ a }2,15}$$
  Y al multiplicar $n$ por $8$ ($10.000 \to 80.000$), el factor teórico es:
  $$8 \times \frac{\log_2(80.000)}{\log_2(10.000)} = 8 \times \frac{16,288}{13,288} = \mathbf{9,81}$$

### ¿Por qué los tiempos medidos no son exactamente proporcionales a sus complejidades teóricas?

1. **Factores constantes y operaciones de distinto peso:** La notación Big-O abstrae constantes multiplicativas y términos de menor orden. En la práctica, `MergeSort` reserva un arreglo `auxiliar` en el *heap* y copia cada elemento dos veces por nivel de recursión, mientras que `QuickSort` permuta valores directamente en el mismo arreglo (*in-place*), razón por la cual `QuickSort` tarda casi la mitad que `MergeSort` en $n = 80.000$ ($7,455\text{ ms}$ vs. $13,595\text{ ms}$) aunque ambos sean $O(n \log n)$.
2. **Jerarquía de memoria caché (L1/L2/L3) y *prefetching*:** Los arreglos de $10.000$ a $80.000$ enteros ocupan entre $40\text{ KB}$ y $320\text{ KB}$. El recorrido estrictamente secuencial hacia atrás del bucle interno de `Insertion Sort` permite que el *hardware prefetcher* del procesador anticipe las líneas de caché con gran eficiencia y mantenga una alta tasa de aciertos en el predictor de saltos, lo que amortigua parcialmente los ratios en los tramos $40\text{k}/20\text{k}$ ($\times 3,29$) y $80\text{k}/40\text{k}$ ($\times 2,41$).
3. **Variabilidad del sistema operativo y escalado dinámico de frecuencia (DVFS):** En mediciones cortas (de $0,8\text{ ms}$ a $15\text{ ms}$), una sola interrupción de planificación del sistema operativo de $3\text{ a }5\text{ ms}$ altera un promedio puntual (como sucedió en `C3` de `Array.Sort` para $40\text{k}$). Por el contrario, cuando `Insertion Sort` se ejecuta de manera sostenida durante varios segundos en $40\text{k}$ y $80\text{k}$, el procesador mantiene su frecuencia turbo estabilizada en un bucle cerrado.

---

## 11. Complejidad Temporal y Espacial

| Algoritmo | Mejor Caso | Caso Promedio | Peor Caso | Complejidad Espacial | Estabilidad | Observación Técnica |
|---|---|---|---|---|---|---|
| **Insertion Sort** | $O(n)$ | $O(n^2)$ | $O(n^2)$ | $O(1)$ (*in-place*) | Sí | Mejor caso solo si el arreglo ya está ordenado. |
| **MergeSort** | $O(n \log n)$ | $O(n \log n)$ | $O(n \log n)$ | $O(n)$ (arreglo `auxiliar`) | Sí | Tiempo invariante respecto al orden inicial de los datos. |
| **QuickSort** | $O(n \log n)$ | $O(n \log n)$ | $O(n^2)$ | $O(\log n)$ promedio / $O(n)$ peor caso (pila) | No | Peor caso si el pivote (`arreglo[fin]`) es el mínimo o máximo en cada partición. |
| **Array.Sort** | $O(n \log n)$ | $O(n \log n)$ | $O(n \log n)$ | $O(\log n)$ (pila acotada) | No | Usa *IntroSort* (*QuickSort* + *InsertionSort* + *HeapSort*). |

---

## 12. Conclusiones del Torneo

1. **¿Qué algoritmo fue más rápido?**
   - En términos absolutos incluyendo la referencia nativa, **`Array.Sort`** fue el más veloz en casi todos los tamaños ($0,815\text{ ms}$ en $10\text{k}$, $1,846\text{ ms}$ en $20\text{k}$ y $6,669\text{ ms}$ en $80\text{k}$). Entre los tres algoritmos implementados manualmente para el torneo, el ganador indiscutible fue **`QuickSort`** en los cuatro tamaños evaluados ($1,209\text{ ms}$ en $10\text{k}$, $2,139\text{ ms}$ en $20\text{k}$, $6,296\text{ ms}$ en $40\text{k}$ y $7,455\text{ ms}$ en $80\text{k}$), superando a `MergeSort` por no requerir reserva ni copia de memoria auxiliar.

2. **¿Qué algoritmo presentó peor crecimiento?**
   - **`Insertion Sort`** presentó el peor crecimiento, pasando de $58,372\text{ ms}$ en $n = 10.000$ a **$1.966,979\text{ ms}$** ($\approx 2$ segundos) en $n = 80.000$. En el tamaño máximo evaluado ($n = 80.000$), `Insertion Sort` fue **$144,7$ veces más lento que `MergeSort`** y **$263,8$ veces más lento que `QuickSort`**.

3. **¿Cómo se comportó `InsertionSort` al duplicar los datos?**
   - En el primer tramo ($10\text{k} \to 20\text{k}$) multiplicó su tiempo por **$\times 4,24$**, reflejando de manera directa la cuadruplicación teórica ($\times 4$) propia de $O(n^2)$. En los tramos siguientes aumentó por factores de **$\times 3,29$** ($20\text{k} \to 40\text{k}$) y **$\times 2,41$** ($40\text{k} \to 80\text{k}$), acumulando un incremento total de **$\times 33,70$** al multiplicar por $8$ la cantidad de datos, frente a solo $\times 6,17\text{ a }\times 8,56$ de los algoritmos $O(n \log n)$.

4. **¿`MergeSort` y `QuickSort` se aproximaron al crecimiento esperado?**
   - **Sí.** Para un incremento de $8$ veces en el tamaño de entrada ($10.000 \to 80.000$), el factor teórico de $O(n \log_2 n)$ es $\times 9,81$. Experimentalmente, `MergeSort` multiplicó su tiempo total por **$\times 8,56$** ($1,588\text{ ms} \to 13,595\text{ ms}$, con un ratio intermedio de $\times 1,94$ en $40\text{k}/20\text{k}$ muy cercano al teórico $\times 2,14$) y `QuickSort` multiplicó su tiempo total por **$\times 6,17$** ($1,209\text{ ms} \to 7,455\text{ ms}$), manteniéndose ambos en el rango de unos pocos milisegundos.

5. **¿Por qué los tiempos medidos no son exactamente proporcionales a sus complejidades teóricas?**
   - Porque la complejidad asintótica cuenta operaciones abstractas cuando $n \to \infty$ ignorando constantes, mientras que el tiempo físico medido en milisegundos depende de la jerarquía de memoria caché (L1/L2/L3), la localidad espacial de los accesos, el predictor de saltos del procesador, el costo de reservar memoria dinámica (en `MergeSort`), la optimización del compilador JIT de .NET y las interrupciones del planificador de hilos del sistema operativo.

6. **¿Qué algoritmo elegiríamos para conjuntos de datos grandes y por qué?**
   - Para **entornos de producción en .NET**, elegiríamos **`Array.Sort`** porque utiliza *IntroSort*: aprovecha la velocidad práctica y el trabajo *in-place* de `QuickSort`, optimiza particiones pequeñas con `Insertion Sort` y garantiza una cota superior estricta de $O(n \log n)$ conmutando a `HeapSort` si detecta un pivote desfavorable.
   - Si debemos elegir **entre los tres algoritmos puros estudiados**, para datos aleatorios en memoria elegiríamos **`QuickSort`** (mejorando la elección del pivote mediante pivote aleatorio o mediana de tres para evitar el peor caso $O(n^2)$ en datos preordenados), mientras que si requerimos **ordenamiento estable** (preservar el orden relativo de registros con claves empatadas) o garantías estrictas de peor caso sin riesgo de degradación, elegiríamos **`MergeSort`**.

---

## 13. Instrucciones de Reproducción

Para compilar y ejecutar el experimento en modo `Release` desde la carpeta `practicas/c5` (o indicando la ruta del proyecto):

```powershell
cd practicas/c5
dotnet run -c Release
```
