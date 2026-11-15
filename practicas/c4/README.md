# Práctica C4 — Búsqueda Lineal vs. Búsqueda Binaria

**Asignatura:** Análisis de Algoritmos — Ingeniería de Sistemas (UAB)  
**Estudiante:** Pablo Sergio Alvarado Gutiérrez  
**Lenguaje y Entorno:** C# sobre .NET 8.0 (`net8.0`, configuración `Release`)  
**Caso Integrador Asociado (H1):** Gestión y Liquidación de Periodos de Lecturas de Alumbrado Público (`MOVILES.LECTURAS_AP` y `ERP_ELFEC.FAC_LOTES`)

---

## 1. Explicación del Problema

En el desarrollo de sistemas de información y en particular en nuestro caso integrador **H1 (Gestión y Liquidación de Periodos de Lecturas de Alumbrado Público)**, la operación de búsqueda de registros por clave se repite de forma intensiva sobre grandes volúmenes de datos ($173.080$ lecturas móviles y más de $445.000$ lotes o $1.000.000$ de medidores).

Cuando se busca un elemento dentro de una colección de tamaño $n$, existen dos estrategias fundamentales:
1. **Búsqueda Lineal (Secuencial):** Recorre el arreglo posición por posición desde el índice $0$ hasta encontrar el elemento o llegar al final del arreglo. No requiere que los datos estén ordenados, pero su costo crece proporcionalmente al tamaño de la entrada: $O(n)$.
2. **Búsqueda Binaria (Dicotómica):** Aprovecha que el arreglo se encuentra **previamente ordenado** para comparar la clave buscada con el elemento central (`medio`). En cada paso descarta exactamente la mitad del espacio de búsqueda restante, logrando un crecimiento logarítmico: $O(\log_2 n)$.

Esta práctica evalúa y contrasta ambos algoritmos en dos experimentos independientes y complementarios:
- **Experimento A (Cantidad de pasos):** Medición exacta del número de comparaciones realizadas en el peor caso (buscando una clave inexistente) sobre arreglos ordenados de $1.000$, $100.000$ y $1.000.000$ de elementos.
- **Experimento B (Tiempo de ejecución):** Medición empírica en milisegundos del tiempo requerido para resolver un lote de $1.000$ búsquedas de claves existentes sobre un arreglo de $1.000.000$ de enteros, aplicando calentamiento previo y promediando tres corridas independientes.

---

## 2. Implementación de Ambos Algoritmos

El código fuente completo y ejecutable se encuentra en [Program.cs](Program.cs) junto al archivo de proyecto [c4.csproj](c4.csproj). Adicionalmente, se conserva en esta carpeta el registro del ejercicio manual de clase en [Tarea busqueda binaria a mano.txt](Tarea%20busqueda%20binaria%20a%20mano.txt).

### 2.1. Búsqueda Lineal — $O(n)$

```csharp
public static int BuscarLineal(int[] datos, int clave)
{
    for (int i = 0; i < datos.Length; i++)
    {
        if (datos[i] == clave)
        {
            return i;
        }
    }
    return -1;
}
```

### 2.2. Búsqueda Binaria — $O(\log_2 n)$

Se utiliza la fórmula `int medio = ini + (fin - ini) / 2;` para evitar cualquier riesgo de desbordamiento de enteros (`integer overflow`) que ocurriría con `(ini + fin) / 2` en arreglos muy grandes:

```csharp
public static int BuscarBinaria(int[] datos, int clave)
{
    int ini = 0;
    int fin = datos.Length - 1;

    while (ini <= fin)
    {
        int medio = ini + (fin - ini) / 2;
        int valorMedio = datos[medio];

        if (valorMedio == clave)
        {
            return medio;
        }

        if (valorMedio < clave)
        {
            ini = medio + 1;
        }
        else
        {
            fin = medio - 1;
        }
    }

    return -1;
}
```

---

## 3. Experimento A — Comparación de Cantidad de Pasos (Peor Caso)

