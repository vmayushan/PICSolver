using MathNet.Numerics.Data.Text;
using MathNet.Numerics.LinearAlgebra;
using PICSolver.Abstract;
using System;

namespace PICSolver.Grid
{
    public class RectangleGrid : IRectangleGrid
    {
        private double[] _x;
        private double[] _y;
        private int[] _up;

        private double[] _Ex;
        private double[] _Ey;
        private double[] _rho;

        private int _n;
        private int _m;
        private int _cells;

        private double _hx;
        private double _hy;

        private double[] _gridx;
        private double[] _gridy;

        private double _left;
        private double _right;
        private double _bottom;
        private double _top;

        public double CellSquare { get { return _hx * _hy; } }

        public double Hx { get { return _hx; } }

        public double Hy { get { return _hy; } }

        public int N { get { return _n; } }

        public int M { get { return _m; } }

        public double[] X { get { return _gridx; } }

        public double[] Y { get { return _gridy; } }

        public double[] Ex { get { return _Ex; } set { _Ex = value; } }

        public double[] Ey { get { return _Ey; } set { _Ey = value; } }

        public double[] Rho { get { return _rho; } }

        public double[] Potential { get; set; }

        public int GetTopIndex(int cell)
        {
            return _up[cell];
        }

        public double GetEx(int cell)
        {
            return _Ex[cell];
        }

        public double GetEy(int cell)
        {
            return _Ey[cell];
        }

        public void AddDensity(int cell, double density)
        {
            _rho[cell] += density;
        }
        public double GetDensity(int cell)
        {
            return _rho[cell];
        }

        public int FindCell(double x, double y)
        {
            var cell = (int)Math.Floor(y / _hy) * _n + (int)Math.Floor(x / _hx);
            if (cell > _cells || cell < 0) throw new ApplicationException();
            return cell;
            //for (int i = _cells - 1; i >= 0; i--)
            //    if (x >= _x[i] && y >= _y[i]) return i;
            //throw new ApplicationException();
        }

        public bool IsOutOfGrid(double x, double y)
        {
            if (x > _right || x < _left || y > _top || y < _bottom) return true;
            return false;
        }

        public void InitializeGrid(int n, int m, double left, double right, double bottom, double top)
        {
            if (n < 0) { throw new ArgumentOutOfRangeException("n"); }
            if (m < 0) { throw new ArgumentOutOfRangeException("m"); }

            _n = n;
            _m = m;
            _left = left;
            _right = right;
            _top = top;
            _bottom = bottom;
            _cells = _n * _m;

            _gridx = LinearSpaced(_n, left, right);
            _gridy = LinearSpaced(_m, bottom, top);

            _x = new double[_cells];
            _y = new double[_cells];
            _up = new int[_cells];

            for (int i = 0; i < _n; i++)
            {
                for (int j = 0; j < _m; j++)
                {
                    _x[j * _n + i] = _gridx[i];
                    _y[j * _n + i] = _gridy[j];
                }
            }

            for (int i = 0; i < _cells; i++)
            {
                _up[i] = (i + _n < _cells) ? i + _n : -1;
            }

            _hx = _x[1] - _x[0];
            _hy = _y[_n] - _y[0];

            _rho = new double[_cells];
            _Ex = new double[_cells];
            _Ey = new double[_cells];
        }

        public void ResetDensity()
        {
            for (int i = 0; i < _cells; i++)
            {
                _rho[i] = 0;
            }
        }
        public double InterpolateWeight(double x, double y, int cellId)
        {
            var dxdy = (_x[cellId] - x) * (_y[cellId] - y);
            var weight = Math.Abs(dxdy) / (_hx * _hy);
            if (weight < 0 || weight > 1.01) throw new ApplicationException();
            return weight;
        }
        public double[] LinearSpaced(int length, double start, double stop)
        {
            if (length < 0) { throw new ArgumentOutOfRangeException("length"); }

            if (length == 0) return new double[0];
            if (length == 1) return new[] { start };

            double step = (stop - start) / (length - 1);

            var data = new double[length];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = start + i * step;
            }
            data[data.Length - 1] = stop;
            return data;
        }

    }
}
