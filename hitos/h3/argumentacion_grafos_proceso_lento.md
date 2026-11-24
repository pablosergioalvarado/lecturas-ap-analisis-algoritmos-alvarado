# ¿Nuestro proceso lento se puede modelar como un grafo?

## Descripción del problema

Nuestro caso de H2 se centra en el proceso de listado de períodos de
lectura. El proceso obtiene una lista de períodos y, para cada uno,
busca el `FACLOTE` que le corresponde dentro de otra colección.

La correspondencia se establece a partir de atributos que identifican la
relación entre ambos datos, por ejemplo:

-   `IdRuta`
-   Año
-   Número de período

El problema de rendimiento surge por la forma en que se realiza esa
búsqueda y asociación entre colecciones, no por la necesidad de recorrer
una red de conexiones.

## Representación simplificada del proceso

``` mermaid
flowchart TD
    A[Lista de períodos] --> B[Buscar FACLOTE correspondiente]
    B --> C[Recorrer colección FACLOTES]
    C --> D[Construir resultado]
```

Este esquema representa un flujo de búsqueda y asociación de datos: para
cada período se localiza el `FACLOTE` correspondiente y, con esa
información, se construye el resultado.

## ¿Qué tipo de problema estamos resolviendo?

Se trata principalmente de un problema de **correspondencia entre
datos**. Por cada período, buscamos un elemento de otra colección que
coincida con determinados atributos.

Esto se relaciona más directamente con:

-   Búsqueda de elementos.
-   Asociación entre colecciones.
-   Optimización del acceso a información.

Por tanto, una mejora basada en una estructura de búsqueda o índice
---por ejemplo, un `Dictionary` con una clave adecuada--- puede ser más
apropiada que construir un grafo.

## ¿Por qué no se modela naturalmente como un grafo?

Aunque los períodos y los `FACLOTES` están relacionados, esa relación no
implica que el problema sea un grafo.

El problema central no consiste en:

-   Encontrar un camino entre nodos.
-   Recorrer conexiones sucesivas.
-   Descubrir qué elementos están conectados.
-   Encontrar componentes conexas.
-   Analizar ciclos.
-   Explorar dependencias.

**Nuestro problema busca identificar qué elemento de una colección
corresponde a otro elemento, no recorrer una red de relaciones.**

Podríamos representar artificialmente los períodos y los `FACLOTES` como
nodos y sus correspondencias como aristas, pero ese modelado no aporta
una ventaja evidente para resolver la búsqueda que necesitamos realizar.
Sería una representación adicional que no responde a la naturaleza
principal del problema.

## Conclusión

Nuestro proceso lento **no se modela naturalmente como un grafo**,
porque no necesitamos analizar caminos, conectividad, ciclos ni
recorridos de una red. Necesitamos asociar eficientemente cada período
con el `FACLOTE` correspondiente.

Por eso, el enfoque más adecuado es optimizar la búsqueda y la
asociación entre colecciones mediante una estructura de datos que
permita localizar los elementos por sus claves, en lugar de utilizar
algoritmos de grafos como BFS o DFS.

> Que los datos estén relacionados no significa necesariamente que
> debamos modelarlos como un grafo. La estructura de datos y el
> algoritmo se eligen según el problema que necesitamos resolver.
