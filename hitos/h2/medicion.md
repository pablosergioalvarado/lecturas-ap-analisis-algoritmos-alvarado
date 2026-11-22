# H2 — Medición Experimental de Estructuras de Datos

**Caso integrador:** Resolución en memoria de `FacLoteFechaHasta` para cada período durante el mapeo de `GET /api/AlumbradoPublico/periodos` (`FacLoteFechaResolver`).

---

## 1. Qué se mide y protocolo experimental

Se mide **exclusivamente el trabajo en memoria RAM** que justifica la elección de la estructura en `estructura.md` (**construir la estructura con los $k$ lotes `FacLote` ya cargados y resolver las $n$ búsquedas por clave exacta `(Idruta, Anio, Nroper)` al mapear los $n$ objetos `PeriodoLecturasAp` hacia `PeriodoLecturasApDto`** con $k = n$ claves únicas 1 a 1), **excluyendo las consultas SQL a Oracle, la latencia de red y el resto del endpoint HTTP**.

- **Implementación ingenua (`List<FacLoteSim>`):** Almacena los $k$ lotes en una lista secuencial y, para cada uno de los $n$ períodos, busca su lote recorriendo linealmente la lista hasta encontrar `Idruta == p.Suministro.Idruta && Anio == p.Anio && Nroper == p.Mes`. Complejidad esperada: **$O(n \cdot k) = O(n^2)$** cuando $k = n$.
- **Implementación elegida (`Dictionary<(int Idruta, int Anio, int Nroper), string?>`):** Inserta los $k$ lotes en una tabla hash indexada por la tupla `(Idruta, Anio, Nroper)` y, para cada uno de los $n$ períodos, obtiene `FechaHasta` mediante `TryGetValue`. Complejidad esperada en caso promedio: **$O(n + k) = O(n)$** cuando $k = n$.
- **Protocolo aplicado:**
  1. **Cuatro tamaños que se duplican progresivamente:** $n \in \{1500, 3000, 6000, 12000\}$, partiendo del volumen histórico actual en producción ($1.534$ períodos en `MOVILES.PERIODO_LECTURAS_AP`).
  2. **Calentamiento (*Warmup*):** Se ejecutan ambas implementaciones con $n = 500$ antes de iniciar el cronómetro para que el compilador JIT de .NET compile los métodos en condiciones iguales.
  3. **Repeticiones y promedio:** Para cada tamaño e implementación se fuerza una limpieza de recolector de basura (`GC.Collect()`), se mide con `Stopwatch` en tres repeticiones independientes y se calcula el promedio en milisegundos.
  4. **Verificación de equivalencia:** Fuera de la ventana del cronómetro, se comprueba para cada tamaño que ambas implementaciones devuelvan exactamente la misma cantidad de elementos y los mismos valores en todos los campos del DTO (`Id`, `Anio`, `Mes`, `IdSuministro` y `FacLoteFechaHasta` no nulo).

---

## 2. Resultados reales obtenidos

| Tamaño de datos | Ingenua (ms) | Factor respecto al tamaño anterior | Elegida (ms) | Factor respecto al tamaño anterior |
|---|---:|---:|---:|---:|
| 1500 | 0.9068 | — | 0.2377 | — |
| 3000 | 3.2327 | 3.56x | 0.4990 | 2.10x |
| 6000 | 13.2208 | 4.09x | 1.0241 | 2.05x |
| 12000 | 52.8658 | 4.00x | 1.7993 | 1.76x |

---

## 3. Cómo ejecutar la medición

Desde la raíz del repositorio, ejecutar en terminal:

```powershell
dotnet run --project hitos/h2/Medicion.csproj -c Release
```

*(El proyecto [`Medicion.csproj`](./Medicion.csproj) compila [`Medicion.cs`](./Medicion.cs) en modo `Release` redirigiendo los artefactos temporales de compilación a `%TEMP%` para mantener limpio el directorio `hitos/h2/`).*

### Código ejecutable utilizado ([`hitos/h2/Medicion.cs`](./Medicion.cs))

