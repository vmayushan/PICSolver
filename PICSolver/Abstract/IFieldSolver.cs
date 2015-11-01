using MathNet.Numerics.LinearAlgebra;

namespace PICSolver.Abstract
{
    public interface IFieldSolver
    {
        Matrix<double> Matrix { get; set; }
        Matrix<double> BuildMatrix();
        Vector<double> BuildVector(IMesh grid);
        double[] Solve(Matrix<double> matrix, Vector<double> right);
    }
}
