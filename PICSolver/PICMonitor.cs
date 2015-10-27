using PICSolver.Abstract;
using PICSolver.Derivative;
using PICSolver.Domain;
using PICSolver.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICSolver
{
    public class PICMonitor
    {
        private IRectangleGrid _grid;
        private IParticleStorage<Particle> _particles;

        public double[,] Rho { get { return MatrixExtensions.RectangleArray(_grid.Rho, _grid.N, _grid.M); } }
        public double[,] Ex { get { return MatrixExtensions.RectangleArray(_grid.Ex, _grid.N, _grid.M); } }
        public double[,] Ey { get { return MatrixExtensions.RectangleArray(_grid.Ey, _grid.N, _grid.M); } }
        public double[,] Potential { get { return MatrixExtensions.RectangleArray(_grid.Potential, _grid.N, _grid.M);  } }
        public int ParticlesCount { get { return _particles.Count; } }

        public PICMonitor(IRectangleGrid grid, IParticleStorage<Particle> particles)
        {
            this._grid = grid;
            this._particles = particles;
        }

        internal void Test()
        {
        }

        //private void SaveStateToCsv(double[] stateVector, string filename)
        //{
        //    Matrix<double> result = Matrix<double>.Build.Dense(_n, _m);
        //    for (int i = 0; i < _n; i++)
        //    {
        //        for (int j = 0; j < _m; j++)
        //        {
        //            result[i, j] = stateVector[_m * i + j];
        //        }
        //    }
        //    DelimitedWriter.Write(filename, result, ";");
        //}
        //public void SaveDebugInfoToCSV()
        //{
        //    SaveStateToCsv(_rho, "rho.csv");
        //    SaveStateToCsv(_Ex, "ex.csv");
        //    SaveStateToCsv(_Ey, "ey.csv");
        //}
    }
}