```csharp
using System.Diagnostics;
using System.Globalization;

namespace MedicionH2;

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

        Console.WriteLine();
        Console.WriteLine("[CALENTAMIENTO]");

        var (periodosWarmup, lotesWarmup) = GenerarDatos(500);
        var resIngenuoWarmup = MapearConListaIngenua(periodosWarmup, lotesWarmup);
        var resHashWarmup = MapearConDictionary(periodosWarmup, lotesWarmup);
        VerificarIgualdad(resIngenuoWarmup, resHashWarmup);

        Console.WriteLine("  Datos utilizados : 500 períodos / 500 lotes");
        Console.WriteLine("  Ingenua          : Ejecutada");
        Console.WriteLine("  Elegida (Hash)   : Ejecutada");
        Console.WriteLine("  Verificación     : RESULTADOS EQUIVALENTES");
        Console.WriteLine("  Estado           : COMPLETADO");

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

            VerificarIgualdad(salidaIngenua!, salidaHash!);

            for (int r = 0; r < repeticiones; r++)
            {
                Console.WriteLine($"  {r + 1,-10} {tiemposIngenua[r],12:F4} {tiemposHash[r],18:F4}");
            }

            double promedioIngenua = tiemposIngenua.Average();
            double promedioHash = tiemposHash.Average();

            Console.WriteLine("  ------------------------------------------");
            Console.WriteLine($"  {"PROMEDIO",-10} {promedioIngenua,12:F4} {promedioHash,18:F4}");

            resultados.Add((n, promedioIngenua, promedioHash));

            Console.WriteLine();
            Console.WriteLine("  Verificación: RESULTADOS EQUIVALENTES");
        }

        Console.WriteLine();
        Console.WriteLine("==============================================================");
        Console.WriteLine("                  RESUMEN COMPARATIVO");
        Console.WriteLine("==============================================================");
        Console.WriteLine();
        Console.WriteLine($"{"Tamaño",-10} {"Ingenua (ms)",15} {"Factor",10} {"Hash (ms)",15} {"Factor",10}");
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

    private static List<PeriodoLecturasApDtoSim> MapearConListaIngenua(
        List<PeriodoLecturasApSim> periodos,
        List<FacLoteSim> lotesRecuperados)
    {
        var listaLotes = new List<FacLoteSim>(lotesRecuperados.Count);
        foreach (var lote in lotesRecuperados)
        {
            listaLotes.Add(lote);
        }

        var resultado = new List<PeriodoLecturasApDtoSim>(periodos.Count);
        foreach (var p in periodos)
        {
            int idRuta = p.Suministro.Idruta;
            int anio = p.Anio;
            int mes = p.Mes;

            FacLoteSim? facLote = null;
            for (int i = 0; i < listaLotes.Count; i++)
            {
                var candidato = listaLotes[i];
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
```

---

## 4. Conclusión

Al duplicar progresivamente el tamaño de entrada ($1500 \to 3000 \to 6000 \to 12000$), los tiempos medidos son consistentes con los costos teóricos esperados: la solución ingenua con lista secuencial (`List<FacLote>`) multiplica su tiempo por un factor cercano a $4\times$ ($3,56\times$, $4,09\times$ y $4,00\times$) acorde a un comportamiento cuadrático $O(n^2)$ cuando $k = n$.
En cambio, la estructura elegida (`Dictionary<(int Idruta, int Anio, int Nroper), string?>`) multiplica su tiempo por un factor cercano a $2\times$ ($2,10\times$, $2,05\times$ y $1,76\times$), coherente con un costo lineal en caso promedio $O(n + k) = O(n)$ para la carga y las $n$ búsquedas por clave exacta.
Las ligeras variaciones respecto de los factores ideales ($4,00\times$ y $2,00\times$) se explican por el ruido propio del entorno de ejecución (efectos de caché de CPU, planificación del sistema operativo y reserva de memoria para las cadenas formateadas), con la limitación de que el experimento evalúa datos sintéticos en memoria local con tres repeticiones por punto.
Por ello, estas mediciones no constituyen una demostración matemática de la complejidad asintótica, pero aportan evidencia empírica clara de la ventaja práctica de indexar los lotes en una tabla hash para nuestro proceso `FacLoteFechaResolver`.
