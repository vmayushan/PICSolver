using MathNet.Numerics.LinearAlgebra;
using System;

namespace PICSolver.Abstract
{
    public interface IFieldSolver
    {
        Matrix<double> BuildMatrix();
        Vector<double> BuildVector(IMesh grid);
        double[] Solve(Matrix<double> matrix, Vector<double> right);
    }
}
