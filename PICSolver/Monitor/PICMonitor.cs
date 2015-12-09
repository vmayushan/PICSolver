using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PICSolver.Abstract;
using PICSolver.Domain;
using PICSolver.Extensions;

namespace PICSolver.Monitor
{
    // ReSharper disable once InconsistentNaming
    public class PICMonitor
    {
        private readonly IParticleStorage<Particle> particles;
        private readonly Stopwatch watch = new Stopwatch();
        private readonly Stopwatch watchPoisson = new Stopwatch();
        private readonly IGrid2D grid;
        private readonly IMesh mesh;
        private readonly IPICSolver solver;

        public PICMonitor(IGrid2D grid, IMesh mesh, IParticleStorage<Particle> particles, IPICSolver solver)
        {
            this.grid = grid;
            this.particles = particles;
            this.mesh = mesh;
            this.solver = solver;
            GridX = grid.X;
            GridY = grid.Y;
            Status = PICStatus.Created;
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
        public PICStatus Status { get; set; }
        public ILookup<int,Tuple<int,double,double>> Trajectories { get; set; }



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
            //LineGraph = Helpers.VectorDerivative(Helpes.GetLineArrayY(mesh.Density, grid.N, grid.M, 51),grid.Hx); //Helpers.VectorDerivative(, grid.Hx);
            //LineGraph = Helpers.GetLineArrayX(mesh.Density, grid.N, grid.M, 0);
            Trajectories = solver.Trajectories.ToLookup(x => x.Item1);

        }

        public double[] GetLine(PlotSource source, LinePlotAlignment alignment, int line)
        {
            switch (source)
            {
                case PlotSource.Density:
                    return (alignment == LinePlotAlignment.Horizontal) ? Helpers.GetLineY(Rho, grid.N, grid.M, line) : Helpers.GetLineX(Rho, grid.N, grid.M, line);
                case PlotSource.Potential:
                    return (alignment == LinePlotAlignment.Horizontal) ? Helpers.GetLineY(Potential, grid.N, grid.M, line) : Helpers.GetLineX(Potential, grid.N, grid.M, line);
                case PlotSource.ElectricFieldX:
                    return (alignment == LinePlotAlignment.Horizontal) ? Helpers.GetLineY(Ex, grid.N, grid.M, line) : Helpers.GetLineX(Ex, grid.N, grid.M, line);
                case PlotSource.ElectricFieldY:
                    return (alignment == LinePlotAlignment.Horizontal) ? Helpers.GetLineY(Ey, grid.N, grid.M, line) : Helpers.GetLineX(Ey, grid.N, grid.M, line);
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, null);
            }
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