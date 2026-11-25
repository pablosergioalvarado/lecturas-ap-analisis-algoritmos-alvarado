// EVALUACIÓN INTEGRADORA · VARIANTE A — "Furgoneta de reparto"
// Caso: una distribuidora tiene una lista de paquetes (cada uno con un peso en kg y una ganancia en Bs) y una furgoneta
// que aguanta una capacidad en kg. Hay que elegir QUÉ paquetes llevar para ganar lo máximo sin pasarse de la capacidad.
// Cada paquete se lleva entero o no se lleva.
// PlanificadorIngenuo es CORRECTO pero LENTO (prueba todas las combinaciones). Tu trabajo: analizarlo, elegir la
// estrategia, APLICARLA en PlanificadorMejorado, medir la diferencia y justificar todo.
// Puedes usar este archivo tal cual (si tienes .NET) o portar todo a TU lenguaje.



using System.Diagnostics;

namespace Integradora.VarianteA;

public sealed record Paquete(string Codigo, int Kg, int Ganancia);

public interface IPlanificador
{
    // Devuelve la ganancia máxima y los códigos de los paquetes elegidos (en cualquier orden).
    (int ganancia, List<string> codigos) Elegir(Paquete[] paquetes, int capacidadKg);

    // Después de Elegir: ¿este paquete va en la furgoneta?
    bool EstaEnElViaje(string codigo);
}

// ---------- SOLUCIÓN INGENUA: prueba todos los subconjuntos (2^n) ----------
public sealed class PlanificadorIngenuo : IPlanificador
{
    private List<string> _elegidos = new();

    public (int ganancia, List<string> codigos) Elegir(Paquete[] paquetes, int capacidadKg)
    {
        int n = paquetes.Length, mejor = 0;
        long mejorMascara = 0;

        for (long mascara = 0; mascara < (1L << n); mascara++)
        {
            int kg = 0, ganancia = 0;

            for (int i = 0; i < n; i++)
                if ((mascara & (1L << i)) != 0)
                {
                    kg += paquetes[i].Kg;
                    ganancia += paquetes[i].Ganancia;
                }

            if (kg <= capacidadKg && ganancia > mejor)
            {
                mejor = ganancia;
                mejorMascara = mascara;
            }
        }

        _elegidos = new List<string>();

        for (int i = 0; i < n; i++)
            if ((mejorMascara & (1L << i)) != 0)
                _elegidos.Add(paquetes[i].Codigo);

        return (mejor, new List<string>(_elegidos));
    }

    public bool EstaEnElViaje(string codigo) => _elegidos.Contains(codigo);
}

// ---------- SOLUCIÓN MEJORADA: PROGRAMACIÓN DINÁMICA ----------
public sealed class PlanificadorMejorado : IPlanificador
{
    // Almacena los códigos de los paquetes seleccionados para consultas rápidas.
    private HashSet<string> _elegidos = new();

    public (int ganancia, List<string> codigos) Elegir(Paquete[] paquetes, int capacidadKg)
    {
        int n = paquetes.Length;

        // DP[i, c]: máxima ganancia considerando los primeros i paquetes
        // con una capacidad disponible de c kilogramos.
        int[,] dp = new int[n + 1, capacidadKg + 1];

        // Construcción de la tabla de programación dinámica.
        for (int i = 1; i <= n; i++)
        {
            Paquete paquete = paquetes[i - 1];

            for (int c = 0; c <= capacidadKg; c++)
            {
                // Alternativa 1: no incluir el paquete.
                dp[i, c] = dp[i - 1, c];

                // Alternativa 2: incluirlo, si cabe.
                if (paquete.Kg <= c)
                {
                    int gananciaIncluyendo =
                        paquete.Ganancia + dp[i - 1, c - paquete.Kg];

                    if (gananciaIncluyendo > dp[i, c])
                        dp[i, c] = gananciaIncluyendo;
                }
            }
        }

        // La ganancia máxima se encuentra en la última fila y capacidad disponible.
        int gananciaMaxima = dp[n, capacidadKg];

        // Reconstrucción de los paquetes seleccionados.
        List<string> codigos = new();
        int capacidadRestante = capacidadKg;

        for (int i = n; i > 0; i--)
        {
            // Si el valor cambió respecto a la fila anterior,
            // significa que este paquete fue seleccionado.
            if (dp[i, capacidadRestante] != dp[i - 1, capacidadRestante])
            {
                Paquete paquete = paquetes[i - 1];

                codigos.Add(paquete.Codigo);
                capacidadRestante -= paquete.Kg;
            }
        }

        // Preparamos el HashSet para las consultas posteriores.
        _elegidos = new HashSet<string>(codigos);

        return (gananciaMaxima, codigos);
    }