### 3.1. Condiciones del Experimento A
- **Tamaños evaluados ($n$):** $1.000$, $100.000$ y $1.000.000$ de elementos.
- **Datos:** Valores pares consecutivos ordenados ascendentemente: $0, 2, 4, \dots, 2(n-1)$.
- **Clave buscada:** $2n + 1$ (valor impar superior al máximo del arreglo, por lo tanto inexistente), lo que fuerza el **peor caso** tanto en la búsqueda lineal como en la búsqueda binaria.
- **Métrica:** Cantidad exacta de iteraciones/comparaciones de clave realizadas hasta concluir que el elemento no existe (devolviendo `-1`).

### 3.2. Tabla de Resultados — Experimento A (Pasos)

Los resultados obtenidos al ejecutar [Program.cs](Program.cs) coinciden de manera exacta con el antecedente histórico y con la cota teórica $\lfloor\log_2(n)\rfloor + 1$ documentada en [Tarea busqueda binaria a mano.txt](Tarea%20busqueda%20binaria%20a%20mano.txt):

| Tamaño ($n$) | Pasos Búsqueda Lineal $O(n)$ | Pasos Búsqueda Binaria $O(\log_2 n)$ | Fórmula Teórica $\lfloor\log_2(n)\rfloor + 1$ | Reducción de Pasos |
|---:|---:|---:|---:|---:|
| **1.000** | 1.000 | 10 | $\lfloor 9,966 \rfloor + 1 = 10$ | $100\times$ menos pasos |
| **100.000** | 100.000 | 17 | $\lfloor 16,610 \rfloor + 1 = 17$ | $5.882\times$ menos pasos |
| **1.000.000** | 1.000.000 | 20 | $\lfloor 19,932 \rfloor + 1 = 20$ | $50.000\times$ menos pasos |

**Análisis del Experimento A:** Mientras la búsqueda lineal multiplica por $1.000$ sus pasos al pasar de $n = 1.000$ a $n = 1.000.000$, la búsqueda binaria apenas duplica la cantidad de pasos (de $10$ a $20$ pasos), ya que $1.000.000 \approx (1.000)^2$ y $\log_2(n^2) = 2\log_2(n)$.

---

## 4. Experimento B — Protocolo y Medición de Tiempos de Ejecución

### 4.1. Protocolo Experimental
1. **Construcción del arreglo base:** Se genera fuera del cronómetro un arreglo ordenado de $n = 1.000.000$ de enteros (pares consecutivos $0, 2, 4, \dots, 1.999.998$).
2. **Selección reproducible de claves:** Se eligen $1.000$ claves **existentes** en el arreglo utilizando el generador pseudoaleatorio con semilla fija `new Random(21)` fuera del cronómetro. Ambos algoritmos buscan exactamente el mismo lote de $1.000$ claves.
3. **Calentamiento (Warm-up):** Antes de iniciar las mediciones, se ejecutan corridas de calentamiento de ambos lotes de búsqueda para asegurar que el compilador JIT (*Just-In-Time*) de .NET 8 haya promovido los métodos al nivel nativo optimizado (*Tier 1*) y se invoca la limpieza del recolector de basura (`GC.Collect()`).
4. **Tres corridas medidas:** Se mide con `Stopwatch` de alta resolución el tiempo total de ejecutar las $1.000$ búsquedas en tres corridas independientes (`Corrida 1`, `Corrida 2`, `Corrida 3`).
5. **Cálculo del promedio y cociente:** Se calcula la media aritmética de las tres corridas:
   $$\text{Promedio} = \frac{T_1 + T_2 + T_3}{3}$$
   Y el cociente de aceleración:
   $$\text{Cociente} = \frac{\text{Promedio Búsqueda Lineal}}{\text{Promedio Búsqueda Binaria}}$$

### 4.2. Tabla de Tiempos — Experimento B

#### A) Registro de Validación Reproducible Actual (`dotnet run -c Release`)

| Algoritmo | Corrida 1 (ms) | Corrida 2 (ms) | Corrida 3 (ms) | Promedio de 3 Corridas (ms) |
|---|---:|---:|---:|---:|
| **Búsqueda lineal** | 641,1822 ms | 512,1480 ms | 664,3452 ms | **605,8918 ms** |
| **Búsqueda binaria** | 0,6292 ms | 0,6528 ms | 0,4786 ms | **0,5869 ms** |

- **Cociente obtenido en la validación actual:**
  $$\text{Cociente} = \frac{605,8918\text{ ms}}{0,5869\text{ ms}} = \mathbf{1.032,42\text{ veces más rápida la búsqueda binaria}}$$

