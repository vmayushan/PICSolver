using System;
using System.Collections.Generic;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double.Solvers;
using MathNet.Numerics.LinearAlgebra.Solvers;
using MathNet.Numerics.Providers.LinearAlgebra.Mkl;
using MathNet.Numerics.Providers.LinearAlgebra.OpenBlas;
using PICSolver.Abstract;
using Constants = PICSolver.Domain.Constants;

namespace PICSolver.Poisson
{
    public class Poisson2DFdmSolver : IFieldSolver
    {

        private IPreconditioner<double> preconditioner;
        private IIterativeSolver<double> solver;
        private Iterator<double> monitor;
        private readonly BoundaryConditions boundary;
        private readonly double dx;
        private readonly double dy;
        private readonly int m;
        private readonly int n;
        private readonly double[] x;
        private readonly double[] y;
        private Vector<double> result;
        public Matrix<double> Matrix { get; set; }

        public Poisson2DFdmSolver(IGrid2D grid, BoundaryConditions boundary)
        {
            this.boundary = boundary;
            n = grid.N;
            m = grid.M;
            x = grid.X;
            y = grid.Y;
            dx = grid.Hx;
            dy = grid.Hy;

            InitializeSolver();
        }

        public Matrix<double> BuildMatrix()
        {
            var size = n * m;

            var matrix = Matrix<double>.Build.Sparse(size, size);

            //Определение коэффициентов и свободных членов СЛАУ, соответствующих граничным условиям по оси X 
            for (var j = 0; j < m; j++)
            {
                //left boundary
                if (boundary.Left.Type == BoundaryConditionType.Dirichlet)
                {
                    matrix[j, j] = 1.0;
                }
                else if (boundary.Left.Type == BoundaryConditionType.Neumann)
                {
                    matrix[j, j] = -1.0 / dx;
                    matrix[j, m + j] = 1.0 / dx;
                }

                //right boundary
                if (boundary.Right.Type == BoundaryConditionType.Dirichlet)
                {
                    matrix[m * (n - 1) + j, m * (n - 1) + j] = 1.0;
                }
                else if (boundary.Right.Type == BoundaryConditionType.Neumann)
                {
                    matrix[m * (n - 1) + j, m * (n - 1) + j] = 1.0 / dx;
                    matrix[m * (n - 1) + j, m * (n - 2) + j] = -1.0 / dx;
                }
            }
            //Определение коэффициентов и свободных членов СЛАУ, соответствующих граничным условиям по оси Y 
            for (var i = 1; i < n - 1; i++)
            {
                //bottom boundary
                if (boundary.Bottom.Type == BoundaryConditionType.Dirichlet)
                {
                    matrix[m * i, m * i] = 1.0;
                }
                else if (boundary.Bottom.Type == BoundaryConditionType.Neumann)
                {
                    matrix[m * i, m * i] = -1.0 / dy;
                    matrix[m * i, m * i + 1] = 1.0 / dy;
                }

                //top boundary
                if (boundary.Top.Type == BoundaryConditionType.Dirichlet)
                {
                    matrix[m * i + m - 1, m * i + m - 1] = 1.0;
                }
                else if (boundary.Top.Type == BoundaryConditionType.Neumann)
                {
                    matrix[m * i + m - 1, m * i + m - 1] = 1.0 / dy;
                    matrix[m * i + m - 1, m * i + m - 2] = -1.0 / dy;
                }
            }
            //Определение коэффициентов СЛАУ, соответствующих внутренним точкам области
            for (var i = 1; i < n - 1; i++)
            {
                for (var j = 1; j < m - 1; j++)
                {
                    matrix[m * i + j, m * i + j] = -2 / (dx * dx) - 2 / (dy * dy);
                    matrix[m * i + j, m * (i + 1) + j] = 1 / (dx * dx);
                    matrix[m * i + j, m * (i - 1) + j] = 1 / (dx * dx);
                    matrix[m * i + j, m * i + j + 1] = 1 / (dy * dy);
                    matrix[m * i + j, m * i + j - 1] = 1 / (dy * dy);
                }
            }
            preconditioner.Initialize(matrix);
            return matrix;
        }

        public Vector<double> BuildVector(IMesh mesh)
        {
            var vector = Vector<double>.Build.Dense(n * m);

            for (var i = 1; i < n - 1; i++)
            {
                for (var j = 1; j < m - 1; j++)
                {
                    vector[m * i + j] = -mesh.GetDensity(n * j + i) / Constants.VacuumPermittivity;
                }
            }
            VectorBoundaries(vector);
            return vector;
        }

        private bool TrySolve(Matrix<double> matrix, Vector<double> right)
        {
            try
            {
                solver.Solve(matrix, right, result, monitor, preconditioner);
            }
            catch (NumericalBreakdownException)
            {
                return false;
            }
            return true;
        }

        public double[] Solve(Matrix<double> matrix, Vector<double> right)
        {
            monitor.Reset();
            for (int i = 0; ; i++)
            {
                if(i == 2) throw new ApplicationException();
                if (TrySolve(matrix, right)) break;
            }

            var newResult = new double[result.Count];
            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < m; j++)
                {
                    newResult[n * j + i] = result.At(m * i + j);
                }
            }
            return newResult;
        }

        private void InitializeSolver()
        {
            result = Vector<double>.Build.Dense(n * m);
            Control.LinearAlgebraProvider = new OpenBlasLinearAlgebraProvider();
            var iterationCountStopCriterion = new IterationCountStopCriterion<double>(100);
            var residualStopCriterion = new ResidualStopCriterion<double>(1e-12);
            monitor = new Iterator<double>(iterationCountStopCriterion, residualStopCriterion);
            solver = new BiCgStab();
            preconditioner = new MILU0Preconditioner();
        }

        private void VectorBoundaries(IList<double> vector)
        {
            for (var j = 0; j < m; j++)
            {
                vector[j] = boundary.Left.Value(y[j]);
                vector[m * (n - 1) + j] = boundary.Right.Value(y[j]);
            }
            for (var i = 1; i < n - 1; i++)
            {
                vector[m * i] = boundary.Bottom.Value(x[i]);
                vector[m * i + m - 1] = boundary.Top.Value(x[i]);
            }
        }

       
    }
}