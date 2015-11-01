using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PICSolver.Abstract;

namespace PICSolver.Mesh
{
    public class Mesh2d : IMesh
    {
        private double[] _Ex;
        private double[] _Ey;
        private double[] _rho;
        private int _cells;

        public double[] Ex { get { return _Ex; } set { _Ex = value; } }

        public double[] Ey { get { return _Ey; } set { _Ey = value; } }

        public double[] Density { get { return _rho; } }

        public double[] Potential { get; set; }

        public void InitializeMesh(int cells)
        {
            _cells = cells;
            _rho = new double[_cells];
            _Ex = new double[_cells];
            _Ey = new double[_cells];
        }

        public double GetEx(int cell)
        {
            return _Ex[cell];
        }

        public double GetEy(int cell)
        {
            return _Ey[cell];
        }

        public void AddDensity(int cell, double density)
        {
            _rho[cell] += density;
        }
        public double GetDensity(int cell)
        {
            return _rho[cell];
        }
        public void ResetDensity()
        {
            for (int i = 0; i < _cells; i++)
            {
                _rho[i] = 0;
            }
        }
    }
}
