# Pablo Sergio Alvarado Gutierrez 

# P2.1 — Elegir

## 1. Estructuras elegidas

| Operación | Cantidad por jornada | Estructura | Costo O |
|---|---:|---|---|
| Registrar | 120.000 | Heap | O(log n) |
| PrepararSiguiente | 120.000 | Heap | O(log n) |
| EstaPendiente | 40.000 | HashSet | O(1) promedio |
| Pendientes | Consultas | HashSet.Count | O(1) |

Se utiliza un Heap para obtener el pedido con menor plazo, y un HashSet para consultar si un pedido sigue pendiente.

En el Heap se utiliza como prioridad el plazo y el orden de llegada, para que, si dos pedidos tienen el mismo plazo, se prepare primero el que llegó antes.

## 2. ¿Por qué no utilizar la lista simple?

Con la lista simple, para preparar un pedido se debe recorrer la lista buscando el menor plazo. Como se realizan 120.000 preparaciones, el costo es cuadrático, O(n²).

Con n = 120.000, el número aproximado de comparaciones para buscar el mínimo durante todas las preparaciones es:

120.000 × 119.999 / 2 ≈ 7.200 millones de comparaciones.

Con la solución mejorada, el Heap permite incorporar y preparar pedidos en O(log n), mientras que el HashSet permite consultar pedidos pendientes en O(1) promedio.

Esto evita recorrer toda la lista repetidamente y reduce considerablemente el trabajo.

## 3. ¿Cuándo cambiaría de opinión?

Si el sistema solamente necesitara registrar pedidos y consultar si existen, sin necesitar preparar el pedido más urgente, podría utilizar únicamente un HashSet o un Diccionario, evitando mantener un Heap.

Pero mientras se necesite preparar siempre el pedido con menor plazo, el Heap es adecuado para este proceso.


# P2.2 — Aplicar

## Implementación de la solución mejorada

La implementación se realizó en la clase `ReceptorMejorado`, dentro de `Program.cs`, utilizando las estructuras `PriorityQueue` (Heap) y `HashSet`.

### 1. Estructuras implementadas

- **Heap (`PriorityQueue`)**: almacena los pedidos priorizando el menor plazo. En caso de empate, prioriza el orden de llegada.
- **HashSet**: permite consultar y actualizar rápidamente los pedidos que todavía están pendientes.

### 2. Métodos implementados

| Método | Implementación | Costo O |
|---|---|---|
| `Registrar()` | Inserta el pedido en el Heap y en el HashSet. | O(log n) |
| `PrepararSiguiente()` | Extrae del Heap el pedido prioritario y lo elimina del HashSet. | O(log n) |
| `EstaPendiente()` | Consulta la existencia del pedido en el HashSet. | O(1) promedio |
| `Pendientes` | Obtiene la cantidad mediante `HashSet.Count`. | O(1) |

### 3. Desempate por orden de llegada

Para garantizar que, cuando dos pedidos tienen el mismo plazo, se prepare primero el que llegó antes, el Heap utiliza una prioridad compuesta:

`(Plazo, Llegada)`

El Heap compara primero el plazo y, si es igual, compara el número de llegada.

### 4. Prueba funcional

Se ejecutó el guion funcional definido en `Main`:

- `ReceptorIngenuo`: 11 de 11 pasos correctos.
- `ReceptorMejorado`: 11 de 11 pasos correctos.

La implementación mejorada cumple con las operaciones solicitadas y el desempate por orden de llegada.