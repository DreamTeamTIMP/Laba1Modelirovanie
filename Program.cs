namespace Appr
{


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
        // f(n) = 1 - 0.5 * n
        /*
         * Формула для интервалов:
         * F(n) = интеграл(0->n) (1 - 0.5 t) * dt = n - (n^2 / 4)
         * F(Ck) = k/l =>
         * => Ck - (Ck^2 / 4) = (k / l)     =>   Ck^2 - (4 * Ck) + ((4 * k) / l) = 0
         * Ck : [0,2]
         * Ck = 2 - 2 * (1 - k/l)^(1/2)
         */
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Генерация случайных чисел с плотностью f(eta) = 1 - 0.5*eta на [0,2]");
            Console.Write("Введите число интервалов lambda: ");
            int lambda = int.Parse(Console.ReadLine());
            Console.Write("Введите количество генерируемых чисел N: ");
            int N = int.Parse(Console.ReadLine());
            double[] boundaries = new double[lambda + 1];
            for (int k = 0; k <= lambda; k++)
            {
                double t = (double)k / lambda;
                boundaries[k] = 2 - 2 * Math.Sqrt(1 - t);
            }

            Random rand = new();
            List<double> results = [];

            for (int i = 0; i < N; i++)
            {
                double Xi = rand.NextDouble();
                int index = (int)(lambda * Xi);
                if (index == lambda) index = lambda - 1;

                double Xip1 = rand.NextDouble();
                double y = boundaries[index] + (boundaries[index + 1] - boundaries[index]) * Xip1;
                results.Add(y);
            }
            Console.WriteLine("\nСгенерированные числа:");
            foreach (var val in results)
            {
                Console.WriteLine($"{val:F6}");
            }
        }
    }
} 
//class Program
//{
//    string path = "test.txt";
//    static Random rnd = new Random();
//    static void Main()
//    {
//        int l = 10;
//        int N = 1000;
//        double[] C = new double[l + 1];
//        C[0] = 0.0;
//        C[l] = 2.0;
//        for (int k = 1; k < l; k++)
//        {
//            double p = (double)k / l;
//            C[k] = 2.0 - 2.0 * Math.Sqrt(1.0 - p);
//        }
//        double[] sample = new double[N];
//        for (int i = 0; i < N; i++)
//        {
//            double U1 = rnd.NextDouble();
//            double U2 = rnd.NextDouble();
//            int k = (int)(U1 * l);
//            double y = C[k] + (C[k + 1] - C[k]) * U2;
//            sample[i] = y;
//        }
//        Array.Sort(sample);
//        Func<double, double> F = x =>
//        {
//            if (x <= 0) return 0.0;
//            if (x >= 2) return 1.0;
//            return x - 0.25 * x * x;
//        };
//        double D = 0.0;
//        for (int i = 0; i < N; i++)
//        {
//            double x = sample[i];
//            double fn = (i + 1.0) / N;
//            double fnPrev = (double)i / N;
//            double diff1 = Math.Abs(fn - F(x));
//            double diff2 = Math.Abs(fnPrev - F(x));
//            D = Math.Max(D, Math.Max(diff1, diff2));
//        }
//        double lambda = D * Math.Sqrt(N);
//        double criticallambda = 1.36;
//        int Stur = (int)(1 + 3.322 * Math.Log10(N));
//        double[] histT = new double[Stur];
//        double[] histE = new double[Stur];
//        Console.WriteLine($"Объём выборки N = {N}");
//        Console.WriteLine($"Статистика Колмогорова D = {D:F6}");
//        Console.WriteLine($"λ = DsqrtN = {lambda:F6}");
//        Console.WriteLine($"Критическое значение λ_0.05 = {criticallambda}");
//        Console.WriteLine($"По формуле Стерджеса подинтервалы={Stur}");
//        Console.WriteLine(lambda <= criticallambda
//            ? "Гипотеза о согласии принимается"
//            : "Гипотеза отвергается");
//        Console.WriteLine($"\nДанные для графиков");
//        double max_teor = double.NegativeInfinity;
//        double min_teor = double.PositiveInfinity;
//        double max_emp = double.NegativeInfinity;
//        double min_emp = double.PositiveInfinity;
//        double[] Teor = new double[N];
//        double[] Emp = new double[N];
//        for (int i = 0; i < N; i++)
//        {
//            double x = sample[i];
//            double fTeor = F(x);
//            double fEmp = (i + 1.0) / N;
//            Teor[i] = fTeor;
//            Emp[i] = fEmp;
//            if (fTeor > max_teor) max_teor = fTeor;
//            if (fTeor < min_teor) min_teor = fTeor;
//            if (fEmp > max_emp) max_emp = fEmp;
//            if (fEmp < min_emp) min_emp = fEmp;
//        }
//        double stepT = (max_teor - min_teor) / Stur;
//        double stepEmp = (max_emp - min_emp) / Stur;
//        Console.WriteLine($"N№;x;F_teor;F_emp");
//        string filename = "data_for_excel.csv";
//        Console.WriteLine("\nДанные для графиков");
//        for (int i = 0; i < N; i++)
//        {
//            int indexT = (int)((Teor[i] - min_teor) / stepT);
//            int indexE = (int)((Emp[i] - min_emp) / stepEmp);
//            if (indexT == Stur) indexT--;
//            if (indexE == Stur) indexE--;
//            histT[indexT]++;
//            histE[indexE]++;
//        }
//        File.WriteAllLines("teor.csv", histT.Select(n => n.ToString()));

//        File.WriteAllLines("emp.csv", histE.Select(n => n.ToString()));

//    }
//}