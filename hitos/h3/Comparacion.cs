using System.Diagnostics;
using System.Globalization;

namespace ComparacionH3;

// Entidades determinísticas que representan el cruce real entre Períodos y FACLOTES
public sealed class PeriodoLectura
{
    public int Id { get; init; }
    public int IdSuministro { get; init; }
    public int IdRuta { get; init; }
    public int Anio { get; init; }
    public int Mes { get; init; }
}

public sealed class FacLoteRegistro
{
    public int IdLote { get; init; }
    public int IdRuta { get; init; }
    public int Anio { get; init; }
    public int Nroper { get; init; }
    public DateTime FechaHasta { get; init; }
}

public sealed class AsociacionPeriodoLote
{
    public int PeriodoId { get; init; }
    public int IdLote { get; init; }
    public int IdRuta { get; init; }
    public int Anio { get; init; }
    public int Mes { get; init; }
    public string FechaHasta { get; init; } = string.Empty;
}

public static class Program
{
    public static void Main()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        int[] tamanios = { 500, 1000, 2000, 4000 };
        const int repeticiones = 3;

        Console.WriteLine();
        Console.WriteLine("====================================================================");
        Console.WriteLine("        COMPARACIÓN EXPERIMENTAL H3: FUERZA BRUTA VS INDEXACIÓN");
        Console.WriteLine("====================================================================");
        Console.WriteLine("Caso      : Asociación Períodos <-> FACLOTES por (IdRuta, Anio, Nroper)");
        Console.WriteLine("Estrategia A : Fuerza Bruta (búsqueda secuencial) -> O(n * k)");
        Console.WriteLine("Estrategia B : Indexación con Dictionary          -> O(k + n) esp.");
        Console.WriteLine($"Protocolo    : Calentamiento + {repeticiones} repeticiones + promedio");
        Console.WriteLine("====================================================================");

        // ------------------------------------------------------------------
        // 1. CALENTAMIENTO PREVIO (JIT Warmup) Y VERIFICACIÓN INICIAL
        // ------------------------------------------------------------------
        Console.WriteLine();
        Console.WriteLine("[CALENTAMIENTO Y VERIFICACIÓN INICIAL]");
        var (periodosWarmup, lotesWarmup) = GenerarDatosDeterministicos(250);

        var resWarmupBruta = AsociarFuerzaBruta(periodosWarmup, lotesWarmup);
        var resWarmupIndex = AsociarConIndexacion(periodosWarmup, lotesWarmup);
        if (!VerificarResultados(resWarmupBruta, resWarmupIndex, periodosWarmup.Count, out string? errorWarmup))
        {
            Console.Error.WriteLine($"ERROR EN VERIFICACIÓN DE CALENTAMIENTO: {errorWarmup}");
            Environment.Exit(1);
            return;
        }

        Console.WriteLine($"  Datos utilizados : {periodosWarmup.Count} períodos / {lotesWarmup.Count} lotes");
        Console.WriteLine($"  Asociaciones     : {resWarmupIndex.Count} / {periodosWarmup.Count} verificadas e idénticas");
        Console.WriteLine("  Estado           : COMPLETADO");

        // ------------------------------------------------------------------
        // 2. VERIFICACIÓN PREVIA Y MEDICIÓN POR TAMAÑO
        // ------------------------------------------------------------------
        var resumen = new List<(int N, double FuerzaBrutaMs, double IndexacionMs)>();

