using System;
using System.Diagnostics;

namespace C4.BusquedaLinealVsBinaria
{
    internal class Program
    {
        // ============================================================
        // C4 - BÚSQUEDA LINEAL VS. BÚSQUEDA BINARIA
        //
        // OBJETIVO:
        // Comparar experimentalmente la búsqueda lineal O(n) y la
        // búsqueda binaria O(log n) en dos dimensiones claramente
        // diferenciadas:
        //   1. Experimento A: Cantidad de pasos en el peor caso
        //      (arreglos ordenados de 1.000, 100.000 y 1.000.000 de
        //      valores pares consecutivos, buscando una clave inexistente).
        //   2. Experimento B: Tiempo de ejecución real en milisegundos
        //      (arreglo ordenado de 1.000.000 de enteros, buscando un
        //      lote de 1.000 claves existentes generadas con Random(21),
        //      con 1 corrida de calentamiento y 3 corridas medidas).
        // ============================================================

        private static readonly int[] TamanosExperimentoA =
        {
            1_000,
            100_000,
            1_000_000
        };

        private const int TamanoExperimentoB = 1_000_000;
        private const int CantidadClavesBuscadas = 1_000;
        private const int Semilla = 21;
        private const int NumeroCorridas = 3;

        static void Main(string[] args)
        {
            Console.WriteLine("======================================================================");
            Console.WriteLine("             C4 - BÚSQUEDA LINEAL VS. BÚSQUEDA BINARIA");
            Console.WriteLine("======================================================================");
            Console.WriteLine();

            EjecutarExperimentoA();

            Console.WriteLine();

            EjecutarExperimentoB();
        }

        // ============================================================
        // EXPERIMENTO A: COMPARACIÓN DE CANTIDAD DE PASOS (PEOR CASO)
        // ============================================================
        private static void EjecutarExperimentoA()
        {
            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine("EXPERIMENTO A: CANTIDAD DE PASOS EN EL PEOR CASO (CLAVE INEXISTENTE)");
            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine("- Datos: Valores pares consecutivos (0, 2, 4, ..., 2*(n-1)).");
            Console.WriteLine("- Clave buscada: 2*n + 1 (impar mayor al máximo, inexistente).");
            Console.WriteLine();
            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine($"{"n",12} | {"Pasos Lineal O(n)",20} | {"Pasos Binaria O(log2 n)",24} | {"Teórico Binaria",15}");
            Console.WriteLine("----------------------------------------------------------------------");

            foreach (int n in TamanosExperimentoA)
            {
                int[] datos = GenerarParesConsecutivos(n);

                // Clave inexistente que fuerza el peor caso en ambos algoritmos
                int claveInexistente = (n * 2) + 1;

                BuscarLinealContandoPasos(datos, claveInexistente, out int pasosLineal);
                BuscarBinariaContandoPasos(datos, claveInexistente, out int pasosBinaria);

                int teoricoBinaria = (int)Math.Floor(Math.Log2(n)) + 1;

                Console.WriteLine(
                    $"{n,12:N0} | {pasosLineal,20:N0} | {pasosBinaria,24:N0} | {teoricoBinaria,15:N0}");
            }

            Console.WriteLine("----------------------------------------------------------------------");
        }

