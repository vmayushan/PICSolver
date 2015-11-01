using PICSolver.Abstract;
using PICSolver.Derivative;
using PICSolver.Domain;
using PICSolver.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICSolver
{
    public class PICMonitor
    {
        private IRectangleGrid _grid;
        private IMesh _mesh;
        private IParticleStorage<Particle> _particles;
        private Stopwatch _watch = new Stopwatch();
        private Stopwatch _watchPoisson = new Stopwatch();

        public double[,] Rho { get; set; }
        public double[,] Ex { get { return MatrixExtensions.RectangleArray(_mesh.Ex, _grid.N, _grid.M); } }
        public double[,] Ey { get { return MatrixExtensions.RectangleArray(_mesh.Ey, _grid.N, _grid.M); } }
        public double[,] Potential { get { return MatrixExtensions.RectangleArray(_mesh.Potential, _grid.N, _grid.M);  } }
        public int ParticlesCount { get { return _particles.Count; } }
        public long Time { get; set; }
        public long TimePoisson { get; set; }

        public PICMonitor(IRectangleGrid grid, IMesh mesh, IParticleStorage<Particle> particles)
        {
            this._grid = grid;
            this._particles = particles;
            this._mesh = mesh;
        }

        internal void BeginIteration()
        {
            _watch.Restart();
        }

        internal void EndIteration()
        {
            _watch.Stop();
            Time = _watch.ElapsedMilliseconds;
            Rho = MatrixExtensions.RectangleArray(_mesh.Density, _grid.N, _grid.M);
        }

        internal void BeginPoissonSolve()
        {
            _watchPoisson.Restart();
        }

        internal void EndPoissonSolve()
        {
            _watchPoisson.Stop();
            TimePoisson = _watchPoisson.ElapsedMilliseconds;
        }
    }
}
