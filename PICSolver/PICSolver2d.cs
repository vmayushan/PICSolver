using MathNet.Numerics.LinearAlgebra;
using PICSolver.Abstract;
using PICSolver.Derivative;
using PICSolver.Domain;
using PICSolver.Emitter;
using PICSolver.Grid;
using PICSolver.Mover;
using PICSolver.Poisson;
using PICSolver.Storage;
using System;
using PICSolver.Mesh;
using Constants = PICSolver.Domain.Constants;

namespace PICSolver
{
    public class PICSolver2D
    {
        private IParticleStorage<Particle> _particles;
        private IEmitter _emitter;
        private IMover _mover;
        private IFieldSolver _poissonSolver;
        private IRectangleGrid _grid;
        private BoundaryConditions _conditions;
        private IInterpolationScheme _interpolation;
        private IMesh _mesh;
        private double _step;
        private double _u;

        private Matrix<double> _poissonMatrixFDM;
        private double _startImpact;
        private double _h;

        public PICMonitor Monitor { get; set; }
        //в качестве начального решения Пуассона использовать старое
        public void Prepare()
        {
            var e0 = 10;
            _step = 1E-11;
            _u = 100000;
            var maxParticles = 100000;
            _particles = new ParticleArrayStorage<Particle>(maxParticles);
            _conditions = new BoundaryConditions();
            _conditions.Top = new BoundaryCondition() { Value = (x) => 0, Type = BoundaryConditionType.Neumann };
            _conditions.Bottom = new BoundaryCondition() { Value = (x) => 0, Type = BoundaryConditionType.Neumann };
            _conditions.Left = new BoundaryCondition() { Value = (x) => 0, Type = BoundaryConditionType.Dirichlet };
            _conditions.Right = new BoundaryCondition() { Value = (x) => _u, Type = BoundaryConditionType.Dirichlet };
            _emitter = new RectangleEmitter(1E-6, 0.04, 1E-6, 0.06, 101);
            _mover = new Leapfrog();
            _grid = new RectangleGrid();
            _grid.InitializeGrid(101, 101, 0, 0.1, 0, 0.1);
            _mesh = new Mesh2D();
            _mesh.InitializeMesh(_grid.N * _grid.M);
            _interpolation = new CloudInCell(_particles, _grid, _mesh);
            _poissonSolver = new Poisson2DFdmSolver(_grid, _conditions);
            _poissonMatrixFDM = _poissonSolver.BuildMatrix();
            double gamma = 1 - Constants.Alfa * e0;
            double beta = Math.Sqrt(gamma * gamma - 1) / gamma;
            _startImpact = beta / Math.Sqrt(1 - beta * beta);
            _h = _step * Constants.LightVelocity;
            Monitor = new PICMonitor(_grid, _mesh, _particles);
        }

        public void Step()
        {
            Monitor.BeginIteration();
            var particlesToInject = _emitter.GetParticlesToInject();
            var injectedParticles = new int[_emitter.N];
            var emissionCurrent = -Constants.ChildLangmuirCurrent(0.1, _u) * _emitter.Length / _emitter.N * _step;
            for (int i = 0; i < _emitter.N; i++)
            {
                var cell = _grid.FindCell(particlesToInject[2 * i], particlesToInject[2 * i + 1]);
                var id = _particles.Add(new Particle(particlesToInject[2 * i], particlesToInject[2 * i + 1], _startImpact, 0, 0, 0, emissionCurrent));
                _particles.SetParticleCell(id, cell);
                injectedParticles[i] = id;
            }

            _mesh.ResetDensity();
            _interpolation.InterpolateToGrid();

            var vector = _poissonSolver.BuildVector(_mesh);
            Monitor.BeginPoissonSolve();
            _mesh.Potential = _poissonSolver.Solve(_poissonMatrixFDM, vector);
            Monitor.EndPoissonSolve();
            ElectricField.EvaluateFlatten(_mesh.Potential, _mesh.Ex, _mesh.Ey, _grid.N, _grid.M, _grid.Hx, _grid.Hy);

            _particles.ResetForces();
            _interpolation.InterpolateForces();

            for (int i = 0; i < _emitter.N; i++)
            {
                _mover.Prepare(_particles, injectedParticles[i],  _h);
            }


            foreach (var index in _particles.EnumerateIndexes())
            {
                _mover.Step(_particles, index,  _h);

                if (_grid.IsOutOfGrid(_particles.Get(Field.X, index), _particles.Get(Field.Y, index)))
                {
                    _particles.RemoveAt(index);
                }
                else
                {
                    var cell = _grid.FindCell(_particles.Get(Field.X, index), _particles.Get(Field.Y, index));
                    _particles.SetParticleCell(index, cell);
                }
            }

            Monitor.EndIteration();
        }


    }
}
