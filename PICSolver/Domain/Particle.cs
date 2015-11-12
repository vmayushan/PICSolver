using System;
using PICSolver.Abstract;

namespace PICSolver.Domain
{
    public struct Particle : IParticle
    {
        public Particle(double x, double y, double px, double py, double ex, double ey, double q) : this()
        {
            X = x;
            Y = y;
            Px = px;
            Py = py;
            Q = q;
            Ex = ex;
            Ey = ey;
            PrevX = x;
            PrevY = y;
        }
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Px { get; set; }
        public double Py { get; set; }
        public double Ex { get; set; }
        public double Ey { get; set; }
        public double Q { get; set; }
        public double PrevX { get; set; }
        public double PrevY { get; set; }
        public double BetaX => Px / Math.Sqrt(1 + Px * Px);
        public double BetaY => Py / Math.Sqrt(1 + Py * Py);
        public double Beta => Math.Sqrt(BetaX * BetaX + BetaY * BetaY);
        public double LorentzFactor => 1 / Math.Sqrt(1 - Beta * Beta);
        public double KineticEnergy => (-1 / Constants.Alfa) * (LorentzFactor - 1);
        public override string ToString()
        {
            // ReSharper disable once UseStringInterpolation
            return string.Format("x = {0}, y = {1}, px = {2}, py = {3}, q = {4}", X, Y, Px, Py, Q);
        }
    }
}