# Pablo Sergio Alvarado Gutierrez 

# P2.3 — Medir

Se compararon la solución ingenua y la mejorada usando tres tamaños de entrada que se duplican, una corrida de calentamiento y tres corridas medidas por tamaño.

## 1. Resultados

### Solución ingenua

| Pedidos (n) | Corrida 1 (ms) | Corrida 2 (ms) | Corrida 3 (ms) | Promedio (ms) | Veces |
|---:|---:|---:|---:|---:|---:|
| 5.000 | 91,37 | 88,92 | 93,90 | 91,40 | - |
| 10.000 | 483,01 | 572,78 | 376,40 | 477,40 | 5,22x |
| 20.000 | 925,86 | 955,88 | 949,10 | 943,61 | 1,98x |

### Solución mejorada

| Pedidos (n) | Corrida 1 (ms) | Corrida 2 (ms) | Corrida 3 (ms) | Promedio (ms) | Veces |
|---:|---:|---:|---:|---:|---:|
| 5.000 | 13,83 | 10,28 | 2,94 | 9,02 | - |
| 10.000 | 6,26 | 7,33 | 19,07 | 10,89 | 1,21x |
| 20.000 | 17,01 | 38,05 | 12,99 | 22,68 | 2,08x |

## 2. Conclusión

La solución mejorada, utilizando un Heap y un HashSet, fue considerablemente más rápida. Con 20.000 pedidos, la ingenua tardó 943,61 ms y la mejorada 22,68 ms, aproximadamente 41,6 veces menos.

Los resultados muestran variaciones entre corridas, especialmente en tamaños pequeños, por lo que existe ruido en las mediciones. El crecimiento observado es compatible con la mejora esperada al sustituir la búsqueda repetida del mínimo en una lista por un Heap, aunque los tiempos no representan una comprobación exacta de la complejidad teórica.

