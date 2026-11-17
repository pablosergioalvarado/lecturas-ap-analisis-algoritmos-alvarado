# PARCIAL 1 — ANÁLISIS DE ALGORITMOS · VARIANTE A (Registro de carnets)
**Lunes 16-nov-2026 · 19:10 a 20:40 (90 minutos) · link en Classroom hasta las 20:45 · 15 puntos · individual · en TU lenguaje**

Se te entrega el archivo `examen-p1-A.cs`: una función CORRECTA pero LENTA para el problema «¿hay algún carnet
repetido en la lista?». Puedes usar el C# tal cual si tienes .NET o portar todo a tu lenguaje; la lógica de la función
lenta no se cambia. Tu trabajo es **analizar, curar y medir, y elegir**.

## Qué entregas (en tu repositorio, carpeta `parcial1/`, por commit)

La primera línea de cada archivo lleva tu nombre completo.

### P1.1 — Analizar (5 puntos) · archivo `analisis.md`
**a) Cuatro fragmentos (3 puntos, 0,75 c/u).** Para cada uno: su costo O en el peor caso y UNA línea que lo justifique.

```
// F1                                   // F2
total = 0                               cuenta = 0
for i = 1 to n:                         for i = 1 to n:
    total = total + dato[i]                 for j = i + 1 to n:
                                                if dato[i] == dato[j]: cuenta = cuenta + 1

// F3                                   // F4
k = n                                   for i = 1 to n:
while k > 1:                                k = 1
    k = k / 2                               while k < n:
    paso = paso + 1                             k = k * 2
```

**b) Dos recurrencias (2 puntos, 1 c/u).** Resuelve con el teorema maestro de bolsillo (T(1) = 1): escribe a, b, d, la comparación de a con b^d y la forma.

```
(i)   T(n) = 3 T(n/2) + n
(ii)  T(n) =   T(n/2) + n²
```

### P1.2 — Curar y medir (5 puntos) · archivo `medicion.md` (+ tu código)
**1. Cura (1,5 puntos).** Escribe `HayRepetidosCurado` (en tu lenguaje): una versión correcta con un costo MENOR que O(n²), sin modificar la lista que recibe. Debe pasar los cinco casos que imprime `Main` (repetido en el medio, sin repetidos, lista vacía, un solo elemento, repetido en los extremos). En `medicion.md` escribe en una línea qué hace tu cura y cuál es su costo O.

**2. Medición (3,5 puntos).** Mide `HayRepetidosPorPares` y tu cura en el PEOR CASO (lista SIN repetidos, ya la genera `GenerarSinRepetidos`).
- Usa **cuatro tamaños que se dupliquen** (n, 2n, 4n, 8n). Elige n para que la solución lenta, con el mayor tamaño, tarde entre 0,5 y 20 segundos en TU lenguaje.
- Protocolo: **1 corrida de calentamiento + 3 corridas medidas + promedio**, en modo Release o equivalente.
- Entrega una **tabla** (n, tiempo de cada solución y cuántas veces más tarda respecto de la fila anterior) y **tres o cuatro líneas de conclusión**: ¿cada solución se comporta como el O que calculaste? ¿Hay ruido en algún tiempo? ¿Cuál es el peor tiempo medido y con qué n?

### P1.3 — Elegir (4 puntos) · archivo `eleccion.md`
Un hospital guarda **300.000 números de historia clínica**. El listado llega desordenado cada mañana y durante el día se hacen **5.000 consultas** del tipo «¿está este número registrado?». La memoria es limitada: no se puede duplicar el listado.
Compara estas opciones y decide:
1. Para cada consulta, recorrer el listado de principio a fin.
2. Ordenar el listado una vez en la mañana (con un algoritmo n log n) y usar búsqueda binaria en cada consulta.
3. Ordenar el listado una vez con un algoritmo cuadrático y usar búsqueda binaria.

Tu archivo debe tener: **el costo total aproximado de cada opción en operaciones, con números** (usa n = 300.000 y 5.000 consultas); **tu elección**; y **a partir de cuántas consultas cambiarías de opinión**. Usa lo que mediste en P1.2 como apoyo.

### P1.4 — Entrega prolija (1 punto)
Carpeta `parcial1/` con los tres archivos (`analisis.md`, `medicion.md`, `eleccion.md`) y tu código de medición, todo con el último push antes de las 20:40.

## Reglas
- **Tiempos:** trabajas 90 minutos, de 19:10 a 20:40. A las 20:40 cierra el último push: la hora del push es la evidencia. De 20:40 a 20:45 pegas en la tarea de Classroom el link de tu carpeta `parcial1/` y pulsas Entregar. Un push entre las 20:40 y las 20:55 tiene techo de 12. Después de las 20:55 no se evalúa.
- Trabajo **individual y sin IA**. Puedes consultar el material del curso publicado en el tablón (código y guías).
- En la defensa final puedes ser interrogado sobre cualquier línea de tu parcial: entrega solo lo que puedas defender.
- Pega el link de tu carpeta `parcial1/` en GitHub como entrega en la tarea de Classroom (nunca el link de un commit).

## Rúbrica (publicada por anticipado)
| Ítem | Criterio | Puntos |
|---|---|---|
| P1.1 a | 4 fragmentos: costo correcto con su línea de justificación (0,75 c/u) | 3 |
| P1.1 b | 2 recurrencias: a, b, d, comparación y forma correctas (1 c/u) | 2 |
| P1.2 | Cura correcta, que pasa los 5 casos, con su O declarado (1,5) · tabla con 4 tamaños que se duplican y protocolo aplicado (1,5) · cocientes bien calculados y bien leídos (1) · conclusión que conecta la tabla con el O y menciona el ruido (1) | 5 |
| P1.3 | Costos totales con números (2) · elección coherente con ellos (1) · punto en que cambiaría la decisión (1) | 4 |
| P1.4 | Carpeta completa, con nombre en cada archivo y push antes de las 20:40 | 1 |
| **Total** | | **15** |
