// Nombre completo: Sergio Alvarado
// Parcial 2 - Variante B: solución ingenua y mejorada.
// Objetivo: comprobar el guion funcional y comparar el rendimiento de ambas soluciones.

using System.Diagnostics;

public interface IReceptorPedidos
{
    void Registrar(string pedido, int plazoMinutos);
    string? PrepararSiguiente();
    bool EstaPendiente(string pedido);
    int Pendientes { get; }
}

public sealed record Pedido(string Nombre, int Plazo, int Llegada);

// Solución ingenua: almacena pedidos en una lista.
// Para preparar, recorre la lista buscando el plazo mínimo.
// Si hay empate, el pedido que llegó antes tiene preferencia.
public sealed class ReceptorIngenuo : IReceptorPedidos
{
    private readonly List<Pedido> _lista = new();
    private int _llegadas;

    public int Pendientes => _lista.Count;

    public void Registrar(string pedido, int plazoMinutos)
    {
        _lista.Add(new Pedido(pedido, plazoMinutos, _llegadas++));
    }

    public string? PrepararSiguiente()
    {
        if (_lista.Count == 0) return null;

        // Búsqueda lineal del menor plazo; desempate por orden de llegada.
        int mejor = 0;
        for (int i = 1; i < _lista.Count; i++)
        {
            Pedido actual = _lista[i];
            Pedido candidato = _lista[mejor];

            if (actual.Plazo < candidato.Plazo ||
                (actual.Plazo == candidato.Plazo && actual.Llegada < candidato.Llegada))
            {
                mejor = i;
            }
        }

        string nombre = _lista[mejor].Nombre;

        // Eliminar de la lista puede desplazar los elementos que están detrás.
        _lista.RemoveAt(mejor);
        return nombre;
    }

    public bool EstaPendiente(string pedido)
    {
        // Búsqueda lineal: se revisa hasta encontrarlo o llegar al final.
        foreach (Pedido p in _lista)
            if (p.Nombre == pedido) return true;

        return false;
    }
}

public sealed class ReceptorMejorado : IReceptorPedidos
{
    // Heap: prioriza el menor plazo y, en caso de empate,
    // el pedido que llegó primero (Plazo, Llegada).
    private readonly PriorityQueue<Pedido, (int Plazo, int Llegada)> _heap = new();

    // HashSet: permite consultar si un pedido sigue pendiente
    // sin recorrer todos los pedidos.
    private readonly HashSet<string> _pendientes = new();

    // Contador que asigna un orden de llegada a cada pedido.
    private int _llegadas;

    public void Registrar(string pedido, int plazoMinutos)
    {
        // Se crea el pedido asignándole su orden de llegada.
        var nuevo = new Pedido(pedido, plazoMinutos, _llegadas++);

        // Se inserta en el Heap usando plazo y llegada como prioridad.
        _heap.Enqueue(nuevo, (nuevo.Plazo, nuevo.Llegada));

        // Se registra también en el conjunto de pedidos pendientes.
        _pendientes.Add(pedido);
    }

    public string? PrepararSiguiente()
    {
        // Si no hay pedidos, no hay nada que preparar.
        if (_heap.Count == 0)
            return null;

        // Extrae el pedido con menor plazo y menor llegada en caso de empate.
        var pedido = _heap.Dequeue();

        // El pedido deja de estar pendiente.
        _pendientes.Remove(pedido.Nombre);

        return pedido.Nombre;
    }

    public bool EstaPendiente(string pedido)
    {
        // Consulta directa en el HashSet.
        return _pendientes.Contains(pedido);
    }

    // Devuelve directamente la cantidad de elementos del HashSet.
    public int Pendientes => _pendientes.Count;
}

public static class Program
{
    // Guion: (operación, argumento, plazo, resultado esperado).
    private static readonly (string op, string arg, int plazo, string esperado)[] Guion =
    {
        ("registrar", "Torta", 120, ""),
        ("registrar", "Galletas", 30, ""),
        ("registrar", "Pan", 120, ""),
        ("registrar", "Brazo", 60, ""),
        ("pendientes", "", 0, "4"),
        ("esta", "Galletas", 0, "True"),
        ("esta", "Flan", 0, "False"),
        ("preparar", "", 0, "Galletas"),
        ("esta", "Galletas", 0, "False"),
        ("preparar", "", 0, "Brazo"),
        ("registrar", "Alfajor", 30, ""),
        ("preparar", "", 0, "Alfajor"),
        ("preparar", "", 0, "Torta"),
        ("preparar", "", 0, "Pan"),
        ("preparar", "", 0, "(ninguno)"),
        ("pendientes", "", 0, "0"),
    };

