// PARCIAL 1 · VARIANTE A — "Registro de carnets"
// Problema: dada una lista de carnets, ¿hay alguno repetido?
// La función HayRepetidosPorPares es CORRECTA pero LENTA. Tu trabajo: analizarla, CURARLA (escribir una
// versión que cueste menos), medir las dos y elegir. Puedes usar este archivo tal cual (si tienes .NET)
// o portar todo a TU lenguaje.

using System.Diagnostics;
using System.Collections.Generic;

namespace Parcial1.VarianteA;

public static class Registro
{
    // Solución ACTUAL (lenta): comparar cada carnet con todos los que le siguen.
    public static bool HayRepetidosPorPares(int[] carnets)
    {
        for (int i = 0; i < carnets.Length; i++)
            for (int j = i + 1; j < carnets.Length; j++)
                if (carnets[i] == carnets[j]) return true;
        return false;
    }

    // TU CURA: escribe aquí una versión correcta que cueste MENOS que O(n²).
    // No modifiques la lista que recibes (trabaja sobre una copia si la necesitas ordenada).
    public static bool HayRepetidosCurado(int[] carnets)
    {
        var vistos = new HashSet<int>();
        foreach (int carnet in carnets)
        {
            if (!vistos.Add(carnet)) return true;
        }

        return false;
    }

    // El PEOR CASO para las dos: una lista SIN repetidos (hay que mirarlo todo).
    public static int[] GenerarSinRepetidos(int n, int semilla = 42)
    {
        var datos = new int[n];
        for (int i = 0; i < n; i++) datos[i] = i;
        var azar = new Random(semilla);
        for (int i = n - 1; i > 0; i--)
        {
            int k = azar.Next(i + 1);
            (datos[i], datos[k]) = (datos[k], datos[i]);
        }
        return datos;
    }
}

public static class Programa
{
    // Los cinco casos que tu cura debe pasar (la esperada va a la derecha).
    private static readonly (int[] datos, bool esperado, string que)[] Casos =
    {
        (new[] { 7, 3, 9, 3, 5 },   true,  "repetido en el medio"),
        (new[] { 7, 3, 9, 4, 5 },   false, "sin repetidos"),
        (new int[] { },             false, "lista vacía"),
        (new[] { 8 },               false, "un solo elemento"),
        (new[] { 4, 1, 2, 6, 4 },   true,  "repetido en los extremos"),
    };

    /*public static void Main()
    {
        foreach (var (datos, esperado, que) in Casos)
        {
            bool lenta = Registro.HayRepetidosPorPares(datos);
            string curada;
            try { curada = Registro.HayRepetidosCurado(datos).ToString(); }
            catch (NotImplementedException) { curada = "(sin escribir)"; }
            Console.WriteLine($"{que,-26} esperado {esperado,-5} · por pares {lenta,-5} · tu cura {curada}");
        }

        // Una medición de ejemplo, SIN protocolo (el protocolo lo agregas tú en tu medición):
        var grande = Registro.GenerarSinRepetidos(5_000);
        var reloj = Stopwatch.StartNew();
        Registro.HayRepetidosPorPares(grande);
        Console.WriteLine($"Por pares con 5.000 datos: {reloj.ElapsedMilliseconds} ms (una sola medición: no es evidencia)");
    }*/

    public static void Main()
    {
        // 1. Comprobar los cinco casos del docente
        Console.WriteLine("=== PRUEBAS DE CORRECCIÓN ===");

        foreach (var (datos, esperado, que) in Casos)
        {
            bool lenta = Registro.HayRepetidosPorPares(datos);
            bool curada = Registro.HayRepetidosCurado(datos);

            Console.WriteLine(
                $"{que,-26} esperado {esperado,-5} · por pares {lenta,-5} · tu cura {curada,-5}");
        }

        // 2. Cuatro tamaños duplicados: n, 2n, 4n y 8n
        int[] tamanos = { 5_000, 10_000, 20_000, 40_000 };

        double tiempoAnteriorPares = 0;
        double tiempoAnteriorCura = 0;

        Console.WriteLine("\n=== MEDICIÓN DE RENDIMIENTO (Release) ===");
        Console.WriteLine(
            $"{"n",10} {"Pares (ms)",15} {"Cura (ms)",15} {"Factor pares",15} {"Factor cura",15}");

        foreach (int n in tamanos)
        {
            // 3. Generar datos sin repetidos: peor caso
            int[] datos = Registro.GenerarSinRepetidos(n);

            // 4. Calentamiento: una ejecución de cada método
            Registro.HayRepetidosPorPares(datos);
            Registro.HayRepetidosCurado(datos);

            // 5. Tres mediciones del método por pares
            double sumaPares = 0;

            for (int i = 0; i < 3; i++)
            {
                var reloj = Stopwatch.StartNew();
                bool resultado = Registro.HayRepetidosPorPares(datos);
                reloj.Stop();

                if (resultado)
                    throw new Exception("Se esperaba false en una lista sin repetidos.");

                sumaPares += reloj.Elapsed.TotalMilliseconds;
            }

            double promedioPares = sumaPares / 3;

            // 6. Tres mediciones de la cura
            double sumaCura = 0;

            for (int i = 0; i < 3; i++)
            {
                var reloj = Stopwatch.StartNew();
                bool resultado = Registro.HayRepetidosCurado(datos);
                reloj.Stop();

                if (resultado)
                    throw new Exception("Se esperaba false en una lista sin repetidos.");

                sumaCura += reloj.Elapsed.TotalMilliseconds;
            }

            double promedioCura = sumaCura / 3;

            // 7. Factores respecto del tamaño anterior
            string factorPares = tiempoAnteriorPares == 0
                ? "-"
                : (promedioPares / tiempoAnteriorPares).ToString("F2") + "x";

            string factorCura = tiempoAnteriorCura == 0
                ? "-"
                : (promedioCura / tiempoAnteriorCura).ToString("F2") + "x";

            // 8. Mostrar la fila
            Console.WriteLine(
                $"{n,10:N0} {promedioPares,15:F3} {promedioCura,15:F3} {factorPares,15} {factorCura,15}");

            tiempoAnteriorPares = promedioPares;
            tiempoAnteriorCura = promedioCura;
        }

        Console.WriteLine("\nMedición finalizada.");
    }
}