        foreach (int n in tamanios)
        {
            // Generación determinística fuera del cronómetro
            var (periodos, lotes) = GenerarDatosDeterministicos(n);

            // Verificación obligatoria ANTES de medir este tamaño
            var preBruta = AsociarFuerzaBruta(periodos, lotes);
            var preIndex = AsociarConIndexacion(periodos, lotes);
            if (!VerificarResultados(preBruta, preIndex, periodos.Count, out string? errorMsg))
            {
                Console.Error.WriteLine($"ERROR EN VERIFICACIÓN PARA n={n}: {errorMsg}");
                Environment.Exit(1);
                return;
            }

            double[] tiemposBruta = new double[repeticiones];
            double[] tiemposIndex = new double[repeticiones];

            // Repeticiones: Estrategia A (Fuerza Bruta)
            for (int r = 0; r < repeticiones; r++)
            {
                LimpiarGC();
                var sw = Stopwatch.StartNew();
                var resultado = AsociarFuerzaBruta(periodos, lotes);
                sw.Stop();

                if (resultado.Count != n)
                {
                    Console.Error.WriteLine($"ERROR: Cantidad inesperada en fuerza bruta ({resultado.Count} != {n}).");
                    Environment.Exit(1);
                    return;
                }

                tiemposBruta[r] = sw.Elapsed.TotalMilliseconds;
            }

            // Repeticiones: Estrategia B (Indexación con Dictionary, incluye construcción del índice)
            for (int r = 0; r < repeticiones; r++)
            {
                LimpiarGC();
                var sw = Stopwatch.StartNew();
                var resultado = AsociarConIndexacion(periodos, lotes);
                sw.Stop();

                if (resultado.Count != n)
                {
                    Console.Error.WriteLine($"ERROR: Cantidad inesperada en indexación ({resultado.Count} != {n}).");
                    Environment.Exit(1);
                    return;
                }

                tiemposIndex[r] = sw.Elapsed.TotalMilliseconds;
            }

            double promedioBruta = tiemposBruta.Average();
            double promedioIndex = tiemposIndex.Average();
            resumen.Add((n, promedioBruta, promedioIndex));

            Console.WriteLine();
            Console.WriteLine($"[TAMAÑO n={n:N0}, k={lotes.Count:N0}] Verificación previa: IDÉNTICAS ({preIndex.Count} asociaciones)");
            Console.WriteLine("  Corrida     Fuerza Bruta (ms)     Indexación (ms)");
            Console.WriteLine("  -------------------------------------------------");
            for (int r = 0; r < repeticiones; r++)
            {
                Console.WriteLine($"  {r + 1,-10} {tiemposBruta[r],16:F4} {tiemposIndex[r],19:F4}");
            }
            Console.WriteLine("  -------------------------------------------------");
            Console.WriteLine($"  {"PROMEDIO",-10} {promedioBruta,16:F4} {promedioIndex,19:F4}");
        }

        // ------------------------------------------------------------------
        // 3. TABLA RESUMEN EN FORMATO MARKDOWN
        // ------------------------------------------------------------------
        Console.WriteLine();
        Console.WriteLine("====================================================================");
        Console.WriteLine("                     RESUMEN COMPARATIVO (ms)");
        Console.WriteLine("====================================================================");
        Console.WriteLine();
        Console.WriteLine("| Tamaño (n = k) | Fuerza bruta (ms) | Factor respecto al anterior | Indexación (ms) | Factor respecto al anterior |");
        Console.WriteLine("|---:|---:|---:|---:|---:|");

        double? anteriorBruta = null;
        double? anteriorIndex = null;