    // Consulta de pertenencia: O(1) promedio.
    public bool EstaEnElViaje(string codigo) => _elegidos.Contains(codigo);
}

public static class Programa
{
    private static Paquete[] Lote(params (string c, int kg, int g)[] datos) =>
        datos.Select(d => new Paquete(d.c, d.kg, d.g)).ToArray();

    // Los casos de prueba (todos con ÓPTIMO ÚNICO):
    // (paquetes, capacidad, ganancia esperada, códigos esperados).
    private static readonly (string que, Paquete[] paquetes, int capacidad, int esperada, string[] codigos)[] Casos =
    {
        ("el voraz se equivoca", Lote(("P1", 10, 60), ("P2", 20, 100), ("P3", 30, 120)), 50, 220, new[] { "P2", "P3" }),
        ("cuatro paquetes",      Lote(("P1", 2, 3), ("P2", 3, 4), ("P3", 4, 5), ("P4", 5, 6)), 8, 10, new[] { "P2", "P4" }),
        ("capacidad cero",       Lote(("P1", 5, 10), ("P2", 7, 12)), 0, 0, Array.Empty<string>()),
        ("sin paquetes",         Lote(), 30, 0, Array.Empty<string>()),
        ("nada cabe",            Lote(("P1", 40, 90), ("P2", 50, 100)), 30, 0, Array.Empty<string>()),
        ("entran todos",         Lote(("P1", 5, 10), ("P2", 7, 12), ("P3", 3, 4)), 100, 26, new[] { "P1", "P2", "P3" }),
    };

    private static void Probar(string titulo, Func<IPlanificador> fabrica)
    {
        int bien = 0, total = 0;

        try
        {
            foreach (var (que, paquetes, capacidad, esperada, codigos) in Casos)
            {
                var plan = fabrica();
                var (ganancia, elegidos) = plan.Elegir(paquetes, capacidad);

                total++;

                bool ok = ganancia == esperada &&
                          elegidos.OrderBy(x => x).SequenceEqual(codigos.OrderBy(x => x));

                if (ok)
                    bien++;
                else
                    Console.WriteLine($"   MAL en «{que}»: esperado {esperada} con [{string.Join(",", codigos)}], obtuvo {ganancia} con [{string.Join(",", elegidos)}]");
            }

            var p = fabrica();
            p.Elegir(Casos[0].paquetes, Casos[0].capacidad);

            total += 2;

            if (p.EstaEnElViaje("P2"))
                bien++;
            else
                Console.WriteLine("   MAL en «EstaEnElViaje(P2)»: esperado True");

            if (!p.EstaEnElViaje("P1"))
                bien++;
            else
                Console.WriteLine("   MAL en «EstaEnElViaje(P1)»: esperado False");

            Console.WriteLine($"{titulo}: {bien} de {total} pruebas correctas");
        }
        catch (NotImplementedException)
        {
            Console.WriteLine($"{titulo}: (sin escribir todavía)");
        }
    }

    // Genera exactamente los mismos datos para ambos algoritmos
    // cuando se utiliza el mismo tamaño n.
    private static Paquete[] GenerarPaquetes(int n)
    {
        var azar = new Random(n);

        return Enumerable.Range(0, n)
            .Select(i => new Paquete("C" + i, azar.Next(5, 60), azar.Next(10, 100)))
            .ToArray();
    }

    private static int ObtenerCapacidad(Paquete[] paquetes)
    {
        return paquetes.Sum(p => p.Kg) / 3;
    }

    // Una corrida de calentamiento y tres corridas medidas.
    private static double MedirPromedio(
        Func<IPlanificador> fabrica,
        Paquete[] paquetes,
        int capacidad)
    {
        const int repeticiones = 3;

        // Calentamiento: no se contabiliza.
        fabrica().Elegir(paquetes, capacidad);

        double totalMs = 0;

        for (int i = 0; i < repeticiones; i++)
        {
            var plan = fabrica();

            var reloj = Stopwatch.StartNew();
            plan.Elegir(paquetes, capacidad);
            reloj.Stop();

            totalMs += reloj.Elapsed.TotalMilliseconds;
        }

        return totalMs / repeticiones;
    }

