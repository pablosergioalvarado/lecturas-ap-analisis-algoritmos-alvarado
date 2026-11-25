# Pablo Sergio Alvarado Gutierrez 

### Resultado de ejecución — Pruebas de la cura

| Caso | Resultado esperado | Por pares | Tu cura |
|---|---|---|---|
| Repetido en el medio | `true` | `true` | `true` |
| Sin repetidos | `false` | `false` | `false` |
| Lista vacía | `false` | `false` | `false` |
| Un solo elemento | `false` | `false` | `false` |
| Repetido en los extremos | `true` | `true` | `true` |

**Resultado:** las cinco pruebas fueron superadas correctamente.

### Medición de ejemplo del algoritmo original

`Por pares con 5.000 datos: 16 ms`

*Nota: es una sola medición, por lo tanto, no constituye evidencia de rendimiento.*

La cura utiliza un HashSet para detectar si un carnet ya apareció, recorriendo la lista una sola vez; su complejidad temporal esperada es O(n).

## P1.2 — Curar y medir

### 1. Descripción de la cura

La cura utiliza un `HashSet` para detectar si un carnet ya apareció, recorriendo la lista una sola vez. Su complejidad temporal esperada es **O(n)** y utiliza espacio adicional **O(n)**. No modifica el arreglo original.

### 2. Protocolo de medición

Se compararon `HayRepetidosPorPares` y `HayRepetidosCurado` utilizando listas sin elementos repetidos, generadas mediante `GenerarSinRepetidos`.

Se utilizaron cuatro tamaños de entrada: 5.000, 10.000, 20.000 y 40.000 elementos. Para cada tamaño se realizó una ejecución de calentamiento y tres ejecuciones medidas. Se calculó el promedio de los tres tiempos, trabajando en configuración Release.

### 3. Resultados

| Elementos (n) | Por pares (ms) | Cura (ms) | Factor por pares | Factor cura |
|---:|---:|---:|---:|---:|
| 5.000 | 32,478 | 0,781 | — | — |
| 10.000 | 40,848 | 1,325 | 1,26x | 1,70x |
| 20.000 | 177,695 | 2,513 | 4,35x | 1,90x |
| 40.000 | 521,205 | 2,331 | 2,93x | 0,93x |

### 4. Conclusiones

El algoritmo por pares presenta un crecimiento esperado de **O(n²)**, mientras que la cura presenta un crecimiento esperado de **O(n)**. Los resultados muestran que el algoritmo por pares aumenta considerablemente su tiempo al crecer la cantidad de datos, mientras que la cura mantiene tiempos mucho menores. Los factores no son exactos debido a variaciones propias de las mediciones, como se observa en el último resultado de la cura (0,93x). El peor tiempo registrado fue de **521,205 ms**, con 40.000 elementos, utilizando el algoritmo por pares.