#### B) Contraste con el Antecedente Histórico Registrado

| Algoritmo | Tiempo Promedio Histórico | Tiempo Promedio Reproducido Actual | Observación |
|---|---:|---:|---|
| **Búsqueda lineal** | 145,6 ms | 605,8918 ms | Orden de cientos de milisegundos ($O(n)$ por cada una de las 1.000 claves). |
| **Búsqueda binaria** | 0,165 ms | 0,5869 ms | Fracción de milisegundo para las 1.000 búsquedas completas ($O(\log n)$). |
| **Cociente (Lineal / Binaria)** | **$\approx 883$ veces** | **$1.032,42$ veces** | En ambas mediciones la búsqueda binaria supera entre $\sim 880\times$ y $\sim 1.030\times$ a la lineal. |

> **¿Por qué los tiempos medidos y el cociente ($\sim 883\times$ a $1.032\times$) no son exactamente iguales al cociente teórico de pasos ($\sim 25.000\times$ en caso promedio)?**
> En teoría, para $n = 1.000.000$, una búsqueda lineal de clave existente revisa en promedio $n/2 = 500.000$ elementos, mientras que la búsqueda binaria realiza $\approx 19$ comparaciones ($\text{ratio teórico de pasos} \approx 26.315\times$). Sin embargo, en el tiempo real medido en CPU el cociente es de $\sim 883\times$ a $1.032\times$ porque:
> 1. **Localidad espacial de caché y *Hardware Prefetching*:** La búsqueda lineal recorre posiciones de memoria contiguas (`datos[0], datos[1], datos[2]...`). El procesador carga líneas de caché L1/L2 completas (64 bytes = 16 enteros `int` a la vez) de forma anticipada sin fallos de caché.
> 2. **Predicción de saltos (*Branch Prediction*):** En la búsqueda lineal, la condición `datos[i] == clave` es falsa el $99,9998\%$ de las veces, por lo que el predictor de saltos de la CPU acierta casi siempre sin vaciar la tubería (*pipeline*). En cambio, en la búsqueda binaria cada salto (`datos[medio] < clave`) depende de los datos con probabilidad $50\% / 50\%$ y salta a posiciones distantes de la RAM, provocando fallos de caché (*cache misses*) y penalizaciones de predicción de salto por cada paso. Aun con esa ventaja física a favor del recorrido secuencial, la diferencia algorítmica entre $O(n)$ y $O(\log n)$ hace que la búsqueda binaria sea **alrededor de mil veces más veloz**.

---

## 5. Complejidad Temporal y Espacial

| Algoritmo | Mejor Caso | Caso Promedio | Peor Caso | Complejidad Espacial (Iterativa) | Requisito Previo |
|---|---|---|---|---|---|
| **Búsqueda Lineal** | $O(1)$ (primer elemento) | $O(n)$ ($n/2$ pasos) | $O(n)$ ($n$ pasos) | $O(1)$ | Ninguno (admite arreglos desordenados) |
| **Búsqueda Binaria** | $O(1)$ (elemento central) | $O(\log_2 n)$ | $O(\log_2 n)$ ($\lfloor\log_2 n\rfloor + 1$ pasos) | $O(1)$ | **Arreglo previamente ordenado** |

---

## 6. Conclusión sobre si Conviene Ordenar Previamente para Nuestro Proceso

Sí conviene ordenar (o indexar) previamente los datos en nuestro proceso de Alumbrado Público, porque el costo único de ordenamiento $O(n \log n)$ se amortiza rápidamente frente a las decenas de consultas repetitivas por lote y suministro que hoy ejecutan escaneos secuenciales $O(n)$. Al realizar múltiples búsquedas sobre el mismo padrón de lecturas y lotes de facturación, sustituir recorridos lineales por búsquedas logarítmicas $O(\log n)$ o estructuras indexadas reduce el tiempo de respuesta en casi tres órdenes de magnitud ($\sim 883\times$ a $1.032\times$).

---

## 7. Instrucciones para Reproducir las Mediciones

Desde la raíz del repositorio o desde la carpeta `practicas/c4`, ejecutar en una terminal con .NET 8.0 SDK:

```powershell
dotnet run --project practicas/c4/c4.csproj -c Release
```
