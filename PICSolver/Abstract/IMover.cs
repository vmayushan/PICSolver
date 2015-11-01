using PICSolver.Domain;

namespace PICSolver.Abstract
{
    public interface IMover
    {
        void Step(IParticleStorage<Particle> particles, int index, double h);
        void Prepare(IParticleStorage<Particle> particles, int index, double h);
    }
}
