using PICSolver.Abstract;
using PICSolver.Derivative;
using PICSolver.Domain;
using PICSolver.Emitter;
using PICSolver.Extensions;
using PICSolver.Grid;
using PICSolver.Interpolation;
using PICSolver.Mesh;
using PICSolver.Mover;
using PICSolver.Poisson;
using PICSolver.Storage;

namespace PICSolver
{
    // ReSharper disable once InconsistentNaming
    public class IterativePICSolver2D
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

        public PICMonitor Monitor { get; set; }
        public void Prepare()
        {
            var particlesCount = 101;
            step = 1E-12;
            u = 100000;
            particles = new ParticleArrayStorage<Particle>(particlesCount);
            boundaryConditions = new BoundaryConditions
            {
                Top = new BoundaryCondition { Value = x => 0, Type = BoundaryConditionType.Neumann },
                Bottom = new BoundaryCondition { Value = x => 0, Type = BoundaryConditionType.Neumann },
                Left = new BoundaryCondition { Value = x => 0, Type = BoundaryConditionType.Dirichlet },
                Right = new BoundaryCondition { Value = x => u, Type = BoundaryConditionType.Dirichlet }
            };

            emitter = new Emitter2D(0, 0.04, 0, 0.06, particlesCount, 0, 0, -Constants.ChildLangmuirCurrent(0.1, u), step);
            mover = new Leapfrog();
            grid = new Grid2D();
            grid.InitializeGrid(101, 101, 0, 0.1, 0, 0.1);
            mesh = new Mesh2D();
            mesh.InitializeMesh(grid.N * grid.M);
            interpolator = new CloudInCellCurrentLinkage(particles, grid, mesh);
            poissonSolver = new Poisson2DFdmSolver(grid, boundaryConditions);
            poissonSolver.Matrix = poissonSolver.BuildMatrix();
            h = step * Constants.LightVelocity;
            Monitor = new PICMonitor(grid, mesh, particles);
            density = new double[grid.N * grid.M];
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

            var vector = poissonSolver.BuildVector(mesh);
            Monitor.BeginPoissonSolve();
            mesh.Potential = poissonSolver.Solve(poissonSolver.Matrix, vector);
            Monitor.EndPoissonSolve();
            mesh.ResetDensity();
            ElectricField.EvaluateFlatten(mesh.Potential, mesh.Ex, mesh.Ey, grid.N, grid.M, grid.Hx, grid.Hy);

            particles.ResetForces();
            interpolator.InterpolateForces();

            for (var i = 0; i < emitter.ParticlesCount; i++)
            {
                mover.Prepare(particles, injectedParticlesId[i], h);
            }

            while (particles.Count > 0) MoveParticles();
            var w = 0.5;
            
            mesh.Density = MatrixExtensions.Sum(MatrixExtensions.Multiply(density, 1.0 - w), MatrixExtensions.Multiply(mesh.Density, w));
            density = mesh.Density.Clone() as double[];

            Monitor.EndIteration();
        }

        public void MoveParticles()
        {
            interpolator.InterpolateToGrid();
            particles.ResetForces();
            interpolator.InterpolateForces();
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
            particles.ResetForces();
            interpolator.InterpolateForces();


        }
    }
}