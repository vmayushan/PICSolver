using System;
using MathNet.Numerics.LinearAlgebra;
using PICSolver.Derivative;

namespace PICSolver.Extensions
{
    public static class MatrixExtensions
    {
        public static double[,] RectangleArray(double[] source, int n, int m)
        {
            double[,] result = new double[n, m];
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
            double[] result = new double[n * m];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    result[m * i + j] = source[i, j];
                }
            }
            return result;
        }
    }
}
