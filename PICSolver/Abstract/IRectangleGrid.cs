using MathNet.Numerics.LinearAlgebra;

namespace PICSolver.Abstract
{
    public interface IRectangleGrid
    {
        void InitializeGrid(int n, int m, double left, double right, double bottom, double top);
        double CellSquare { get; }
        double Hx { get; }
        double Hy { get; }
        double[] Ex { get; set; }
        double[] Ey { get; set; }
        double[] Rho { get; }
        double[] Potential { get; set; }
        double[] X { get; }
        double[] Y { get; }
        int N { get; }
        int M { get; }
        double GetEx(int cell);
        double GetEy(int cell);
        void ResetDensity();
        void AddDensity(int cell, double density);
        double GetDensity(int cell);
        double InterpolateWeight(double x, double y, int cellId);
        int GetTopIndex(int cell);
        int FindCell(double x, double y);
        bool IsOutOfGrid(double x, double y);
    }
}