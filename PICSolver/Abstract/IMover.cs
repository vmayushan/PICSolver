using PICSolver.Domain;

namespace PICSolver.Abstract
{
    public interface IMover
    {
        void Step(int index, IParticleStorage<Particle> storage, IRectangleGrid grid, double h);
        void Prepare(int index, IParticleStorage<Particle> storage, IRectangleGrid grid, double h);
    }
}
