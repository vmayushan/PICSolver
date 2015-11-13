namespace PICSolver.Abstract
{
    public interface IMesh
    {
        void InitializeMesh(int cells);
        double[] Density { get; set; }
        double[] Potential { get; set; }
        double[] Ex { get; set; }
        double[] Ey { get; set; }
        double GetEx(int cellId);
        double GetEy(int cellId);
        void ResetDensity();
        void AddDensity(int cell, double density);
        double GetDensity(int cellId);
    }
}
