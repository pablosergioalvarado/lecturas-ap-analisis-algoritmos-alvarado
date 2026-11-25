using System.Diagnostics;
using System.Globalization;

namespace MedicionH2;

// Modelos que representan fielmente las entidades reales del proceso GET /periodos
public sealed class SuministroSim
{
    public int Idsuministro { get; init; }
    public int Idruta { get; init; }
}

public sealed class PeriodoLecturasApSim
{
    public int Id { get; init; }
    public int Anio { get; init; }
    public int Mes { get; init; }
    public int IdSuministro { get; init; }
    public SuministroSim Suministro { get; init; } = null!;
}

public sealed class FacLoteSim
{
    public int Idlote { get; init; }
    public int Idruta { get; init; }
    public int Anio { get; init; }
    public int Nroper { get; init; }
    public DateTime? FechaHasta { get; init; }
}

public sealed class PeriodoLecturasApDtoSim
{
    public int Id { get; init; }
    public int Anio { get; init; }
    public int Mes { get; init; }
    public int IdSuministro { get; init; }
    public string? FacLoteFechaHasta { get; init; }
}

public static class Program
{
   
public static void Main()
{
    CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

    int[] tamanios = { 1500, 3000, 6000, 12000 };
    const int repeticiones = 3;

    Console.WriteLine();
    Console.WriteLine("==============================================================");
    Console.WriteLine("                 MEDICIÓN DE ALGORITMOS - H2");
    Console.WriteLine("==============================================================");
    Console.WriteLine("Problema : Mapeo de períodos y búsqueda de FacLoteFechaHasta");
    Console.WriteLine("Ingenua  : Lista + búsqueda secuencial");
    Console.WriteLine("Elegida  : Dictionary (Tabla Hash)");
    Console.WriteLine($"Protocolo: {repeticiones} mediciones por tamaño + promedio");
    Console.WriteLine("==============================================================");

    // ----------------------------------------------------------
    // CALENTAMIENTO
    // ----------------------------------------------------------

    Console.WriteLine();
    Console.WriteLine("[CALENTAMIENTO]");

    var (periodosWarmup, lotesWarmup) = GenerarDatos(500);

    var resIngenuoWarmup =
        MapearConListaIngenua(periodosWarmup, lotesWarmup);

    var resHashWarmup =
        MapearConDictionary(periodosWarmup, lotesWarmup);

    VerificarIgualdad(resIngenuoWarmup, resHashWarmup);

    Console.WriteLine("  Datos utilizados : 500 períodos / 500 lotes");
    Console.WriteLine("  Ingenua          : Ejecutada");
    Console.WriteLine("  Elegida (Hash)   : Ejecutada");
    Console.WriteLine("  Verificación     : RESULTADOS EQUIVALENTES");
    Console.WriteLine("  Estado           : COMPLETADO");

    // ----------------------------------------------------------
    // MEDICIONES
    // ----------------------------------------------------------

    var resultados = new List<(int N, double Ingenua, double Hash)>();

    foreach (int n in tamanios)
    {
        var (periodos, lotes) = GenerarDatos(n);

        double[] tiemposIngenua = new double[repeticiones];
        double[] tiemposHash = new double[repeticiones];

        List<PeriodoLecturasApDtoSim>? salidaIngenua = null;
        List<PeriodoLecturasApDtoSim>? salidaHash = null;

        Console.WriteLine();
        Console.WriteLine("==============================================================");
        Console.WriteLine($"TAMAÑO: {n:N0} períodos / {n:N0} lotes");
        Console.WriteLine("==============================================================");
        Console.WriteLine();
        Console.WriteLine("  Corrida     Ingenua (ms)      Elegida (ms)");
        Console.WriteLine("  ------------------------------------------");

        // Medición de la solución ingenua
        for (int r = 0; r < repeticiones; r++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var sw = Stopwatch.StartNew();
            salidaIngenua = MapearConListaIngenua(periodos, lotes);
            sw.Stop();

            tiemposIngenua[r] = sw.Elapsed.TotalMilliseconds;
        }

        // Medición de la solución elegida
        for (int r = 0; r < repeticiones; r++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var sw = Stopwatch.StartNew();
            salidaHash = MapearConDictionary(periodos, lotes);
            sw.Stop();

            tiemposHash[r] = sw.Elapsed.TotalMilliseconds;
        }

        // Verificación de equivalencia
        VerificarIgualdad(salidaIngenua!, salidaHash!);

        // Mostrar las corridas individuales
        for (int r = 0; r < repeticiones; r++)
        {
            Console.WriteLine(
                $"  {r + 1,-10} {tiemposIngenua[r],12:F4} " +
                $"{tiemposHash[r],18:F4}");
        }

        // Calcular promedios
        double promedioIngenua = tiemposIngenua.Average();
        double promedioHash = tiemposHash.Average();

        Console.WriteLine("  ------------------------------------------");
        Console.WriteLine(
            $"  {"PROMEDIO",-10} {promedioIngenua,12:F4} " +
            $"{promedioHash,18:F4}");

        resultados.Add((n, promedioIngenua, promedioHash));

        Console.WriteLine();
        Console.WriteLine("  Verificación: RESULTADOS EQUIVALENTES");
    }

    // ----------------------------------------------------------
    // RESUMEN COMPARATIVO
    // ----------------------------------------------------------

    Console.WriteLine();
    Console.WriteLine("==============================================================");
    Console.WriteLine("                  RESUMEN COMPARATIVO");
    Console.WriteLine("==============================================================");
    Console.WriteLine();

    Console.WriteLine(
        $"{"Tamaño",-10} {"Ingenua (ms)",15} {"Factor",10} {"Hash (ms)",15} {"Factor",10}");

    Console.WriteLine(new string('-', 65));

    double? anteriorIngenua = null;
    double? anteriorHash = null;

    foreach (var resultado in resultados)
    {
        string factorIngenua = anteriorIngenua.HasValue
            ? $"{resultado.Ingenua / anteriorIngenua.Value:F2}x"
            : "—";

        string factorHash = anteriorHash.HasValue
            ? $"{resultado.Hash / anteriorHash.Value:F2}x"
            : "—";

        Console.WriteLine(
            $"{resultado.N,-10:N0} " +
            $"{resultado.Ingenua,15:F4} " +
            $"{factorIngenua,10} " +
            $"{resultado.Hash,15:F4} " +
            $"{factorHash,10}");

        anteriorIngenua = resultado.Ingenua;
        anteriorHash = resultado.Hash;
    }

    Console.WriteLine(new string('-', 65));
    Console.WriteLine();
    Console.WriteLine("==============================================================");
    Console.WriteLine("                  FIN DE LAS MEDICIONES");
    Console.WriteLine("==============================================================");
}

