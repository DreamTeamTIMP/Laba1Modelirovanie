using Laba1.Histogram;

namespace Appr
{
    static class NormalPDF
    {

    }

    static class Appr
    {
        public static void CreateAppr(int left, int right)
        {
            if (left > right) (right, left) = (left, right);
            int intervals = right * 2;
            double step = (right - left) / intervals;
            Random rand = new();
            rand.NextDouble();
        }
    }

    static class Func
    {
        public static double f(double x)
        {
            return -0.5 * x + 1;
        }

        public static double F(double x)
        {
            return -0.25 * x * x + x;
        }

        public static double F1(double x)
        {
            if (1 - x < 0) throw new ArgumentException("Argument can't be negative!");
            return 2 - 2 * Math.Sqrt(1 - x);
        }
    }

    class Intervals
    {
        public readonly int Count;
        
        private readonly List<double> data;

        public IReadOnlyList<double> Data => data;

        public Intervals(int count)
        {
            Count = count;
            data = [];
            Generate();
        }

        private void Generate()
        {
            double NMinus1Value = 0;
            data.Add(NMinus1Value);

            Console.WriteLine("Intervals:");
            Console.WriteLine($"0: {NMinus1Value}");
            for (int i = 1; i < Count; i++)
            {
                NMinus1Value = Func.F1(1.0 / Count + Func.F(NMinus1Value));
                data.Add(NMinus1Value);
                Console.WriteLine($"{i}: {NMinus1Value}");
            }
            

            // Последнее значение из-за погрешности иногда выходит за пределы поэтому его считаем отдельно
            NMinus1Value = Func.F1(1);
            data.Add(NMinus1Value);
            Console.WriteLine($"{Count}: {NMinus1Value}");
        }
    }

    class MyRandom
    {
        private Random rand = new();
        
        public double From0To1() => rand.NextDouble();

        public double GetNum(Intervals intervals) 
        {
            // Случайное число [0, 1) для выбора интервала
            double Xi = From0To1();

            // Номер левой границы интервала
            int k = (int) (intervals.Count * Xi);

            // Случайное число [0, 1)
            double Xi1 = From0To1();

            // Нормализация случайного числа к интервалу
            double randNumInInterval = (intervals.Data[k + 1] - intervals.Data[k]) * Xi1;

            // Финальное случайное число
            // Левая граница + случайное число на интервале
            double finalRandNum = intervals.Data[k] + randNumInInterval;
            
            return finalRandNum;  
        }
        

        // Немного более быстрый метод для получения сразу массива случайных чисел
        public double[] GetNNums(Intervals intervals, int N)
        {
            if (N < 1) throw new ArgumentException("Can genarated only positive amount of numbers");
            
            double[] data = new double[N];

            double Xi = From0To1();
            for (int i = 0; i < N; i++)
            {
                int k = (int) ((intervals.Count) * Xi);
                
                Xi = From0To1();

                double randNumInInterval = (intervals.Data[k + 1] - intervals.Data[k]) * Xi;
                double finalRandNum = intervals.Data[k] + randNumInInterval;

                data[i] = finalRandNum;
            }

            return data;
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            // Создаём и запускаем окно гистограммы
            using var histogram = new HistogramWindow(1280, 720, "Гистограмма");
            histogram.Start();
            
            Intervals intervals = new(20);
            
            MyRandom rand = new();
            double[] data = rand.GetNNums(intervals, 100000);

            Console.WriteLine();
            double max = -1;
            double min = 12312123;
            double sum = 0;
            double mean = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] > max) max = data[i];
                if (data[i] < min) min = data[i];
                sum += data[i];
                //Console.WriteLine($"{i}: {data[i]}");
            }
            mean = sum / data.Length;
            Console.WriteLine($"Max: {max}");
            Console.WriteLine($"Min: {min}");
            Console.WriteLine($"Sum: {sum}");
            Console.WriteLine($"Mean: {mean}");
            
            Console.ReadLine();
            histogram.UpdateData(data, 50);
            Console.ReadLine();
            histogram.UpdateData(data, 51);
            Console.ReadLine();
            histogram.UpdateData(data, 100);
            Console.ReadLine();
            histogram.UpdateData(data, 200);
            Console.ReadLine();
            histogram.UpdateData(data, 500);
            Console.ReadLine();
            histogram.UpdateData(data, 1000);
            Console.ReadLine();

            // Даём время потоку окна завершиться
            histogram.Close();
            await Task.Delay(500);
            Console.WriteLine("Программа завершена.");
        }
    }
}