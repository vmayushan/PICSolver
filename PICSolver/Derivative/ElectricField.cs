using MathNet.Numerics.LinearAlgebra;
using System;

namespace PICSolver.Derivative
{
    public class ElectricField
    {
        public static void EvaluateFlatten(double[] vector, double[] dx, double[] dy, int n, int m, double hx = 1.0, double hy = 1.0)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (i == 0)
                        //left
                        dx[j * n + i] = -(-3 * vector[j * n + i] + 4 * vector[j * n + i + 1] - vector[j * n + i + 2]) / 2 / hx;

                    else if (i == n - 1)
                        //right
                        dx[j * n + i] = -(3 * vector[j * n + i] - 4 * vector[j * n + i - 1] + vector[j * n + i - 2]) / 2 / hx;
                    else
                        //central
                        dx[j * n + i] = -(vector[j * n + i + 1] - vector[j * n + i - 1]) / 2 / hx;

                    if (j == 0)
                        //left
                        dy[j * n + i] = -(-3 * vector[j * n + i] + 4 * vector[(j + 1) * n + i] - vector[(j + 2) * n + i]) / 2 / hy;
                    else if (j == m - 1)
                        //right
                        dy[j * n + i] = -(3 * vector[j * n + i] - 4 * vector[(j - 1) * n + i] + vector[(j - 2) * n + i]) / 2 / hy;
                    else
                        //central
                        dy[j * n + i] = -(vector[(j + 1) * n + i] - vector[(j - 1) * n + i]) / 2 / hy;
                }
            }
        }
    }
}
