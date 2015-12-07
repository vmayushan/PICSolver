using System;
using PICSolver.Abstract;

namespace PICSolver.Mesh
{
    public class Mesh2D : IMesh
    {
        private int count;

        public double[] Ex { get; set; }
        public double[] Ey { get; set; }
        public double[] Density { get; set; }
        public double[] Potential { get; set; }

        private object[] DensityLock { get; set; }

        public void InitializeMesh(int cells)
        {
            count = cells;
            Density = new double[count];
            Ex = new double[count];
            Ey = new double[count];
            DensityLock = new object[count];
            for (var i = 0; i < count; i++)
                DensityLock[i] = new object();
        }

        public double GetEx(int cellId)
        {
            return Ex[cellId];
        }

        public double GetEy(int cellId)
        {
            return Ey[cellId];
        }

        public void AddDensity(int cell, double density, bool synchronized)
        {
            if (synchronized)
                lock (DensityLock[cell])
                    Density[cell] += density;
            else
                Density[cell] += density;
        }

        public double GetDensity(int cellId)
        {
            return Density[cellId];
        }

        public void ResetDensity()
        {
            Array.Clear(Density, 0, count);
        }
    }
}