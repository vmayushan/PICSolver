using MathNet.Numerics.LinearAlgebra;
using System;

namespace PICSolver.Abstract
{
    public interface IFieldSolver
    {
        Matrix<double> BuildMatrix();
        Vector<double> BuildVector(Func<double, double, double> f);
        Vector<double> BuildVector(IMesh grid);
        Matrix<double> Solve(Matrix<double> A, Vector<double> B);
        void Solve(Matrix<double> A, Vector<double> B, Matrix<double> result);
        double[] SolveFlatten(Matrix<double> A, Vector<double> B);
    }
}
