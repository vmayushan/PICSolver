using System;
using PICSolver.Abstract;
using PICSolver.Domain;

namespace PICSolver.Emitter
{
    public class Emitter2D : IEmitter
    {
        private readonly double fromX;
        private readonly double toX;
        private readonly double fromY;
        private readonly double toY;
        private readonly double px;
        private readonly double py;
        private readonly double currentDensity;
        private readonly double dx;
        private readonly double dy;
        private readonly double step;
        private readonly Particle[] particles;

        public Emitter2D(double x0, double y0, double xN, double yN, int particlesCount, double energyX, double energyY, double currentDensity, double step)
        {
            fromX = x0;
            toX = xN;
            fromY = y0;
            toY = yN;
            ParticlesCount = particlesCount;

            var gammaX = Constants.KineticEnergyToLorentzFactor(energyX);
            var betaX = Constants.LorentzFactorToBeta(gammaX);
            px = Constants.Momentum(betaX);

            var gammaY = Constants.KineticEnergyToLorentzFactor(energyY);
            var betaY = Constants.LorentzFactorToBeta(gammaY);
            py = Constants.Momentum(betaY);

            dx = (toX - fromX) / (ParticlesCount - 1.0);
            dy = (toY - fromY) / (ParticlesCount - 1.0);

            this.currentDensity = currentDensity;
            this.step = step;
            particles = new Particle[ParticlesCount];
        }

        public int ParticlesCount { get; }

        public double Length => Math.Sqrt(Math.Pow((toX - fromX), 2) + Math.Pow((toY - fromY), 2));

        public Particle[] Inject()
        {
            for (var i = 0; i < ParticlesCount; i++)
            {
                var x = fromX + i * dx;
                var y = fromY + i * dy;
                var q = currentDensity * Length * step / ParticlesCount;
                particles[i] = new Particle(x, y, px, py, 0, 0, q);
            }

            return particles;
        }
    }
}