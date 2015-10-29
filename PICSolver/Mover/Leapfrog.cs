using PICSolver.Abstract;
using PICSolver.Domain;
using PICSolver.Storage;
using System;

namespace PICSolver.Mover
{
    public class Leapfrog : IMover
    {
        public void Prepare(int index, IParticleStorage<Particle> storage, IRectangleGrid grid, double h)
        {
            var p = storage.At(index);
            var px = p.Px + 0.5 * h * Constants.Alfa * p.Ex;
            var py = p.Py + 0.5 * h * Constants.Alfa * p.Ey;
            storage.Update(index, p.X, p.Y, px, py);
        }

        public void Step(int index, IParticleStorage<Particle> storage, IRectangleGrid grid, double h)
        {
            var p = storage.At(index);
            var px = p.Px + h * Constants.Alfa * p.Ex;
            var py = p.Py + h * Constants.Alfa * p.Ey;
            var x = p.X + h * p.BetaX;
            var y = p.Y + h * p.BetaY;
            storage.Update(index, x, y, px, py);
        }
    }
}