        // ============================================================
        // EXPERIMENTO B: MEDICIÓN DE TIEMPOS (1.000 CLAVES EN n = 1.000.000)
        // ============================================================
        private static void EjecutarExperimentoB()
        {
            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine("EXPERIMENTO B: MEDICIÓN DE TIEMPOS DE EJECUCIÓN (1.000 BÚSQUEDAS)");
            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine($"- Tamaño del arreglo (n): {TamanoExperimentoB:N0} enteros ordenados (pares consecutivos).");
            Console.WriteLine($"- Claves a buscar: {CantidadClavesBuscadas:N0} claves existentes elegidas con Random({Semilla}).");
            Console.WriteLine($"- Protocolo: 1 corrida de calentamiento + {NumeroCorridas} corridas medidas.");
            Console.WriteLine();

            // 1. Preparación de datos fuera del cronómetro
            int[] datos = GenerarParesConsecutivos(TamanoExperimentoB);
            int[] claves = GenerarClavesExistentes(datos, CantidadClavesBuscadas, Semilla);

            // 2. Calentamiento (Warm-up) fuera de las corridas medidas
            // Ejecutamos varias pasadas para que el compilador JIT de .NET 8
            // promueva ambos métodos al nivel optimizado (Tier 1) antes de medir.
            int encontradosWarmupLineal = 0;
            int encontradosWarmupBinaria = 0;
            for (int w = 0; w < 5; w++)
            {
                encontradosWarmupLineal = EjecutarLoteLineal(datos, claves);
                encontradosWarmupBinaria = EjecutarLoteBinaria(datos, claves);
            }
            System.Threading.Thread.Sleep(200);

            if (encontradosWarmupLineal != CantidadClavesBuscadas ||
                encontradosWarmupBinaria != CantidadClavesBuscadas)
            {
                throw new InvalidOperationException("Error de validación: no se encontraron todas las claves existentes.");
            }

            // Limpieza de memoria previa a las corridas medidas
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // 3. Tres corridas medidas para Búsqueda Lineal
            double[] tiemposLineal = new double[NumeroCorridas];
            double sumaLineal = 0;

            for (int c = 0; c < NumeroCorridas; c++)
            {
                Stopwatch sw = Stopwatch.StartNew();
                int encontrados = EjecutarLoteLineal(datos, claves);
                sw.Stop();

                GC.KeepAlive(encontrados);
                tiemposLineal[c] = sw.Elapsed.TotalMilliseconds;
                sumaLineal += tiemposLineal[c];
            }

            double promedioLineal = sumaLineal / NumeroCorridas;

            // 4. Tres corridas medidas para Búsqueda Binaria
            double[] tiemposBinaria = new double[NumeroCorridas];
            double sumaBinaria = 0;

            for (int c = 0; c < NumeroCorridas; c++)
            {
                Stopwatch sw = Stopwatch.StartNew();
                int encontrados = EjecutarLoteBinaria(datos, claves);
                sw.Stop();

                GC.KeepAlive(encontrados);
                tiemposBinaria[c] = sw.Elapsed.TotalMilliseconds;
                sumaBinaria += tiemposBinaria[c];
            }

            double promedioBinaria = sumaBinaria / NumeroCorridas;
            double cociente = promedioLineal / promedioBinaria;

            // 5. Presentación de resultados
            Console.WriteLine("------------------------------------------------------------------------------------");
            Console.WriteLine($"{"Algoritmo",18} | {"Corrida 1",12} | {"Corrida 2",12} | {"Corrida 3",12} | {"Promedio",12}");
            Console.WriteLine("------------------------------------------------------------------------------------");
            Console.WriteLine(
                $"{"Búsqueda lineal",18} | {tiemposLineal[0],9:F4} ms | {tiemposLineal[1],9:F4} ms | {tiemposLineal[2],9:F4} ms | {promedioLineal,9:F4} ms");
            Console.WriteLine(
                $"{"Búsqueda binaria",18} | {tiemposBinaria[0],9:F4} ms | {tiemposBinaria[1],9:F4} ms | {tiemposBinaria[2],9:F4} ms | {promedioBinaria,9:F4} ms");
            Console.WriteLine("------------------------------------------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine($"Cociente (Tiempo Lineal / Tiempo Binaria): {cociente:F2} veces más rápida la búsqueda binaria.");
            Console.WriteLine("======================================================================");
        }

        // ============================================================
        // GENERACIÓN DE DATOS REPRODUCIBLES
        // ============================================================
        private static int[] GenerarParesConsecutivos(int cantidad)
        {
            int[] datos = new int[cantidad];
            for (int i = 0; i < cantidad; i++)
            {
                datos[i] = i * 2;
            }
            return datos;
        }

        private static int[] GenerarClavesExistentes(int[] datos, int cantidadClaves, int semilla)
        {
            Random random = new Random(semilla);
            int[] claves = new int[cantidadClaves];

            for (int i = 0; i < cantidadClaves; i++)
            {
                int indiceAleatorio = random.Next(0, datos.Length);
                claves[i] = datos[indiceAleatorio];
            }

            return claves;
        }

        // ============================================================
        // ALGORITMOS DE BÚSQUEDA (CON CONTEO DE PASOS PARA EXP. A)
        // ============================================================
        public static int BuscarLinealContandoPasos(int[] datos, int clave, out int pasos)
        {
            pasos = 0;
            for (int i = 0; i < datos.Length; i++)
            {
                pasos++;
                if (datos[i] == clave)
                {
                    return i;
                }
            }
            return -1;
        }

        public static int BuscarBinariaContandoPasos(int[] datos, int clave, out int pasos)
        {
            pasos = 0;
            int ini = 0;
            int fin = datos.Length - 1;

            while (ini <= fin)
            {
                pasos++;
                int medio = ini + (fin - ini) / 2;
                int valorMedio = datos[medio];

                if (valorMedio == clave)
                {
                    return medio;
                }

                if (valorMedio < clave)
                {
                    ini = medio + 1;
                }
                else
                {
                    fin = medio - 1;
                }
            }

            return -1;
        }

        // ============================================================
        // ALGORITMOS DE BÚSQUEDA (PARA MEDICIÓN DE TIEMPOS EN EXP. B)
        // ============================================================
        public static int BuscarLineal(int[] datos, int clave)
        {
            for (int i = 0; i < datos.Length; i++)
            {
                if (datos[i] == clave)
                {
                    return i;
                }
            }
            return -1;
        }

        public static int BuscarBinaria(int[] datos, int clave)
        {
            int ini = 0;
            int fin = datos.Length - 1;

            while (ini <= fin)
            {
                int medio = ini + (fin - ini) / 2;
                int valorMedio = datos[medio];

                if (valorMedio == clave)
                {
                    return medio;
                }

                if (valorMedio < clave)
                {
                    ini = medio + 1;
                }
                else
                {
                    fin = medio - 1;
                }
            }

            return -1;
        }

        private static int EjecutarLoteLineal(int[] datos, int[] claves)
        {
            int encontrados = 0;
            for (int i = 0; i < claves.Length; i++)
            {
                if (BuscarLineal(datos, claves[i]) >= 0)
                {
                    encontrados++;
                }
            }
            return encontrados;
        }

        private static int EjecutarLoteBinaria(int[] datos, int[] claves)
        {
            int encontrados = 0;
            for (int i = 0; i < claves.Length; i++)
            {
                if (BuscarBinaria(datos, claves[i]) >= 0)
                {
                    encontrados++;
                }
            }
            return encontrados;
        }
    }
}
