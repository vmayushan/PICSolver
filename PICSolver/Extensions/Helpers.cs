using System;

namespace PICSolver.Extensions
{
    public static class Helpers
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

        public static double[] GetLineX(double[,] source, int n, int m, int line)
        {
            var result = new double[m];

            for (int j = 0; j < m; j++)
            {
                result[j] = source[line, j];
            }
            return result;
        }

        public static double[] GetLineY(double[,] source, int n, int m, int line)
        {
            var result = new double[n];

            for (int i = 0; i < n; i++)
            {
                result[i] = source[i, line];
            }
            return result;
        }

        public static double[] Multiply(double[] source, double scalar)
        {
            for (var i = 0; i < source.Length; i++) source[i] *= scalar;
            return source;
        }

        public static double[] Sum(double[] lhs, double[] rhs)
        {
            var result = new double[lhs.Length];
            for (var i = 0; i < lhs.Length; i++) result[i] = lhs[i] + rhs[i];
            return result;
        }

        public static double Epsilon(double[] lhs, double[] rhs)
        {
            var max = 0.0;
            for (var i = 0; i < lhs.Length; i++)
            {
                if (Math.Abs(lhs[i]) < 1E-20) continue;
                max = Math.Max(max, Math.Abs((lhs[i] - rhs[i]) / lhs[i]));
            }
            return max;
        }

        public static double Length(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }

        public static double LineY(double x1, double x2, double y1, double y2, double x)
        {
            return ((x - x2) * y1 + (x1 - x) * y2) / (x1 - x2);
        }

        public static double LineX(double x1, double x2, double y1, double y2, double y)
        {
            return ((y1 - y) * x2 + (y - y2) * x1) / (y1 - y2);
        }

        public static double[] VectorDerivative(double[] vector, double hx)
        {
            var n = vector.Length;
            var derivative = new double[n];
            for (var i = 0; i < n; i++)
            {
                if (i == 0)
                    derivative[i] = -(-3 * vector[i] + 4 * vector[i + 1] - vector[i + 2]) / 2 / hx;

                else if (i == n - 1)
                    derivative[i] = -(3 * vector[i] - 4 * vector[i - 1] + vector[i - 2]) / 2 / hx;
                else
                    derivative[i] = -(vector[i + 1] - vector[i - 1]) / 2 / hx;
            }
            return derivative;
        }
    }
}