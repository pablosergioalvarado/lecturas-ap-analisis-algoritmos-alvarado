# Nombre completo: Sergio Alvarado

# I3 — Medición y comparación experimental

## 1. Objetivo

Comparar el rendimiento de `PlanificadorIngenuo` y `PlanificadorMejorado` a medida que aumenta la cantidad de paquetes, verificando que ambos obtengan la misma ganancia óptima.

Además, probar el algoritmo mejorado con 1.000 paquetes y analizar su viabilidad ante una capacidad de 1.000.000.000 kg.

## 2. Protocolo experimental

Se evaluaron cuatro tamaños de entrada: **18, 20, 22 y 24 paquetes**, incrementando de dos en dos, buscando que la última ejecución del algoritmo ingenuo se encuentre entre 0,5 y 20 segundos.

Para cada tamaño se generaron paquetes y se estableció la capacidad de la furgoneta en un tercio del peso total generado.

Ambos algoritmos se ejecutaron con los mismos paquetes y la misma capacidad, para garantizar una comparación justa.

### Protocolo de ejecución

Para cada algoritmo y tamaño de entrada se realizó:

1. Una ejecución de calentamiento, cuyo resultado no se incluye en el cálculo del promedio.
2. Tres ejecuciones medidas.
3. El cálculo del promedio de las tres ejecuciones medidas.

Los tiempos se expresan en milisegundos.

También se verificó que ambos algoritmos obtuvieran la misma ganancia óptima para cada tamaño, antes de considerar válida la comparación de rendimiento.

Los factores de crecimiento se calcularon dividiendo el tiempo promedio de la fila actual entre el tiempo promedio de la fila anterior.

## 3. Resultados experimentales

### 3.1. Comparación de tiempos de ejecución

| n | Ingenuo (ms) | Mejorado (ms) | Ganancia (Bs) |
|---:|---:|---:|---:|
| 18 | 126,218 | 0,199 | 511 |
| 20 | 421,600 | 0,254 | 520 |
| 22 | 1.538,200 | 0,324 | 743 |
| 24 | 5.139,573 | 0,228 | 1.040 |

**Verificación funcional:** ambos algoritmos obtuvieron la misma ganancia en los cuatro tamaños de entrada.

### 3.2. Factores de crecimiento

Se calcularon los factores de crecimiento respecto al tamaño inmediatamente anterior.

| n | Ingenuo (ms) | Factor ingenuo | Mejorado (ms) | Factor mejorado |
|---:|---:|---:|---:|---:|
| 18 | 126,218 | — | 0,199 | — |
| 20 | 421,600 | 3,34x | 0,254 | 1,28x |
| 22 | 1.538,200 | 3,65x | 0,324 | 1,27x |
| 24 | 5.139,573 | 3,34x | 0,228 | 0,70x |

Los factores se obtuvieron a partir de los tiempos promedio medidos, no de estimaciones teóricas.

## 4. Análisis de los resultados

### 4.1. Planificador ingenuo

El `PlanificadorIngenuo` evalúa todas las combinaciones posibles de paquetes. Para cada combinación, analiza los paquetes seleccionados y calcula el peso y la ganancia correspondientes.

Su coste teórico es O(n · 2^n), debido a la enumeración de las combinaciones y al trabajo realizado para evaluar cada una.

Los resultados experimentales muestran un incremento considerable del tiempo de ejecución a medida que aumenta el número de paquetes.

| Incremento | Factor observado |
|---|---:|
| 18 → 20 | 3,34x |
| 20 → 22 | 3,65x |
| 22 → 24 | 3,34x |

Al pasar de 18 a 24 paquetes, el tiempo promedio pasa de 126,218 ms a 5.139,573 ms.

Esto evidencia un crecimiento acelerado del tiempo de ejecución. Aunque los factores observados no son idénticos, muestran un comportamiento compatible con el crecimiento exponencial esperado para la enumeración de combinaciones.

El algoritmo ingenuo resulta adecuado para tamaños pequeños, pero su rendimiento se deteriora rápidamente a medida que crece el número de paquetes.

### 4.2. Planificador mejorado

El `PlanificadorMejorado` utiliza programación dinámica, con un coste teórico de **O(n · C)**, donde `n` es la cantidad de paquetes y `C` la capacidad de la furgoneta.

| n | Tiempo promedio |
|---:|---:|
| 18 | 0,199 ms |
| 20 | 0,254 ms |
| 22 | 0,324 ms |
| 24 | 0,228 ms |

Los tiempos se mantuvieron muy bajos en todos los tamaños. Las pequeñas variaciones observadas, especialmente por debajo del milisegundo, pueden estar influenciadas por el entorno de ejecución y no representan necesariamente cambios en el rendimiento del algoritmo.

### 4.3. Comparación general

El algoritmo ingenuo mostró un crecimiento acelerado en sus tiempos de ejecución, mientras que el mejorado resolvió los mismos casos en tiempos muy reducidos.

Esta diferencia se debe a que la programación dinámica reutiliza resultados de subproblemas, evitando explorar todas las combinaciones posibles.

Ambos algoritmos obtuvieron la misma ganancia óptima en los cuatro tamaños analizados, por lo que la mejora de rendimiento no implicó una pérdida de exactitud.

## 5. Prueba de escalabilidad: 1.000 paquetes

Se realizó una prueba adicional con el `PlanificadorMejorado`:

| Parámetro | Resultado |
|---|---:|
| Paquetes | 1.000 |
| Capacidad | 10.634 kg |
| Ganancia óptima | 33.297 Bs |
| Paquetes seleccionados | 486 |
| Tiempo | 417,627 ms |

La solución obtuvo la ganancia óptima en aproximadamente 418 ms, mostrando que puede abordar un problema de mayor tamaño en un tiempo razonable.

Esta prueba fue una ejecución puntual, sin aplicar el protocolo de calentamiento y tres corridas utilizado en la comparación principal.

## 6. Análisis de una capacidad de 1.000.000.000 kg

La implementación utiliza una matriz de dimensiones `(n + 1) × (C + 1)`, por lo que su coste temporal y de memoria es **O(n · C)**.

Con 1.000 paquetes y una capacidad de 1.000.000.000 kg, la matriz tendría aproximadamente 1.001.000.001.000 celdas. Considerando 4 bytes por celda, necesitaría alrededor de **4 TB de memoria**, sin contar otros costes.

Por tanto, esta capacidad no sería viable para nuestra implementación. Aunque la programación dinámica evita explorar todas las combinaciones como hace el algoritmo ingenuo, sigue necesitando muchos recursos cuando la capacidad de la furgoneta es demasiado grande.

Esto muestra una limitación de nuestra solución: su rendimiento depende tanto de la cantidad de paquetes como de la capacidad de la furgoneta.

Para capacidades muy grandes sería necesario utilizar otra estrategia, que no forma parte de la implementación realizada en esta práctica.

## 7. Conclusiones

El `PlanificadorMejorado`, basado en programación dinámica, obtuvo un rendimiento considerablemente superior al ingenuo, manteniendo la misma ganancia óptima en las pruebas realizadas.

La prueba con 1.000 paquetes confirmó que puede resolver problemas de mayor tamaño en un tiempo razonable. Sin embargo, también se identificó una limitación: cuando la capacidad de la furgoneta es muy grande, la matriz requiere demasiada memoria.

En conclusión, la programación dinámica permite resolver este problema de forma mucho más eficiente que explorar todas las combinaciones, aunque su rendimiento sigue dependiendo de la capacidad de la furgoneta.