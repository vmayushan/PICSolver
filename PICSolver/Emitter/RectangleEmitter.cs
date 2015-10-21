using PICSolver.Abstract;
using System;

namespace PICSolver.Emitter
{
    public class RectangleEmitter : IEmitter
    {
        private double _x0;
        private double _y0;

        private double _xN;
        private double _yN;

        private int _n;

        public int N { get { return _n; } }
        public double Length { get { return Math.Sqrt(Math.Pow((_xN - _x0), 2) + Math.Pow((_yN - _y0), 2)); } }

        public RectangleEmitter(double x0, double y0, double xN, double yN, int n)
        {
            _x0 = x0;
            _xN = xN;
            _y0 = y0;
            _yN = yN;
            _n = n;
        }

        public double[] GetParticlesToInject()
        {
            var dx = _xN - _x0;
            var dy = _yN - _y0;
            if (_n == 1) return new double[] { _x0, _y0 };
            var injectPositions = new double[2 * _n];
            for (int i = 0; i < _n; i++)
            {
                var cX = _x0 + i * dx / (_n - 1.0);
                var cY = _y0 + i * dy / (_n - 1.0);
                injectPositions[2 * i] = cX;
                injectPositions[2 * i + 1] = cY;
            }
            return injectPositions;
        }
    }
}
