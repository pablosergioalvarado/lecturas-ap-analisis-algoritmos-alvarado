### P1.1 — Analizar (5 puntos) · archivo `analisis.md`


#### F1 — O(n)

El ciclo recorre los n elementos una vez, por lo que el trabajo crece proporcionalmente a n. 

#### F2 — O(n²)

El algoritmo expresa 2 ciclos anidados y que el numero total de comparaciones crece cuadraticamente (n x n).

#### F3 — O(log n)

El contador comienza en n y se divide entre 2 en cada iteración hasta llegar a 1.

#### F4 — O(n log n)

El ciclo externo realiza n iteraciones y, para cada vuelta, el while multiplica k por 2 hasta alcanzar n. En resumen se deduce una iteracion lineal y una iteracion logaritmica que reduce el problema a la mitad.


### Teorema maestro de bolsillo

La forma general de una recurrencia es:

T(n)=aT(n/b)+n^d

Donde:
- **a:** número de llamadas recursivas.
- **b:** factor por el que se divide el tamaño del problema.
- **d:** exponente del término que está fuera de la recursión.

Calculamos (b^d) y comparamos su resultado con (a):

| Comparación   | Complejidad       |

| (a < b^d)     | O(n^d)            |
| (a = b^d)     | O(n^d log n)      |
| (a > b^d)     | O(n^{log_b a})    |

#### (i)   T(n) = 3 T(n/2) + n

Identificamos:
a = 3
b = 2
d = 1 (n^1)
b^d = 2^1 = 2
Resultados
a = 3, b^d = 2
Comparamos:
a < b^d -------> O(n^d)
a = b^d -------> O(n^d log n)
a > b^d -------> O(n^log a)
log2 3 = 1.585
Entonces le costo de esfuerzo de (i) es O(n^1.585)

#### (ii)  T(n) =   T(n/2) + n²
Identificamos:
a = 1
b = 2
d = 2
b^d = 2^2 = 4
Resultados
a = 1, b^d = 4
Comparamos:
a < b^d -------> O(n^d)
a = b^d -------> O(n^d log n)
a > b^d -------> O(n^log a)

resultado comparacion
a < b^d
1 < 2^2

Entonces le costo de esfuerzo de (ii) es O(n^2). Bucle anidado, o n cuadratica.