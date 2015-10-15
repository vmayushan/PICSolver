using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICSolver.Abstract
{
    public interface IRectangleGrid
    {
        double CellSquare { get; }
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
    }
}