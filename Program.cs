using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Laba1.Histogram;

namespace Appr
{
    static class Func
    {
        public static double f(double x) => 1 - 0.5 * x;
        public static double F(double x) => x - 0.25 * x * x; // теоретическая функция распределения на [0,2]
        public static double F1(double x) => 2 - 2 * Math.Sqrt(1 - x); // обратная функция
    }

    class Intervals
    {
        public readonly int Count;
        private readonly List<double> data;

        public IReadOnlyList<double> Data => data;

        public Intervals(int count)
        {
            Count = count;
            data = new List<double>(count + 1);
            Generate();
        }

        private void Generate()
        {
            // Прямое вычисление границ по формуле C_k = 2 - 2*sqrt(1 - k/Count)
            for (int k = 0; k <= Count; k++)
            {
                double t = (double)k / Count;
                double ck = 2 - 2 * Math.Sqrt(1 - t);
                data.Add(ck);
            }

            Console.WriteLine("Границы интервалов (C_k):");
            for (int i = 0; i < data.Count; i++)
                Console.WriteLine($"C[{i}] = {data[i]:F6}");
        }
    }

    class MyRandom
    {
        private Random rand = new Random();

        public double From0To1() => rand.NextDouble();

        public double GetNum(Intervals intervals)
        {
            double Xi = From0To1();
            int k = (int)(intervals.Count * Xi);
            double Xi1 = From0To1();
            double randNumInInterval = (intervals.Data[k + 1] - intervals.Data[k]) * Xi1;
            return intervals.Data[k] + randNumInInterval;
        }

        public double[] GetNNums(Intervals intervals, int N)
        {
            if (N < 1) throw new ArgumentException("N должно быть положительным");
            double[] data = new double[N];
            for (int i = 0; i < N; i++)
            {
                double Xi = From0To1();
                int k = (int)(intervals.Count * Xi);
                double Xi1 = From0To1();
                double randNumInInterval = (intervals.Data[k + 1] - intervals.Data[k]) * Xi1;
                data[i] = intervals.Data[k] + randNumInInterval;
            }
            return data;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Ввод параметров
            Console.Write("Введите число интервалов λ (по умолчанию 20): ");
            string lambdaInput = Console.ReadLine();
            int lambda = string.IsNullOrEmpty(lambdaInput) ? 20 : int.Parse(lambdaInput);

            Console.Write("Введите объём выборки N (по умолчанию 100000): ");
            string nInput = Console.ReadLine();
            int N = string.IsNullOrEmpty(nInput) ? 100000 : int.Parse(nInput);

            // Создаём и запускаем окно гистограммы
            using var histogram = new HistogramWindow(1280, 720, "Гистограмма");
            histogram.Start();

            // Генерация основной выборки
            Intervals intervals = new Intervals(lambda);
            MyRandom rnd = new MyRandom();
            double[] data = rnd.GetNNums(intervals, N);

            // Статистика по выборке
            double mean = data.Average();
            double variance = data.Select(x => Math.Pow(x - mean, 2)).Sum() / (data.Length - 1);
            double stdDev = Math.Sqrt(variance);
            double min = data.Min();
            double max = data.Max();

            Console.WriteLine($"\nСтатистика по выборке (N = {N}):");
            Console.WriteLine($"Минимум: {min:F6}");
            Console.WriteLine($"Максимум: {max:F6}");
            Console.WriteLine($"Среднее: {mean:F6}");
            Console.WriteLine($"СКО: {stdDev:F6}");

            // Критерий Колмогорова
            Console.WriteLine("\n--- Критерий согласия Колмогорова ---");
            KolmogorovTest(data, Func.F);

            // Исследование влияния объёма выборки
            Console.WriteLine("\n--- Исследование влияния объёма выборки ---");
            int[] testVolumes = { 100, 1000, 10000, 100000 };
            foreach (int vol in testVolumes)
            {
                double[] testSample = rnd.GetNNums(intervals, vol);
                Console.Write($"Объём {vol,6}: ");
                KolmogorovTest(testSample, Func.F, silent: true);
            }

            // Демонстрация гистограммы с разным числом карманов
            Console.WriteLine("\n Гистограмма с 5 карманами...");
            histogram.UpdateData(data, 5);

            // Демонстрация гистограммы с разным числом карманов
            Console.WriteLine("\nНажмите Enter для показа гистограммы с 10 карманами...");
            Console.ReadLine();
            histogram.UpdateData(data, 10);

            // Демонстрация гистограммы с разным числом карманов
            Console.WriteLine("\nНажмите Enter для показа гистограммы с 20 карманами...");
            Console.ReadLine();
            histogram.UpdateData(data, 20);

            // Демонстрация гистограммы с разным числом карманов
            Console.WriteLine("\nНажмите Enter для показа гистограммы с 30 карманами...");
            Console.ReadLine();
            histogram.UpdateData(data, 30);

            // Демонстрация гистограммы с разным числом карманов
            Console.WriteLine("\nНажмите Enter для показа гистограммы с 50 карманами...");
            Console.ReadLine();
            histogram.UpdateData(data, 50);

            Console.WriteLine("Нажмите Enter для показа гистограммы со 100 карманами...");
            Console.ReadLine();
            histogram.UpdateData(data, 100);

            Console.WriteLine("Нажмите Enter для показа гистограммы с 200 карманами...");
            Console.ReadLine();
            histogram.UpdateData(data, 200);

            
            Console.WriteLine("Программа завершена. Нажмите Enter для выхода.");
            Console.ReadLine();
            histogram.Close();
        }

        /// <summary>
        /// Выполняет критерий согласия Колмогорова
        /// </summary>
        /// <param name="data">Выборка значений</param>
        /// <param name="F">Теоретическая функция распределения</param>
        /// <param name="silent">Если true, выводится только статистика; иначе подробный вывод</param>
        static void KolmogorovTest(double[] data, Func<double, double> F, bool silent = false)
        {
            Array.Sort(data);
            int n = data.Length;
            double maxDiff = 0.0;

            for (int i = 0; i < n; i++)
            {
                double x = data[i];
                double theor = F(x);
                double empAfter = (i + 1) / (double)n; // F_эмп после скачка
                double empBefore = i / (double)n;      // F_эмп до скачка

                double diffAfter = Math.Abs(empAfter - theor);
                double diffBefore = Math.Abs(empBefore - theor);

                if (diffAfter > maxDiff) maxDiff = diffAfter;
                if (diffBefore > maxDiff) maxDiff = diffBefore;
            }

            double lambda = maxDiff * Math.Sqrt(n);
            // Приближённое p-value (для больших n)
            double p = 2 * Math.Exp(-2 * lambda * lambda);

            if (!silent)
            {
                Console.WriteLine($"Статистика D = {maxDiff:F6}");
                Console.WriteLine($"lambda = D·√N = {lambda:F6}");
                Console.WriteLine($"p-value = {p:F6}");
                // Сравнение с критическим значением для уровня 0.05 (λ_крит ≈ 1.36)
                double crit = 1.36;
                if (lambda < crit)
                    Console.WriteLine("lambda < 1.36 → гипотеза о согласии НЕ отвергается (уровень 0.05)");
                else
                    Console.WriteLine("lambda ≥ 1.36 → гипотеза о согласии отвергается (уровень 0.05)");
            }
            else
            {
                Console.WriteLine($"D = {maxDiff:F6}, λ = {lambda:F6} (p≈{p:F4})");
            }
        }
    }
}