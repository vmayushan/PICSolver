using MathNet.Numerics.LinearAlgebra;
using PICSolver.Derivative;

namespace PICSolver.Extensions
{
    public static class MatrixExtensions
    {
        public static MatrixGradientResult Gradient(this Matrix<double> matrix, double hx = 1.0, double hy = 1.0)
        {
            return MatrixGradient.Instance.Evaluate(matrix, hx, hy);
        }
    }
}
