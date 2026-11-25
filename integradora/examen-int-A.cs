// EVALUACIÓN INTEGRADORA · VARIANTE A — "Furgoneta de reparto"
// Caso: una distribuidora tiene una lista de paquetes (cada uno con un peso en kg y una ganancia en Bs) y una furgoneta
// que aguanta una capacidad en kg. Hay que elegir QUÉ paquetes llevar para ganar lo máximo sin pasarse de la capacidad.
// Cada paquete se lleva entero o no se lleva.
// PlanificadorIngenuo es CORRECTO pero LENTO (prueba todas las combinaciones). Tu trabajo: analizarlo, elegir la
// estrategia, APLICARLA en PlanificadorMejorado, medir la diferencia y justificar todo.
// Puedes usar este archivo tal cual (si tienes .NET) o portar todo a TU lenguaje.

// using System.Diagnostics;

// namespace Integradora.VarianteA;

// public sealed record Paquete(string Codigo, int Kg, int Ganancia);

// public interface IPlanificador
// {
//     // Devuelve la ganancia máxima y los códigos de los paquetes elegidos (en cualquier orden).
//     (int ganancia, List<string> codigos) Elegir(Paquete[] paquetes, int capacidadKg);

//     // Después de Elegir: ¿este paquete va en la furgoneta?
//     bool EstaEnElViaje(string codigo);
// }

// // ---------- SOLUCIÓN INGENUA: prueba todos los subconjuntos (2^n) ----------
// public sealed class PlanificadorIngenuo : IPlanificador
// {
//     private List<string> _elegidos = new();

//     public (int ganancia, List<string> codigos) Elegir(Paquete[] paquetes, int capacidadKg)
//     {
//         int n = paquetes.Length, mejor = 0;
//         long mejorMascara = 0;
//         for (long mascara = 0; mascara < (1L << n); mascara++)
//         {
//             int kg = 0, ganancia = 0;
//             for (int i = 0; i < n; i++)
//                 if ((mascara & (1L << i)) != 0) { kg += paquetes[i].Kg; ganancia += paquetes[i].Ganancia; }
//             if (kg <= capacidadKg && ganancia > mejor) { mejor = ganancia; mejorMascara = mascara; }
//         }
//         _elegidos = new List<string>();
//         for (int i = 0; i < n; i++) if ((mejorMascara & (1L << i)) != 0) _elegidos.Add(paquetes[i].Codigo);
//         return (mejor, new List<string>(_elegidos));
//     }

//     public bool EstaEnElViaje(string codigo) => _elegidos.Contains(codigo);          // recorre la lista
// }

// // ---------- TU SOLUCIÓN: elige la estrategia y la(s) estructura(s) y escríbela aquí ----------
// public sealed class PlanificadorMejorado : IPlanificador
// {
//     public (int ganancia, List<string> codigos) Elegir(Paquete[] paquetes, int capacidadKg) => throw new NotImplementedException("Escribe aquí tu solución.");
//     public bool EstaEnElViaje(string codigo) => throw new NotImplementedException();
// }

// public static class Programa
// {
//     private static Paquete[] Lote(params (string c, int kg, int g)[] datos) => datos.Select(d => new Paquete(d.c, d.kg, d.g)).ToArray();

//     // Los casos de prueba (todos con ÓPTIMO ÚNICO): (paquetes, capacidad, ganancia esperada, códigos esperados).
//     private static readonly (string que, Paquete[] paquetes, int capacidad, int esperada, string[] codigos)[] Casos =
//     {
//         ("el voraz se equivoca", Lote(("P1", 10, 60), ("P2", 20, 100), ("P3", 30, 120)), 50, 220, new[] { "P2", "P3" }),
//         ("cuatro paquetes",      Lote(("P1", 2, 3), ("P2", 3, 4), ("P3", 4, 5), ("P4", 5, 6)), 8, 10, new[] { "P2", "P4" }),
//         ("capacidad cero",       Lote(("P1", 5, 10), ("P2", 7, 12)), 0, 0, Array.Empty<string>()),
//         ("sin paquetes",         Lote(), 30, 0, Array.Empty<string>()),
//         ("nada cabe",            Lote(("P1", 40, 90), ("P2", 50, 100)), 30, 0, Array.Empty<string>()),
//         ("entran todos",         Lote(("P1", 5, 10), ("P2", 7, 12), ("P3", 3, 4)), 100, 26, new[] { "P1", "P2", "P3" }),
//     };

//     private static void Probar(string titulo, Func<IPlanificador> fabrica)
//     {
//         int bien = 0, total = 0;
//         try
//         {
//             foreach (var (que, paquetes, capacidad, esperada, codigos) in Casos)
//             {
//                 var plan = fabrica();
//                 var (ganancia, elegidos) = plan.Elegir(paquetes, capacidad);
//                 total++;
//                 bool ok = ganancia == esperada && elegidos.OrderBy(x => x).SequenceEqual(codigos.OrderBy(x => x));
//                 if (ok) bien++;
//                 else Console.WriteLine($"   MAL en «{que}»: esperado {esperada} con [{string.Join(",", codigos)}], obtuvo {ganancia} con [{string.Join(",", elegidos)}]");
//             }
//             var p = fabrica();
//             p.Elegir(Casos[0].paquetes, Casos[0].capacidad);
//             total += 2;
//             if (p.EstaEnElViaje("P2")) bien++; else Console.WriteLine("   MAL en «EstaEnElViaje(P2)»: esperado True");
//             if (!p.EstaEnElViaje("P1")) bien++; else Console.WriteLine("   MAL en «EstaEnElViaje(P1)»: esperado False");
//             Console.WriteLine($"{titulo}: {bien} de {total} pruebas correctas");
//         }
//         catch (NotImplementedException) { Console.WriteLine($"{titulo}: (sin escribir todavía)"); }
//     }

//     // La CARGA para medir: n paquetes al azar (semilla fija) y una capacidad de un tercio del peso total.
//     public static int EjecutarCarga(IPlanificador plan, int n)
//     {
//         var azar = new Random(n);
//         var paquetes = Enumerable.Range(0, n).Select(i => new Paquete("C" + i, azar.Next(5, 60), azar.Next(10, 100))).ToArray();
//         int capacidad = paquetes.Sum(p => p.Kg) / 3;
//         var (ganancia, _) = plan.Elegir(paquetes, capacidad);
//         return ganancia;
//     }

//     public static void Main()
//     {
//         Probar("PlanificadorIngenuo ", () => new PlanificadorIngenuo());
//         Probar("PlanificadorMejorado", () => new PlanificadorMejorado());

//         // Una medición de ejemplo, SIN protocolo (el protocolo lo agregas tú en tu medición):
//         var reloj = Stopwatch.StartNew();
//         EjecutarCarga(new PlanificadorIngenuo(), 18);
//         Console.WriteLine($"Ingenuo con 18 paquetes: {reloj.ElapsedMilliseconds} ms (una sola medición: no es evidencia)");
//     }
// }
