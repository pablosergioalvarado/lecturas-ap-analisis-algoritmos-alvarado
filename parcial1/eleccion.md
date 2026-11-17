ordenar una vez y utilizar búsqueda binaria

Primero se ordena el listado utilizando un algoritmo de complejidad O(n log n). Para evitar duplicar los datos en memoria, se puede utilizar HeapSort, que ordena en el mismo arreglo.

Costo aproximado del ordenamiento:

300.000 × log₂(300.000) ≈ 5.460.000 operaciones.

Luego se realizan 5.000 búsquedas binarias, con un máximo aproximado de 19 comparaciones por consulta:

5.000 × 19 = 95.000 operaciones.

Costo total:

5.460.000 + 95.000 = 5.555.000 operaciones aproximadamente.

ordenar una vez el listado y utilizar búsqueda binaria durante el día.

Aunque ordenar requiere un trabajo inicial, ese costo se realiza una sola vez. Después, cada consulta es mucho más rápida porque la búsqueda binaria reduce el espacio de búsqueda aproximadamente a la mitad en cada paso.

Además, utilizaría HeapSort para ordenar el arreglo en el mismo espacio de memoria, evitando duplicar los 300.000 registros.

La opción 1 repite demasiadas operaciones y la opción 3 utiliza un algoritmo cuadrático que resulta excesivamente costoso para esta cantidad de datos.