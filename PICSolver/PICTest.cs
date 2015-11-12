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
    public class PICTest
    {
        private BoundaryConditions boundaryConditions;
        private IGrid2D grid;
        private IInterpolationScheme interpolator;
        private IMesh mesh;
        private IParticleStorage<Particle> particles;
        private IFieldSolver poissonSolver;
        private double step;
        private double u;
        public void Test()
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


            grid = new Grid2D();
            grid.InitializeGrid(5, 5, 0, 4, 0, 4);
            mesh = new Mesh2D();
            mesh.InitializeMesh(grid.N * grid.M);
            interpolator = new CloudInCellCurrentLinkage(particles, grid, mesh, step);
            poissonSolver = new Poisson2DFdmSolver(grid, boundaryConditions);
            poissonSolver.Matrix = poissonSolver.BuildMatrix();
            var particle = new Particle(0.25, 0.25, 0, 0, 0, 0, 1);
            //var particle = new Particle(3.75, 2.75, 0, 0, 0, 0, 1);
            var particleId = particles.Add(particle);
            particles.Set(Field.PrevX, particleId, 3.75);
            particles.Set(Field.PrevY, particleId, 2.75);
            //particles.Set(Field.PrevX, particleId, 0.25);
            //particles.Set(Field.PrevY, particleId, 0.25);

            var cell = grid.FindCell(particles.Get(Field.X, particleId), particles.Get(Field.Y, particleId));
            particles.SetParticleCell(particleId, cell);

            interpolator.InterpolateToGrid();

        }
    }
}