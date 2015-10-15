using System;

namespace PICSolver.Derivative
{
    public enum DerivativeType
    {
        Forward,
        Backward,
        Central
    }
    public class ScalarDerivative
    {
        private double Forward(double[] y, int i, double h)
        {
            if (y == null || y.Length < 3 || i < 0 || i > y.Length - 3 || h == 0)
                throw new ArgumentException();
            
            return (-3 * y[i] + 4 * y[i + 1] - y[i + 2]) / 2 / h;
            return (y[i + 1] - y[i]) / h;
        }

        private double Central(double[] y, int i, double h)
        {
            if (y == null || y.Length < 2 || i < 1 || i > y.Length - 2 || h == 0)
                throw new ArgumentException();
            return (y[i + 1] - y[i - 1]) / 2 / h;
        }
        private double Backward(double[] y, int i, double h)
        {
            if (y == null || y.Length < 3 || i < 0 || i < 2 || h == 0)
                throw new ArgumentException();
            
            return (3 * y[i] - 4 * y[i - 1] + y[i - 2]) / 2 / h;
            return (y[i] - y[i - 1]) / h;
        }
        public double Evaluate(double[] y, int i, double h, DerivativeType type)
        {
            switch (type)
            {
                case DerivativeType.Backward:
                    return Backward(y, i, h);
                case DerivativeType.Forward:
                    return Forward(y, i, h);
                case DerivativeType.Central:
                    return Central(y, i, h);
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