        foreach (var fila in resumen)
        {
            string factorBruta = anteriorBruta.HasValue
                ? $"{(fila.FuerzaBrutaMs / anteriorBruta.Value):F2}x"
                : "—";

            string factorIndex = anteriorIndex.HasValue
                ? $"{(fila.IndexacionMs / anteriorIndex.Value):F2}x"
                : "—";

            Console.WriteLine($"| {fila.N} | {fila.FuerzaBrutaMs:F4} | {factorBruta} | {fila.IndexacionMs:F4} | {factorIndex} |");

            anteriorBruta = fila.FuerzaBrutaMs;
            anteriorIndex = fila.IndexacionMs;
        }
        Console.WriteLine();
    }

    // Estrategia A: Fuerza Bruta -> O(n * k)
    // Para cada período, recorre la colección de FACLOTES secuencialmente hasta encontrar la clave compuesta.
    private static List<AsociacionPeriodoLote> AsociarFuerzaBruta(
        List<PeriodoLectura> periodos,
        List<FacLoteRegistro> facLotes)
    {
        var asociaciones = new List<AsociacionPeriodoLote>(periodos.Count);

        for (int i = 0; i < periodos.Count; i++)
        {
            var periodo = periodos[i];
            FacLoteRegistro? encontrado = null;

            for (int j = 0; j < facLotes.Count; j++)
            {
                var lote = facLotes[j];
                if (lote.IdRuta == periodo.IdRuta &&
                    lote.Anio == periodo.Anio &&
                    lote.Nroper == periodo.Mes)
                {
                    encontrado = lote;
                    break;
                }
            }

            if (encontrado is null)
            {
                throw new InvalidOperationException(
                    $"No se encontró lote en fuerza bruta para el período {periodo.Id} ({periodo.IdRuta}, {periodo.Anio}, {periodo.Mes}).");
            }

            asociaciones.Add(new AsociacionPeriodoLote
            {
                PeriodoId = periodo.Id,
                IdLote = encontrado.IdLote,
                IdRuta = periodo.IdRuta,
                Anio = periodo.Anio,
                Mes = periodo.Mes,
                FechaHasta = encontrado.FechaHasta.ToString("dd-MM-yyyy")
            });
        }

        return asociaciones;
    }

    // Estrategia B: Indexación con Dictionary -> O(k) construcción + n * O(1) esperado = O(k + n)
    // Incluye explícitamente la construcción del índice en el tiempo medido.
    private static List<AsociacionPeriodoLote> AsociarConIndexacion(
        List<PeriodoLectura> periodos,
        List<FacLoteRegistro> facLotes)
    {
        // 1. Construcción del índice: O(k) esperado
        var indiceLotes = new Dictionary<(int IdRuta, int Anio, int Nroper), FacLoteRegistro>(facLotes.Count);
        for (int j = 0; j < facLotes.Count; j++)
        {
            var lote = facLotes[j];
            indiceLotes[(lote.IdRuta, lote.Anio, lote.Nroper)] = lote;
        }

        // 2. Consultas al índice por clave compuesta: n * O(1) esperado
        var asociaciones = new List<AsociacionPeriodoLote>(periodos.Count);
        for (int i = 0; i < periodos.Count; i++)
        {
            var periodo = periodos[i];
            var clave = (periodo.IdRuta, periodo.Anio, periodo.Mes);

            if (!indiceLotes.TryGetValue(clave, out var encontrado))
            {
                throw new InvalidOperationException(
                    $"No se encontró lote en índice para el período {periodo.Id} ({periodo.IdRuta}, {periodo.Anio}, {periodo.Mes}).");
            }

            asociaciones.Add(new AsociacionPeriodoLote
            {
                PeriodoId = periodo.Id,
                IdLote = encontrado.IdLote,
                IdRuta = periodo.IdRuta,
                Anio = periodo.Anio,
                Mes = periodo.Mes,
                FechaHasta = encontrado.FechaHasta.ToString("dd-MM-yyyy")
            });
        }

        return asociaciones;
    }

    // Genera n períodos y k = n lotes de forma 100% determinística (sin Random)
    // con correspondencia 1 a 1 mediante clave compuesta única (IdRuta, Anio, Nroper).
    private static (List<PeriodoLectura> Periodos, List<FacLoteRegistro> Lotes) GenerarDatosDeterministicos(int n)
    {
        var periodos = new List<PeriodoLectura>(n);
        var lotesOrdenados = new List<FacLoteRegistro>(n);
        var fechaBase = new DateTime(2026, 11, 28);

        for (int i = 0; i < n; i++)
        {
            int idRuta = 100 + (i / 12);
            int anio = 2020 + ((i / 12) % 7);
            int mes = (i % 12) + 1;

            periodos.Add(new PeriodoLectura
            {
                Id = i + 1,
                IdSuministro = 130000 + i,
                IdRuta = idRuta,
                Anio = anio,
                Mes = mes
            });

            lotesOrdenados.Add(new FacLoteRegistro
            {
                IdLote = 500000 + i,
                IdRuta = idRuta,
                Anio = anio,
                Nroper = mes,
                FechaHasta = fechaBase.AddDays(-(i % 28))
            });
        }

        // Permutación lineal modular determinística: como 199 es primo y coprimo con n
        // (n = 250, 500, 1000, 2000, 4000 solo tienen factores 2 y 5),
        // (i * 199 + 7) % n produce una biyección exacta sin usar aleatoriedad.
        const int strideCoprimo = 199;
        const int desplazamiento = 7;
        var lotesPermutados = new List<FacLoteRegistro>(n);
        for (int i = 0; i < n; i++)
        {
            int indiceOrigen = (int)(((long)i * strideCoprimo + desplazamiento) % n);
            lotesPermutados.Add(lotesOrdenados[indiceOrigen]);
        }

        return (periodos, lotesPermutados);
    }

    private static bool VerificarResultados(
        List<AsociacionPeriodoLote> bruta,
        List<AsociacionPeriodoLote> indexacion,
        int cantidadEsperada,
        out string? error)
    {
        if (bruta.Count != cantidadEsperada)
        {
            error = $"Fuerza bruta devolvió {bruta.Count} asociaciones, se esperaban {cantidadEsperada}.";
            return false;
        }

        if (indexacion.Count != cantidadEsperada)
        {
            error = $"Indexación devolvió {indexacion.Count} asociaciones, se esperaban {cantidadEsperada}.";
            return false;
        }

        for (int i = 0; i < cantidadEsperada; i++)
        {
            var a = bruta[i];
            var b = indexacion[i];

            if (a.PeriodoId != b.PeriodoId ||
                a.IdLote != b.IdLote ||
                a.IdRuta != b.IdRuta ||
                a.Anio != b.Anio ||
                a.Mes != b.Mes ||
                a.FechaHasta != b.FechaHasta)
            {
                error = $"Discrepancia en el índice {i}: FuerzaBruta(Periodo={a.PeriodoId}, Lote={a.IdLote}, Fecha={a.FechaHasta}) != Indexación(Periodo={b.PeriodoId}, Lote={b.IdLote}, Fecha={b.FechaHasta}).";
                return false;
            }
        }

        error = null;
        return true;
    }

    private static void LimpiarGC()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}
