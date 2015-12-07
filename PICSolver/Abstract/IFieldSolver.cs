using Extreme.Mathematics;
using Extreme.Mathematics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra;

namespace PICSolver.Abstract
{
    public interface IFieldSolver
    {
        Matrix<double> FdmMatrix { get; set; }
        Matrix<double> BuildMatrix();
        Vector<double> BuildVector(IMesh grid);
        double[] Solve(Matrix<double> matrix, Vector<double> right);
    }
}