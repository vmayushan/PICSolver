namespace PICSolver.Extensions
{
    public static class MatrixExtensions
    {
        public static double[,] RectangleArray(double[] source, int n, int m)
        {
            var result = new double[n, m];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    result[i, j] = source[n * j + i];
                }
            }
            return result;
        }

        public static double[] FlattenArray(double[,] source, int n, int m)
        {
            var result = new double[n * m];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    result[m * i + j] = source[i, j];
                }
            }
            return result;
        }

        public static double[] GetLineArrayX(double[] source, int n, int m, int line)
        {
            var result = new double[m];

            for (int j = 0; j < m; j++)
            {
                result[j] = source[n * j + line];
            }
            return result;
        }
        public static double[] GetLineArrayY(double[] source, int n, int m, int line)
        {
            var result = new double[n];

            for (int i = 0; i < n; i++)
            {
                result[i] = source[n * line + i];
            }
            return result;
        }
    }
}
