using PICSolver.Abstract;
using PICSolver.Derivative;
using PICSolver.Domain;
using PICSolver.Emitter;
using PICSolver.Grid;
using PICSolver.Interpolation;
using PICSolver.Mesh;
using PICSolver.Mover;
using PICSolver.Poisson;
using PICSolver.Storage;

namespace PICSolver
{
    // ReSharper disable once InconsistentNaming
    public class PICSolver2D
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

        public PICMonitor Monitor { get; set; }
        public void Prepare()
        {
            step = 1E-11;
            u = 100000;
            particles = new ParticleArrayStorage<Particle>(1000000);
            boundaryConditions = new BoundaryConditions
            {
                Top = new BoundaryCondition { Value = x => 0, Type = BoundaryConditionType.Neumann },
                Bottom = new BoundaryCondition { Value = x => 0, Type = BoundaryConditionType.Neumann },
                Left = new BoundaryCondition { Value = x => 0, Type = BoundaryConditionType.Dirichlet },
                Right = new BoundaryCondition { Value = x => u, Type = BoundaryConditionType.Dirichlet }
            };

            emitter = new Emitter2D(0, 0.04, 0, 0.06, 101, 0, 0, -Constants.ChildLangmuirCurrent(0.1, u), step);
            mover = new Leapfrog();
            grid = new Grid2D();
            grid.InitializeGrid(101, 101, 0, 0.1, 0, 0.1);
            mesh = new Mesh2D();
            mesh.InitializeMesh(grid.N * grid.M);
            interpolator = new CloudInCell(particles, grid, mesh);
            poissonSolver = new Poisson2DFdmSolver(grid, boundaryConditions);
            poissonSolver.Matrix = poissonSolver.BuildMatrix();
            h = step * Constants.LightVelocity;
            Monitor = new PICMonitor(grid, mesh, particles);
        }

        public void Step()
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
            interpolator.InterpolateToGrid();

            var vector = poissonSolver.BuildVector(mesh);
            Monitor.BeginPoissonSolve();
            mesh.Potential = poissonSolver.Solve(poissonSolver.Matrix, vector);
            Monitor.EndPoissonSolve();


            ElectricField.EvaluateFlatten(mesh.Potential, mesh.Ex, mesh.Ey, grid.N, grid.M, grid.Hx, grid.Hy);

            particles.ResetForces();
            interpolator.InterpolateForces();

            for (var i = 0; i < emitter.ParticlesCount; i++)
            {
                mover.Prepare(particles, injectedParticlesId[i], h);
            }

            foreach (var index in particles.EnumerateIndexes())
            {
                mover.Step(particles, index, h);

                if (grid.IsOutOfGrid(particles.Get(Field.X, index), particles.Get(Field.Y, index)))
                {
                    particles.RemoveAt(index);
                }
                else
                {
                    var cell = grid.FindCell(particles.Get(Field.X, index), particles.Get(Field.Y, index));
                    particles.SetParticleCell(index, cell);
                }
            }

            Monitor.EndIteration();
        }
    }
}