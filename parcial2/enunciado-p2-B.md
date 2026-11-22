# PARCIAL 2 — ANÁLISIS DE ALGORITMOS · VARIANTE B (Pedidos de una pastelería)
**Domingo 22-nov-2026 · 09:25 a 10:40 (75 minutos) · link en Classroom hasta las 10:45 · 12 puntos · individual · en TU lenguaje**

Se te entrega el archivo `examen-p2-B.cs`: un sistema de pedidos de una pastelería con una solución ingenua (`ReceptorIngenuo`,
una lista simple: correcta pero lenta) y una clase vacía (`ReceptorMejorado`) que debes escribir tú. Puedes usar el C# tal cual
si tienes .NET o portar todo a tu lenguaje. Tu trabajo es **elegir la estructura, aplicarla y medir la diferencia**.

## El caso
Pedidos que llegan con un plazo en minutos (el menor plazo es el más urgente). Se prepara del plazo más corto al más largo;
si dos tienen el mismo plazo, primero el que llegó antes. Las operaciones del sistema (`IReceptorPedidos`):
- `Registrar(pedido, plazoMinutos)`: llega un pedido (los nombres son únicos).
- `PrepararSiguiente()`: saca y devuelve el de menor plazo; devuelve «ninguno» (null) si no hay ninguno.
- `EstaPendiente(pedido)`: ¿ese pedido sigue pendiente? (prepararlo lo saca).
- `Pendientes`: cuántos hay pendientes.

Una jornada normal tiene **120.000 pedidos, 120.000 preparaciones y 40.000 consultas de `EstaPendiente`**.

## Qué entregas (en tu repositorio, carpeta `parcial2/`, por commit)
La primera línea de cada archivo lleva tu nombre completo.

### P2.1 — Elegir (4 puntos) · archivo `eleccion.md`
- **Una tabla** con las cuatro operaciones: cuántas veces se hacen en una jornada, la **estructura** que usas y su **costo O**. Puedes combinar más de una estructura.
- **Por qué no sirve la lista simple (con números):** costo aproximado de una jornada con la lista simple y con tu estructura, usando n = 120.000.
- **Una condición en la que cambiarías de opinión** (por ejemplo, si cambiaran las operaciones o los tamaños).

### P2.2 — Aplicar (5 puntos) · tu código (`ReceptorMejorado` o su equivalente en tu lenguaje)
Escribe la solución mejorada con la(s) estructura(s) que elegiste. Debe pasar los pasos de prueba que imprime `Main` y respetar el desempate (mismo plazo: primero el que llegó antes). Puedes usar las estructuras que trae tu lenguaje (cola de prioridad, conjunto, diccionario, árbol ordenado).

### P2.3 — Medir (2 puntos) · archivo `medicion.md`
Con la función `EjecutarCarga` (n pedidos llegan y luego se preparan los n), mide `ReceptorIngenuo` y tu solución en **tres tamaños que se dupliquen** (n, 2n, 4n). Elige n para que la ingenua, con el mayor tamaño, tarde entre 0,5 y 20 segundos en TU lenguaje. Protocolo: **1 corrida de calentamiento + 3 corridas medidas + promedio**. Entrega la tabla (tiempo de cada una y cuántas veces más tarda respecto de la fila anterior) y dos o tres líneas de conclusión que digan si cada una se comporta como el O que declaraste y mencionen el ruido.

### P2.4 — Entrega prolija (1 punto)
Carpeta `parcial2/` con `eleccion.md`, `medicion.md` y tu código, nombre completo en la primera línea de cada archivo, todo con el último push antes de las 10:40.

## Reglas
- **Tiempos:** trabajas 75 minutos, de 09:25 a 10:40 (el examen se publica a las 09:25). A las 10:40 cierra el último push: la hora del push es la evidencia. De 10:40 a 10:45 pegas en la tarea de Classroom el link de tu carpeta `parcial2/` y pulsas Entregar. Un push entre las 10:40 y las 10:55 tiene techo de 10. Después de las 10:55 no se evalúa.
- Trabajo **individual y sin IA**. Puedes consultar el material del curso publicado en el tablón (código y guías).
- En la defensa final puedes ser interrogado sobre cualquier línea de tu parcial: entrega solo lo que puedas defender.
- Pega el link de tu carpeta `parcial2/` en GitHub como entrega en la tarea de Classroom (nunca el link de un commit).

## Rúbrica (publicada por anticipado)
| Ítem | Criterio | Puntos |
|---|---|---|
| P2.1 | Estructura(s) con el costo O de cada operación (2) · por qué no la lista simple, con números (1) · condición de cambio (1) | 4 |
| P2.2 | Pasa los pasos de prueba y el desempate (3) · usa las estructuras declaradas y su costo real coincide con el declarado (2) | 5 |
| P2.3 | Tabla con 3 tamaños que se duplican y el protocolo aplicado (1) · lectura honesta que conecta la tabla con el O y menciona el ruido (1) | 2 |
| P2.4 | Carpeta completa, con nombre en cada archivo y push antes de las 10:40 | 1 |
| **Total** | | **12** |
