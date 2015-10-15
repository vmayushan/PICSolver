using PICSolver.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICSolver.Domain
{
    public struct Particle : IParticle
    {
        public Particle(double x, double y, double px, double py, double q) : this()
        {
            this.X = x;
            this.Y = y;
            this.Px = px;
            this.Py = py;
            this.Q = q;
        }
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Px { get; set; }
        public double Py { get; set; }
        public double Q { get; set; }
        public double BetaX { get { return Px / Math.Sqrt(1 + Px * Px); } }
        public double BetaY { get { return Py / Math.Sqrt(1 + Py * Py); } }
        public double Beta { get { return Math.Sqrt(BetaX * BetaX + BetaY * BetaY); } }
        public double LorentzFactor { get { return 1 / Math.Sqrt(1 - Beta * Beta); } }
        public double KineticEnergy { get { return (-1 / Constants.Alfa) * (LorentzFactor - 1); } }
        public override string ToString()
        {
            return string.Format("x = {0}, y = {1}, px = {2}, py = {3}, q = {4}", X, Y, Px, Py, Q);
        }
        private double BetaToMomentum(double beta)
        {
            return beta / Math.Sqrt(1 - beta * beta);
        }
        private double LorentzFactorToBeta(double gamma)
        {
            return Math.Sqrt(gamma * gamma - 1) / gamma;
        }
        private double KineticEnergyToLorentzFactor(double w)
        {
            return 1 - Constants.Alfa * w;
        }
    }
}
