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
    class Program
    {
        static void Main(string[] args)
        {
        }
    }
}