    // Solución Ingenua: almacena los lotes en una lista secuencial (List<FacLoteSim>)
    // y por cada período recorre linealmente la lista buscando (Idruta, Anio, Nroper).
    private static List<PeriodoLecturasApDtoSim> MapearConListaIngenua(
        List<PeriodoLecturasApSim> periodos,
        List<FacLoteSim> lotesRecuperados)
    {
        
        var resultado = new List<PeriodoLecturasApDtoSim>(periodos.Count);
        foreach (var p in periodos)
        {
            int idRuta = p.Suministro.Idruta;
            int anio = p.Anio;
            int mes = p.Mes;

            FacLoteSim? facLote = null;
            for (int i = 0; i < lotesRecuperados.Count; i++)
            {
                var candidato = lotesRecuperados[i];
                if (candidato.Idruta == idRuta && candidato.Anio == anio && candidato.Nroper == mes)
                {
                    facLote = candidato;
                    break;
                }
            }

            resultado.Add(new PeriodoLecturasApDtoSim
            {
                Id = p.Id,
                Anio = p.Anio,
                Mes = p.Mes,
                IdSuministro = p.IdSuministro,
                FacLoteFechaHasta = facLote?.FechaHasta?.ToString("dd-MM-yyyy")
            });
        }

        return resultado;
    }

