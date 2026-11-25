# Pablo Sergio Alvarado Gutierrez 


## 1. Análisis de la solución ingenua

La solución ingenua (`PlanificadorIngenuo`) busca la combinación óptima de paquetes probando todos los subconjuntos posibles.

Cada paquete tiene dos posibilidades: incluirlo en la furgoneta o no incluirlo. Por tanto, para `n` paquetes existen `2^n` subconjuntos posibles.

Para evaluar cada subconjunto, el algoritmo recorre los `n` paquetes, comprobando cuáles están seleccionados y acumulando su peso y ganancia. Esta evaluación tiene un costo de `O(n)`.

Por lo tanto, el algoritmo evalúa `2^n` subconjuntos, realizando un trabajo de `O(n)` por cada uno:

**Costo algorítmico: O(n · 2^n)**

Aunque al finalizar se realiza otro recorrido de `n` paquetes para reconstruir la lista de paquetes elegidos, este costo adicional de `O(n)` no modifica la complejidad dominante.

La solución es correcta, pero resulta ineficiente cuando aumenta la cantidad de paquetes, debido al crecimiento exponencial de los subconjuntos que debe evaluar.


## 2. Recurrencia de la versión recursiva

Para cada paquete existen dos posibilidades: incluirlo en la furgoneta o no incluirlo. Por ello, una versión recursiva del problema genera dos llamadas, cada una considerando un paquete menos.

La recurrencia que representa este comportamiento es:

**T(n) = 2T(n-1) + 1**

Al expandirla, observamos cómo se duplica el número de llamadas:

- T(n) = 2T(n-1) + 1
- T(n) = 4T(n-2) + 3
- T(n) = 8T(n-3) + 7

En cada paso se generan el doble de llamadas respecto al nivel anterior. Al llegar al caso base, esta expansión permite observar que el número de llamadas crece proporcionalmente a \(2^n\).

Por tanto, el crecimiento de esta recurrencia es:

**T(n) = O(2^n)**

Esta recurrencia representa el proceso de explorar las dos posibilidades para cada paquete. El Teorema Maestro no se aplica directamente, ya que los subproblemas tienen tamaño n-1, en lugar de dividir el tamaño de entrada por un factor constante.

En la implementación de `PlanificadorIngenuo`, además de explorar las combinaciones, se recorren los paquetes para evaluar cada una. Por eso, su coste es **O(n · 2^n)**.


## 3. Contraejemplo para una estrategia voraz

Una estrategia voraz habitual consiste en seleccionar los paquetes según su relación entre ganancia y peso, priorizando aquellos que generan mayor ganancia por kilogramo.

Consideremos los siguientes paquetes y una capacidad de 50 kg:

| Paquete | Peso | Ganancia | Ganancia/kg |
|---|---:|---:|---:|
| P1 | 10 kg | 60 Bs | 6 Bs/kg |
| P2 | 20 kg | 100 Bs | 5 Bs/kg |
| P3 | 30 kg | 120 Bs | 4 Bs/kg |

La estrategia voraz selecciona primero P1, porque tiene la mayor relación ganancia/peso (6 Bs/kg). Después selecciona P2, que también cabe en la furgoneta. Al quedar solamente 20 kg de capacidad, P3 ya no puede ser seleccionado.

El resultado de esta estrategia es:

- Paquetes seleccionados: P1 y P2.
- Peso total: 30 kg.
- Ganancia total: 160 Bs.

Sin embargo, la combinación P2 y P3 utiliza exactamente los 50 kg de capacidad y obtiene una ganancia de 220 Bs.

Por tanto, la estrategia voraz obtiene 160 Bs, mientras que la solución óptima obtiene 220 Bs.

Este contraejemplo demuestra que seleccionar los paquetes según su relación ganancia/peso no garantiza encontrar la solución óptima cuando los paquetes son indivisibles. Por ello, no podemos utilizar este criterio voraz como estrategia para garantizar la máxima ganancia.


## 4. Estrategia elegida

Elegimos programación dinámica porque permite encontrar la combinación de paquetes con mayor ganancia sin explorar todas las combinaciones posibles.

Para ello, construimos una tabla que guarda la mejor ganancia para cada cantidad de paquetes y capacidad disponible. En cada caso, evaluamos si conviene incluir o no un paquete y reutilizamos resultados calculados anteriormente. Al finalizar, podemos recuperar tanto la ganancia óptima como los paquetes seleccionados.

El coste de esta estrategia es **O(n · C)** en tiempo y espacio, donde `n` es la cantidad de paquetes y `C` la capacidad de la furgoneta.

Esta solución permite evitar los cálculos repetidos de la estrategia ingenua, aunque su rendimiento depende también de la capacidad de la furgoneta.


## 5. Estructura de datos para consultas repetidas

Después de calcular la selección óptima, el programa debe responder repetidamente si un paquete determinado está incluido en el viaje.

La solución ingenua utiliza una lista y el método `Contains`, cuyo costo en el peor caso es O(k), donde `k` es la cantidad de paquetes seleccionados.

Para la solución mejorada se elige un `HashSet<string>`, que almacenará los códigos de los paquetes seleccionados. Esta estructura permite realizar búsquedas de pertenencia con un costo promedio de **O(1)**.

Una vez reconstruida la selección óptima, los códigos se almacenarán en el `HashSet`, que será utilizado por el método `EstaEnElViaje(codigo)` para responder las consultas posteriores.

El costo de construir el `HashSet` es O(k) en promedio, su espacio es O(k) y las consultas de pertenencia tienen un costo promedio de O(1).
