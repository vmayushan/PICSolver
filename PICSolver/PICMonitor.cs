using System.Diagnostics;
using PICSolver.Abstract;
using PICSolver.Domain;
using PICSolver.Extensions;

namespace PICSolver
{
    // ReSharper disable once InconsistentNaming
    public class PICMonitor
    {
        private readonly IParticleStorage<Particle> particles;
        private readonly Stopwatch watch = new Stopwatch();
        private readonly Stopwatch watchPoisson = new Stopwatch();
        private readonly IGrid2D grid;
        private readonly IMesh mesh;

        public PICMonitor(IGrid2D grid, IMesh mesh, IParticleStorage<Particle> particles)
        {
            this.grid = grid;
            this.particles = particles;
            this.mesh = mesh;
            GridX = grid.X;
            GridY = grid.Y;
        }

        public double[,] Rho { get; private set; }
        public double[,] Ex { get; private set; }
        public double[,] Ey { get; private set; }
        public double[] GridX { get; private set; }
        public double[] GridY { get; private set; }
        public double[,] Potential { get; private set; }
        public double[] LineGraph { get; set; }
        public int ParticlesCount => particles.Count;
        public long Time { get; set; }
        public long TimePoisson { get; set; }

        internal void BeginIteration()
        {
            watch.Restart();
        }

        internal void EndIteration()
        {
            watch.Stop();
            Time = watch.ElapsedMilliseconds;
            Rho = Helpers.RectangleArray(mesh.Density, grid.N, grid.M);
            Ex = Helpers.RectangleArray(mesh.Ex, grid.N, grid.M);
            Ey = Helpers.RectangleArray(mesh.Ey, grid.N, grid.M);
            Potential = Helpers.RectangleArray(mesh.Potential, grid.N, grid.M);
            LineGraph = Helpers.GetLineArrayX(mesh.Ex, grid.N, grid.M, 0);
        }

        internal void BeginPoissonSolve()
        {
            watchPoisson.Restart();
        }

        internal void EndPoissonSolve()
        {
            watchPoisson.Stop();
            TimePoisson = watchPoisson.ElapsedMilliseconds;
        }
    }
}