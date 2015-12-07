using System;
using System.Collections.Generic;
using PICSolver.Abstract;
using PICSolver.Domain;
using PICSolver.Emitter;
using PICSolver.Grid;
using PICSolver.Interpolation;
using PICSolver.ElectricField;
using PICSolver.Mesh;
using PICSolver.Monitor;
using PICSolver.Mover;
using PICSolver.Poisson;
using PICSolver.Storage;

namespace PICSolver
{
    // ReSharper disable once InconsistentNaming
    public class PICSolver2D : IPICSolver
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
        private bool backscattering;
        private double alfa;
        private double beta;

        public PICMonitor Monitor { get; set; }
        public List<Tuple<int, double, double>> Trajectories { get; set; }
        public void Prepare(PICProject project)
        {
            backscattering = project.Backscattering;
            alfa = project.BackscatteringAlfa;
            beta = project.BackscatteringBeta;
            step = project.Step;
            u = project.Voltage;
            particles = new ParticleArrayStorage<Particle>(100000);
            boundaryConditions = new BoundaryConditions
            {
                Top = new BoundaryCondition { Value = x => 0, Type = BoundaryConditionType.Neumann },
                Bottom = new BoundaryCondition { Value = x => 0, Type = BoundaryConditionType.Neumann },
                Left = new BoundaryCondition { Value = x => 0, Type = BoundaryConditionType.Dirichlet },
                Right = new BoundaryCondition { Value = x => u, Type = BoundaryConditionType.Dirichlet }
            };

            emitter = new Emitter2D(0, project.EmitterBottom, 0, project.EmitterTop, project.ParticlesCount, 0, 0, -Constants.ChildLangmuirCurrent(project.Length, u), step);
            mover = new Leapfrog();
            grid = new Grid2D();
            grid.InitializeGrid(project.GridN, project.GridM, 0, project.Length, 0, project.Height);
            mesh = new Mesh2D();
            mesh.InitializeMesh(grid.N * grid.M);
            interpolator = new CloudInCell(particles, grid, mesh, false);
            poissonSolver = new Poisson2DFdmSolver(grid, boundaryConditions);
            poissonSolver.FdmMatrix = poissonSolver.BuildMatrix();
            h = step * Constants.LightVelocity;
            Monitor = new PICMonitor(grid, mesh, particles,this);
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

            mesh.ResetDensity();
            interpolator.InterpolateDensity();

            var vector = poissonSolver.BuildVector(mesh);
            Monitor.BeginPoissonSolve();
            mesh.Potential = poissonSolver.Solve(poissonSolver.FdmMatrix, vector);
            Monitor.EndPoissonSolve();


            Gradient.Calculate(mesh.Potential, mesh.Ex, mesh.Ey, grid.N, grid.M, grid.Hx, grid.Hy);

            particles.ResetForces();
            interpolator.InterpolateForces();

            for (var i = 0; i < emitter.ParticlesCount; i++)
            {
                mover.Prepare(particles, injectedParticlesId[i], h);
            }

            foreach (var index in particles.EnumerateIndexes())
            {
                mover.Step(particles, index, h);

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
                        particles.Multiply(Field.Py, index, -beta);
                        particles.Multiply(Field.Q, index, alfa);
                        if (particles.Get(Field.Q, index) > 0.05 * emitter.ParticleCharge) particles.RemoveAt(index);
                    }

                    var cell = grid.FindCell(particles.Get(Field.X, index), particles.Get(Field.Y, index));
                    particles.SetParticleCell(index, cell);
                }
            }

            Monitor.EndIteration();
            return double.NaN;
        }
    }
}