    // I3: comparación de cuatro tamaños.
    private static void MedirComparacion()
    {
        int[] tamanos = { 18, 20, 22, 24 };

        double anteriorIngenuo = 0;
        double anteriorMejorado = 0;

        Console.WriteLine();
        Console.WriteLine("==============================================================");
        Console.WriteLine("MEDICION: PLANIFICADOR INGENUO VS PLANIFICADOR MEJORADO");
        Console.WriteLine("==============================================================");
        Console.WriteLine("Protocolo: 1 calentamiento + 3 corridas medidas.");
        Console.WriteLine("Se muestra el promedio de las 3 corridas.");
        Console.WriteLine();

        Console.WriteLine(
            $"{"n",4} | {"Ingenuo(ms)",12} | {"Mejorado(ms)",12} | {"Ganancia",10} | {"Factor Ing.",12} | {"Factor Mej.",12}");

        Console.WriteLine(new string('-', 82));

        foreach (int n in tamanos)
        {
            Paquete[] paquetes = GenerarPaquetes(n);
            int capacidad = ObtenerCapacidad(paquetes);

            // Verificación funcional de ambos algoritmos antes de medir.
            var ingenuoVerificacion = new PlanificadorIngenuo();
            var mejoradoVerificacion = new PlanificadorMejorado();

            var resultadoIngenuo = ingenuoVerificacion.Elegir(paquetes, capacidad);
            var resultadoMejorado = mejoradoVerificacion.Elegir(paquetes, capacidad);

            if (resultadoIngenuo.ganancia != resultadoMejorado.ganancia)
            {
                Console.WriteLine($"ERROR: resultados diferentes para n={n}");
                Console.WriteLine($"Ingenuo: {resultadoIngenuo.ganancia}");
                Console.WriteLine($"Mejorado: {resultadoMejorado.ganancia}");
                return;
            }

            // Medición: ambos algoritmos utilizan los mismos datos.
            double tiempoIngenuo = MedirPromedio(
                () => new PlanificadorIngenuo(), paquetes, capacidad);

            double tiempoMejorado = MedirPromedio(
                () => new PlanificadorMejorado(), paquetes, capacidad);

            string factorIngenuo = anteriorIngenuo > 0
                ? $"{tiempoIngenuo / anteriorIngenuo:F2}x"
                : "-";

            string factorMejorado = anteriorMejorado > 0
                ? $"{tiempoMejorado / anteriorMejorado:F2}x"
                : "-";

            Console.WriteLine(
                $"{n,4} | {tiempoIngenuo,12:F3} | {tiempoMejorado,12:F3} | " +
                $"{resultadoMejorado.ganancia,10} | {factorIngenuo,12} | {factorMejorado,12}");

            anteriorIngenuo = tiempoIngenuo;
            anteriorMejorado = tiempoMejorado;
        }

        Console.WriteLine();
        Console.WriteLine("Verificacion: ambos algoritmos obtuvieron la misma ganancia en los cuatro tamaños.");
    }

    // Prueba de la solución mejorada con 1.000 paquetes.
    private static void MedirEscalaGrande()
    {
        const int n = 1000;

        Paquete[] paquetes = GenerarPaquetes(n);
        int capacidad = ObtenerCapacidad(paquetes);

        var plan = new PlanificadorMejorado();

        var reloj = Stopwatch.StartNew();
        var resultado = plan.Elegir(paquetes, capacidad);
        reloj.Stop();

        Console.WriteLine();
        Console.WriteLine("==============================================================");
        Console.WriteLine("SOLUCION MEJORADA A ESCALA GRANDE");
        Console.WriteLine("==============================================================");
        Console.WriteLine($"Paquetes: {n}");
        Console.WriteLine($"Capacidad: {capacidad} kg");
        Console.WriteLine($"Ganancia optima: {resultado.ganancia}");
        Console.WriteLine($"Paquetes seleccionados: {resultado.codigos.Count}");
        Console.WriteLine($"Tiempo: {reloj.Elapsed.TotalMilliseconds:F3} ms");
    }

    // La CARGA original, conservada.
    public static int EjecutarCarga(IPlanificador plan, int n)
    {
        var azar = new Random(n);

        var paquetes = Enumerable.Range(0, n)
            .Select(i => new Paquete("C" + i, azar.Next(5, 60), azar.Next(10, 100)))
            .ToArray();

        int capacidad = paquetes.Sum(p => p.Kg) / 3;

        var (ganancia, _) = plan.Elegir(paquetes, capacidad);

        return ganancia;
    }

    public static void Main()
    {
        // I2: verificamos que las soluciones siguen siendo correctas.
        Probar("PlanificadorIngenuo ", () => new PlanificadorIngenuo());
        Probar("PlanificadorMejorado", () => new PlanificadorMejorado());

        // I3: comparación experimental.
        MedirComparacion();

        // I3: solución mejorada a escala grande.
        MedirEscalaGrande();
    }
}
