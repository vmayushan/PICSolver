using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICSolver.Abstract
{
    interface IRectangleGrid
    {
        double CellSquare { get; }
        double Hx { get; }
        double Hy { get; }
        double GetEx(int cell);
        double GetEy(int cell);
        void ResetDensity();
        void AddDensity(int cell, double density);
        int GetTopIndex(int cell);
        double InterpolateWeight(double x, double y, int cellId);
    }
}