    private static string Ejecutar(IReceptorPedidos g, string op, string arg, int plazo)
    {
        switch (op)
        {
            case "registrar": g.Registrar(arg, plazo); return "";
            case "preparar": return g.PrepararSiguiente() ?? "(ninguno)";
            case "esta": return g.EstaPendiente(arg).ToString();
            default: return g.Pendientes.ToString();
        }
    }

    private static void Probar(string titulo, IReceptorPedidos g)
    {
        int bien = 0, total = 0;
        try
        {
            foreach (var (op, arg, plazo, esperado) in Guion)
            {
                string obtenido = Ejecutar(g, op, arg, plazo);
                if (op == "registrar") continue;
                total++;
                if (obtenido == esperado) bien++;
                else Console.WriteLine($"   MAL en «{op} {arg}»: esperado {esperado}, obtuvo {obtenido}");
            }
            Console.WriteLine($"{titulo}: {bien} de {total} pasos correctos");
        }
        catch (NotImplementedException)
        {
            Console.WriteLine($"{titulo}: (sin escribir todavía)");
        }
    }

    // Carga de medición: primero registra n pedidos y después prepara los n.
    public static long EjecutarCarga(IReceptorPedidos g, int n)
    {
        for (int i = 0; i < n; i++)
            g.Registrar("Q" + i, ((int)((i * 6007L) % 6) + 1) * 15);

        long control = 0;
        for (int i = 0; i < n; i++)
            control += g.PrepararSiguiente()!.Length;

        return control;
    }
  
    // Ejecuta una carga completa y mide cuánto tarda.
    private static double MedirUnaCorrida(IReceptorPedidos receptor, int n)
    {
        var reloj = Stopwatch.StartNew();

        EjecutarCarga(receptor, n);

        reloj.Stop();
        return reloj.Elapsed.TotalMilliseconds;
    }

     // Aplica el protocolo: calentamiento + 3 corridas + promedio.
    private static double[] MedirPromedio(IReceptorPedidos receptor, int n)
    {
        // Corrida de calentamiento: no se incluye en los resultados.
        MedirUnaCorrida(receptor, n);

        double[] tiempos = new double[3];

        for (int i = 0; i < 3; i++)
        {
            tiempos[i] = MedirUnaCorrida(receptor, n);
        }

        return tiempos;
    }

    private static void MedirSoluciones()
    {
        int[] tamanos = { 5_000, 10_000, 20_000 };

        Console.WriteLine("\n=== P2.3: MEDICION DE AMBAS SOLUCIONES ===");

        foreach (var nombre in new[] { "Ingenua", "Mejorada" })
        {
            Console.WriteLine($"\n--- Solucion {nombre} ---");
            Console.WriteLine("n\tCorrida 1\tCorrida 2\tCorrida 3\tPromedio\tVeces");

            double promedioAnterior = 0;

            foreach (int n in tamanos)
            {
                IReceptorPedidos receptor = nombre == "Ingenua"
                    ? new ReceptorIngenuo()
                    : new ReceptorMejorado();

                double[] tiempos = MedirPromedio(receptor, n);

                double promedio = tiempos.Average();

                string veces = promedioAnterior == 0
                    ? "-"
                    : (promedio / promedioAnterior).ToString("F2") + "x";

                Console.WriteLine(
                    $"{n}\t" +
                    $"{tiempos[0]:F2}\t\t" +
                    $"{tiempos[1]:F2}\t\t" +
                    $"{tiempos[2]:F2}\t\t" +
                    $"{promedio:F2}\t\t" +
                    $"{veces}");

                promedioAnterior = promedio;
            }
        }
    }    

    public static void Main()
    {
        Console.WriteLine("=== GUION FUNCIONAL ===");
        Probar("ReceptorIngenuo ", new ReceptorIngenuo());
        Probar("ReceptorMejorado", new ReceptorMejorado());

        MedirSoluciones();
    }    
}