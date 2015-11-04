using System;
using System.Diagnostics;
using PICSolver.Abstract;

namespace PICSolver.Grid
{
    public class Grid2D : IGrid2D
    {
        private double[] cellX;
        private double[] cellY;
        private int[] up;

        public int Count { get; private set; }

        public double Left { get; private set; }
        public double Right { get; private set; }
        public double Bottom { get; private set; }
        public double Top { get; private set; }

        public double CellSquare => Hx * Hy;

        public double Hx { get; private set; }
        public double Hy { get; private set; }

        public int N { get; private set; }
        public int M { get; private set; }

        public double[] X { get; private set; }

        public double[] Y { get; private set; }

        public int UpperCell(int cellId)
        {
            return up[cellId];
        }

        public int FindCell(double x, double y)
        {
            var cell = (int)Math.Floor(y / Hy) * N + (int)Math.Floor(x / Hx);
            Debug.Assert(cell <= Count || cell >= 0);
            return cell;
        }

        public bool IsOutOfGrid(double x, double y)
        {
            if (x > Right || x < Left || y > Top || y < Bottom) return true;
            return false;
        }

        public double[] GetCell(int cellId)
        {
            return new[] { cellX[cellId], cellY[cellId] };
        }

        public void InitializeGrid(int n, int m, double left, double right, double bottom, double top)
        {
            N = n;
            M = m;
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
            Count = N * M;

            X = LinearSpaced(N, left, right);
            Y = LinearSpaced(M, bottom, top);

            cellX = new double[Count];
            cellY = new double[Count];
            up = new int[Count];

            for (var i = 0; i < N; i++)
            {
                for (var j = 0; j < M; j++)
                {
                    cellX[j * N + i] = X[i];
                    cellY[j * N + i] = Y[j];
                }
            }

            for (var i = 0; i < Count; i++)
            {
                up[i] = (i + N < Count) ? i + N : -1;
            }

            Hx = cellX[1] - cellX[0];
            Hy = cellY[N] - cellY[0];
        }

        private double[] LinearSpaced(int length, double start, double stop)
        {
            if (length == 0) return new double[0];
            if (length == 1) return new[] { start };

            var step = (stop - start) / (length - 1);

            var data = new double[length];
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = start + i * step;
            }
            data[data.Length - 1] = stop;
            return data;
        }
    }
}