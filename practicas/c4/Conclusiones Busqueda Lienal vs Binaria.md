# Conclusiones: Búsqueda lineal vs. búsqueda binaria

No siempre conviene ordenar los datos para utilizar la búsqueda binaria. Debemos considerar el costo de ordenar y cuántas veces aprovecharemos ese orden mediante búsquedas posteriores.

## ¿Cuándo conviene ordenar los datos?

| Escenario | ¿Conviene ordenar? | Motivo |
|---|---|---|
| Datos desordenados y una sola búsqueda | Generalmente no | Una búsqueda lineal puede ser más económica que ordenar todo el arreglo. |
| Datos desordenados y muchas búsquedas | Generalmente sí | El costo de ordenar se amortiza entre las múltiples consultas. |
| Datos que ya están ordenados | Sí, utilizar búsqueda binaria | No necesitamos pagar nuevamente el costo de ordenamiento. |

## Conclusión del experimento

En nuestro experimento, la búsqueda binaria fue aproximadamente **1.102 veces más rápida** que la búsqueda lineal para 1.000 consultas sobre un millón de elementos ordenados.

Por tanto, si los datos ya están ordenados o necesitamos realizar búsquedas frecuentes, conviene utilizar búsqueda binaria. Sin embargo, si los datos están desordenados y necesitamos realizar una sola búsqueda, generalmente resulta más conveniente la búsqueda lineal, porque el costo de ordenar previamente puede superar el de buscar directamente.

**Aprendizaje:** La elección del algoritmo no depende únicamente de su complejidad teórica, sino también del tamaño de los datos, de si están ordenados y de la cantidad de búsquedas que necesitamos realizar.