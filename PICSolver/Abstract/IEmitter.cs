using PICSolver.Domain;

namespace PICSolver.Abstract
{
    public interface IEmitter
    {
        Particle[] Inject();
        double Length { get; }
        int ParticlesCount { get; }
    }
}
