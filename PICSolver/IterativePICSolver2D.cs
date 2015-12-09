using System;
using System.Collections.Generic;
using System.Linq;
using PICSolver.Abstract;
using PICSolver.Domain;
using PICSolver.ElectricField;
using PICSolver.Emitter;
using PICSolver.Extensions;
using PICSolver.Grid;
using PICSolver.Interpolation;
using PICSolver.Mesh;
using PICSolver.Mover;
using PICSolver.Poisson;
using PICSolver.Storage;
using System.Threading.Tasks;
using PICSolver.Monitor;

namespace PICSolver
{
    // ReSharper disable once InconsistentNaming
    public class IterativePICSolver2D : IPICSolver
    {
        private BoundaryConditions boundaryConditions;
        private IEmitter emitter;
        private IGrid2D grid;
        private double h;
        private IInterpolationScheme interpolator;
        private IMesh mesh;
        private IMover mover;
        private IParticleStorage<Particle> particles;
        private IFieldSolver poissonSolver;
        private double step;
        private double u;
        private double[] density;
        private double w;
        private int trajectoryInterval;

        private bool backscattering;
        private double alfa;
        private double beta;

        public List<Tuple<int, double, double>> Trajectories { get; set; }
        public PICMonitor Monitor { get; set; }

        public void Prepare(PICProject project)
        {
            w = project.Relaxation;
            backscattering = project.Backscattering;
            alfa = project.BackscatteringAlfa;
            beta = project.BackscatteringBeta;
            step = project.Step;
            u = project.Voltage;

            particles = new ParticleArrayStorage<Particle>(project.ParticlesCount);
            boundaryConditions = new BoundaryConditions
            {
                Top = new BoundaryCondition { Value = x => 0, Type = BoundaryConditionType.Neumann },
                Bottom = new BoundaryCondition { Value = x => 0, Type = BoundaryConditionType.Neumann },
                Left = new BoundaryCondition { Value = x => 0, Type = BoundaryConditionType.Dirichlet },
                Right = new BoundaryCondition { Value = x => u, Type = BoundaryConditionType.Dirichlet }
            };

            emitter = new Emitter2D(0, project.EmitterBottom, 0, project.EmitterTop, project.ParticlesCount, 0, 0, -0.5 * Constants.ChildLangmuirCurrent(project.Length, u), step);
            mover = new Leapfrog();
            grid = new Grid2D();
            grid.InitializeGrid(project.GridN, project.GridM, 0, project.Length, 0, project.Height);
            mesh = new Mesh2D();
            mesh.InitializeMesh(grid.N * grid.M);
            interpolator = new CloudInCellCurrentLinkage(particles, grid, mesh, false);
            poissonSolver = new Poisson2DFdmSolver(grid, boundaryConditions);
            poissonSolver.FdmMatrix = poissonSolver.BuildMatrix();
            h = step * Constants.LightVelocity;
            Monitor = new PICMonitor(grid, mesh, particles, this);
            density = new double[grid.N * grid.M];
            Trajectories = new List<Tuple<int, double, double>>(particles.Count * 1000);
        }

        public double Step()
        {
            Monitor.BeginIteration();
            var injectedParticles = emitter.Inject();
            var injectedParticlesId = new int[emitter.ParticlesCount];
            for (var i = 0; i < emitter.ParticlesCount; i++)
            {
                var cell = grid.FindCell(injectedParticles[i].X, injectedParticles[i].Y);
                var id = particles.Add(injectedParticles[i]);
                particles.SetParticleCell(id, cell);
                injectedParticlesId[i] = id;
            }

            var vector = poissonSolver.BuildVector(mesh);
            Monitor.BeginPoissonSolve();
            mesh.Potential = poissonSolver.Solve(poissonSolver.FdmMatrix, vector);
            Monitor.EndPoissonSolve();
            Gradient.Calculate(mesh.Potential, mesh.Ex, mesh.Ey, grid.N, grid.M, grid.Hx, grid.Hy);
            mesh.ResetDensity();

            particles.ResetForces();
            interpolator.InterpolateForces();

            for (var i = 0; i < emitter.ParticlesCount; i++)
                mover.Prepare(particles, injectedParticlesId[i], h);

            Trajectories.Clear();
            while (particles.Count > 0) MoveParticles();

            var eps = Helpers.Epsilon(mesh.Density, density);
            mesh.Density = Helpers.Sum(Helpers.Multiply(density, 1.0 - w), Helpers.Multiply(mesh.Density, w));
            density = mesh.Density.Clone() as double[];

            Monitor.EndIteration();
            return eps;

        }

        private void MoveParticles()
        {
            particles.ResetForces();
            interpolator.InterpolateDensity();
            interpolator.InterpolateForces();

            foreach (var index in particles.EnumerateIndexes())
            {
                mover.Step(particles, index, h);
                if (trajectoryInterval == 0) Trajectories.Add(new Tuple<int, double, double>(index, particles.Get(Field.X, index), particles.Get(Field.Y, index)));


                if (!backscattering && grid.IsOutOfGrid(particles.Get(Field.X, index), particles.Get(Field.Y, index)))
                {
                    particles.RemoveAt(index);
                }
                else
                {
                    if (backscattering && grid.IsOutOfGrid(particles.Get(Field.X, index), particles.Get(Field.Y, index)))
                    {
                        particles.Set(Field.X, index, particles.Get(Field.PrevX, index));
                        particles.Set(Field.Y, index, particles.Get(Field.PrevY, index));
                        particles.Multiply(Field.Px, index, -beta);
                        particles.Multiply(Field.Py, index, beta);
                        particles.Multiply(Field.Q, index, alfa);
                        if (particles.Get(Field.Q, index) > 0.05 * emitter.ParticleCharge) particles.RemoveAt(index);
                    }

                    var cell = grid.FindCell(particles.Get(Field.X, index), particles.Get(Field.Y, index));
                    particles.SetParticleCell(index, cell);
                }

            }
            if (trajectoryInterval == 20) trajectoryInterval = 0; //todo вынести в проект
            else trajectoryInterval++;

        }
    }
}