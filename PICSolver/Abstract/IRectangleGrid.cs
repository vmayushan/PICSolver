namespace PICSolver.Abstract
{
    public interface IRectangleGrid
    {
        double CellSquare { get; }
        double[] Ex { get; set; }
        double[] Ey { get; set; }
        double Hx { get; }
        double Hy { get; }
        double[] GridX { get; }
        double[] GridY { get; }
        int Nx { get; }
        int Ny { get; }
        double GetEx(int cell);
        double GetEy(int cell);
        void ResetDensity();
        void AddDensity(int cell, double density);
        double GetDensity(int cell);
        int GetTopIndex(int cell);
        double InterpolateWeight(double x, double y, int cellId);
        int FindCell(double x, double y);
        void InitializeGrid(int n, int m, double left, double right, double bottom, double top);
        bool OutOfGrid(double x, double y);
        void SaveDebugInfoToCSV();
        double[,] GetRho();
    }
}