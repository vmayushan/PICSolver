using MathNet.Numerics.LinearAlgebra;
using System;

namespace PICSolver.Abstract
{
    public interface IFieldSolver
    {
        Matrix<double> BuildMatrix();
        Vector<double> BuildVector(Func<double, double, double> f);
        Vector<double> BuildVector(IRectangleGrid grid);
        Matrix<double> Solve(Matrix<double> A, Vector<double> B);
        void Solve(Matrix<double> A, Vector<double> B, Matrix<double> result);
    }
}
