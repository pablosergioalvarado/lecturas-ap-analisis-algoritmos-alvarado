## C5 – Torneo de algoritmos de ordenamiento

### 1. Objetivo

Comparar el rendimiento de diferentes algoritmos de ordenamiento utilizando cantidades crecientes de datos, para observar cómo cambia su tiempo de ejecución y relacionarlo con su complejidad algorítmica.

### 2. Algoritmos evaluados

Se evaluaron cuatro implementaciones:

- **InsertionSort:** algoritmo de ordenamiento cuadrático.
- **MergeSort:** algoritmo basado en divide y vencerás.
- **QuickSort:** algoritmo basado en particiones y pivote.
- **Array.Sort:** método de ordenamiento proporcionado por .NET, utilizado como referencia de rendimiento.

### 3. Datos y metodología

Se utilizaron arreglos de números enteros generados aleatoriamente, con una semilla fija (`Random(2026)`) para garantizar la reproducibilidad de los datos.

Se probaron cuatro tamaños de entrada: 10.000, 20.000, 40.000 y 80.000 elementos.

Para obtener las mediciones se realizaron las siguientes acciones:

- Ejecución de calentamiento para reducir el efecto de la compilación JIT.
- Tres ejecuciones por algoritmo y tamaño de entrada.
- Preparación de una copia del arreglo antes de iniciar el cronómetro.
- Medición del tiempo utilizando `Stopwatch`, considerando únicamente la ejecución del algoritmo.
- Verificación posterior de que los datos estuvieran correctamente ordenados y que el arreglo original no hubiera sido modificado.

Las pruebas se ejecutaron en configuración `Release`.

### 4. Resultados

Los tiempos promedio obtenidos, expresados en milisegundos, fueron los siguientes:

| Cantidad de datos | InsertionSort | MergeSort | QuickSort | Array.Sort |
|---:|---:|---:|---:|---:|
| 10.000 | 58,372 | 1,588 | 1,209 | 0,815 |
| 20.000 | 247,770 | 4,893 | 2,139 | 1,846 |
| 40.000 | 816,281 | 9,498 | 6,296 | 6,982 |
| 80.000 | 1.966,979 | 13,595 | 7,455 | 6,669 |

Los resultados corresponden al promedio de tres ejecuciones por cada combinación de algoritmo y tamaño de entrada.

### 5. Conclusiones


Al aumentar la cantidad de datos de 10.000 a 80.000 elementos, **InsertionSort mostró un crecimiento mucho mayor en su tiempo de ejecución**, pasando de 58,372 ms a 1.966,979 ms. Esto evidencia el costo de un algoritmo cuadrático O(n^2).

En cambio, **MergeSort y QuickSort tuvieron un crecimiento considerablemente menor**, demostrando las ventajas de los algoritmos de complejidad O(n log n) para ordenar cantidades crecientes de datos.

En esta prueba, QuickSort obtuvo mejores tiempos que MergeSort en todos los tamaños evaluados. Sin embargo, Array.Sort de .NET fue el más rápido en tres de los cuatro tamaños, lo que demuestra la eficiencia de una implementación optimizada.

**La principal conclusión es que la elección del algoritmo influye directamente en el rendimiento:** mientras más crecen los datos, más importante resulta utilizar algoritmos con un crecimiento eficiente.