    // Solución Elegida: indexa los lotes en un Dictionary<(int Idruta, int Anio, int Nroper), string?>
    // y resuelve la fecha de cada período en O(1) promedio mediante TryGetValue.
    private static List<PeriodoLecturasApDtoSim> MapearConDictionary(
        List<PeriodoLecturasApSim> periodos,
        List<FacLoteSim> lotesRecuperados)
    {
        var lotesPorClave = new Dictionary<(int Idruta, int Anio, int Nroper), string?>(lotesRecuperados.Count);
        foreach (var lote in lotesRecuperados)
        {
            var clave = (lote.Idruta, lote.Anio, lote.Nroper);
            lotesPorClave[clave] = lote.FechaHasta?.ToString("dd-MM-yyyy");
        }

        var resultado = new List<PeriodoLecturasApDtoSim>(periodos.Count);
        foreach (var p in periodos)
        {
            var clave = (p.Suministro.Idruta, p.Anio, p.Mes);
            lotesPorClave.TryGetValue(clave, out string? fechaHasta);

            resultado.Add(new PeriodoLecturasApDtoSim
            {
                Id = p.Id,
                Anio = p.Anio,
                Mes = p.Mes,
                IdSuministro = p.IdSuministro,
                FacLoteFechaHasta = fechaHasta
            });
        }

        return resultado;
    }

    // Genera n períodos y k = n lotes con claves únicas (Idruta, Anio, Nroper),
    // respetando la relación 1 a 1 sin duplicados verificada en ERP_ELFEC.FAC_LOTES.
    private static (List<PeriodoLecturasApSim> Periodos, List<FacLoteSim> Lotes) GenerarDatos(int n)
    {
        var periodos = new List<PeriodoLecturasApSim>(n);
        var lotes = new List<FacLoteSim>(n);
        var fechaBase = new DateTime(2026, 11, 28);

        for (int i = 0; i < n; i++)
        {
            int idRuta = 100 + (i / 12);
            int anio = 2020 + ((i / 12) % 7);
            int mes = (i % 12) + 1;

            periodos.Add(new PeriodoLecturasApSim
            {
                Id = i + 1,
                Anio = anio,
                Mes = mes,
                IdSuministro = 130000 + i,
                Suministro = new SuministroSim
                {
                    Idsuministro = 130000 + i,
                    Idruta = idRuta
                }
            });

            lotes.Add(new FacLoteSim
            {
                Idlote = 500000 + i,
                Idruta = idRuta,
                Anio = anio,
                Nroper = mes,
                FechaHasta = fechaBase.AddDays(-(i % 28))
            });
        }

        // Desordenamos determinísticamente la lista de lotes (semilla fija 42)
        // para representar el caso promedio de búsqueda en una colección no ordenada.
        var rng = new Random(42);
        for (int i = lotes.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (lotes[i], lotes[j]) = (lotes[j], lotes[i]);
        }

        return (periodos, lotes);
    }

    private static void VerificarIgualdad(
        List<PeriodoLecturasApDtoSim> a,
        List<PeriodoLecturasApDtoSim> b)
    {
        if (a.Count != b.Count)
            throw new InvalidOperationException("Las longitudes de salida no coinciden.");

        for (int i = 0; i < a.Count; i++)
        {
            if (a[i].Id != b[i].Id ||
                a[i].Anio != b[i].Anio ||
                a[i].Mes != b[i].Mes ||
                a[i].IdSuministro != b[i].IdSuministro ||
                a[i].FacLoteFechaHasta != b[i].FacLoteFechaHasta ||
                string.IsNullOrEmpty(a[i].FacLoteFechaHasta))
            {
                throw new InvalidOperationException($"Diferencia o lote no resuelto en el elemento {i}.");
            }
        }
    }
}
