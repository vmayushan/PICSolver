using MathNet.Numerics.LinearAlgebra;
using System;

namespace PICSolver.Derivative
{
    public class MatrixGradientResult
    {
        public Matrix<double> Dx { get; set; }
        public Matrix<double> Dy { get; set; }
    }

    public class MatrixGradient
    {
        private static readonly Lazy<MatrixGradient> lazy = new Lazy<MatrixGradient>(() => new MatrixGradient());

        public static MatrixGradient Instance { get { return lazy.Value; } }

        public ScalarDerivative ScalarDerivative { get; set; }

        private MatrixGradient()
        {
            this.ScalarDerivative = new ScalarDerivative();
        }

        public MatrixGradientResult Evaluate(Matrix<double> matrix, double hx = 1.0, double hy = 1.0)
        {
            if (matrix == null) throw new ArgumentException("null", "matrix");
            if (hx == 0 || hy == 0) throw new ArgumentException();

            var dx = Matrix<double>.Build.Dense(matrix.RowCount, matrix.ColumnCount);
            var dy = Matrix<double>.Build.Dense(matrix.RowCount, matrix.ColumnCount);

            this.Evaluate(matrix, dx, dy, hx, hy);

            return new MatrixGradientResult { Dx = dx, Dy = dy };
        }

        public void Evaluate(Matrix<double> matrix, Matrix<double> dx, Matrix<double> dy, double hx = 1.0, double hy = 1.0)
        {
            if (matrix == null) throw new ArgumentException("null", "matrix");
            if (hx == 0 || hy == 0) throw new ArgumentException();

            //итерация по строкам
            for (int i = 0; i < matrix.RowCount; i++)
            {
                //получаем i-ую строку
                var row = matrix.Row(i).ToArray();

                //итерация по [i,j] элементам i-ой строки
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    DerivativeType dtype = (j == 0) ? DerivativeType.Forward : (j == matrix.ColumnCount - 1) ? DerivativeType.Backward : DerivativeType.Central;
                    dx[i, j] = this.ScalarDerivative.Evaluate(row, j, hx, dtype);
                }
            }

            //итерация по столбцам
            for (int j = 0; j < matrix.ColumnCount; j++)
            {
                //получаем j-ый столбец
                var column = matrix.Column(j).ToArray();

                //итерация по [i,j] элементам j-го столбца
                for (int i = 0; i < matrix.RowCount; i++)
                {
                    DerivativeType dtype = (i == 0) ? DerivativeType.Forward : (i == matrix.RowCount - 1) ? DerivativeType.Backward : DerivativeType.Central;
                    dy[i, j] = this.ScalarDerivative.Evaluate(column, i, hy, dtype);
                }
            }
        }

        public void EvaluateFlatten(Matrix<double> matrix, double[] dx, double[] dy, double hx = 1.0, double hy = 1.0)
        {
            if (matrix == null) throw new ArgumentException("null", "matrix");
            if (hx == 0 || hy == 0) throw new ArgumentException();

            //итерация по строкам
            for (int i = 0; i < matrix.RowCount; i++)
            {
                //получаем i-ую строку
                var row = matrix.Row(i).ToArray();

                //итерация по [i,j] элементам i-ой строки
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    DerivativeType dtype = (j == 0) ? DerivativeType.Forward : (j == matrix.ColumnCount - 1) ? DerivativeType.Backward : DerivativeType.Central;
                    dx[i * matrix.ColumnCount + j] = this.ScalarDerivative.Evaluate(row, j, hx, dtype);
                }
            }

            //итерация по столбцам
            for (int j = 0; j < matrix.ColumnCount; j++)
            {
                //получаем j-ый столбец
                var column = matrix.Column(j).ToArray();

                //итерация по [i,j] элементам j-го столбца
                for (int i = 0; i < matrix.RowCount; i++)
                {
                    DerivativeType dtype = (i == 0) ? DerivativeType.Forward : (i == matrix.RowCount - 1) ? DerivativeType.Backward : DerivativeType.Central;
                    dy[i * matrix.ColumnCount + j] = this.ScalarDerivative.Evaluate(column, i, hy, dtype);
                }
            }
        }
    }
}
