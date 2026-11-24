# H3 — Análisis de estrategia algorítmica del proceso lento

## 1. Proceso analizado

El proceso lento identificado consiste en listar los períodos de lectura (`GET /periodos`). Durante el mapeo de los períodos, se necesita obtener la fecha `FechaHasta` del registro correspondiente en `FACLOTES`.

La correspondencia se establece mediante una clave compuesta:

- `IdRuta`
- `Anio`
- `Nroper` (correspondiente al mes del período)

En la implementación analizada, por cada período se busca el lote correspondiente, recorriendo la colección de `FACLOTES`. Esto provoca búsquedas repetidas y aumenta el costo conforme crece la cantidad de datos.

## 2. ¿Aplica una estrategia Voraz?

No aplica naturalmente una estrategia Voraz.

Voraz consiste en tomar una decisión local que, bajo propiedades demostrables, permita construir una solución global óptima. En este proceso no existe una decisión de selección que deba optimizarse, ni una elección local que determine una solución global.

El objetivo es encontrar el registro de `FACLOTES` que corresponde exactamente a cada período, utilizando una clave. Elegir el primer registro disponible, el más cercano o el que tenga algún criterio local no garantiza una correspondencia correcta.

Por tanto, no existe una propiedad de elección voraz que justifique aplicar esta estrategia.

## 3. ¿Aplica programación dinámica?

Tampoco aplica naturalmente programación dinámica.

La programación dinámica se utiliza cuando existen subproblemas repetidos y una solución óptima puede construirse a partir de soluciones óptimas de subproblemas más pequeños.

En este caso, no se está construyendo una solución óptima a partir de decisiones anteriores. Se busca asociar cada período con un registro de `FACLOTES` mediante una clave compuesta.

Aunque la implementación actual repite búsquedas, esa repetición no corresponde a subproblemas de optimización que deban almacenarse para construir una solución óptima. El problema es de búsqueda y asociación de datos.

## 4. Estrategia de mejora

La mejora adecuada consiste en utilizar una estructura de datos de indexación, como `Dictionary`, que permita localizar los registros de `FACLOTES` mediante la clave compuesta `(IdRuta, Anio, Nroper)`.

En lugar de recorrer la colección de lotes por cada período, se construye previamente un índice y se utiliza la búsqueda por clave.

- Enfoque ingenuo: búsqueda repetida mediante recorridos.
- Enfoque optimizado: construcción de un índice y consultas por clave.

El enfoque ingenuo tiene un costo aproximado de O(n × k), donde n es la cantidad de períodos y k la cantidad de lotes que se recorren para encontrar las correspondencias.

Con un diccionario, la construcción del índice cuesta O(k) y las búsquedas tienen un costo esperado O(1) por consulta, resultando en un costo esperado aproximado de O(k + n).

## 5. Conclusión

El proceso analizado no presenta las propiedades necesarias para aplicar directamente Voraz ni programación dinámica. La mejora se encuentra en seleccionar una estructura de datos adecuada que reduzca el costo de búsqueda y asociación.

Este análisis permite aplicar los conocimientos de la materia para identificar la estrategia algorítmica apropiada, sin forzar una técnica que no corresponde al problema.