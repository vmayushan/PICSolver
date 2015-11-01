using PICSolver.Abstract;
using PICSolver.Domain;
using PICSolver.Storage;

namespace PICSolver.Mover
{
    public class Leapfrog : IMover
    {
        public void Prepare(IParticleStorage<Particle> particles, int index, double h)
        {
            var px = particles.Get(Field.Px, index) + 0.5 * h * Constants.Alfa * particles.Get(Field.Fx, index);
            var py = particles.Get(Field.Py, index) + 0.5 * h * Constants.Alfa * particles.Get(Field.Fy, index);
            particles.Set(Field.Px, index, px);
            particles.Set(Field.Py, index, py);
        }

        public void Step(IParticleStorage<Particle> particles, int index, double h)
        {
            var px = particles.Get(Field.Px, index) + h * Constants.Alfa * particles.Get(Field.Fx, index);
            var py = particles.Get(Field.Py, index) + h * Constants.Alfa * particles.Get(Field.Fy, index);
            var x = particles.Get(Field.X, index) + h * Constants.Beta(px);
            var y = particles.Get(Field.Y, index) + h * Constants.Beta(py);
            particles.Update(index, x, y, px, py);
        }
    }
}
