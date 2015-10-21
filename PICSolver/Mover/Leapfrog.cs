using PICSolver.Abstract;
using PICSolver.Domain;

namespace PICSolver.Mover
{
    public class Leapfrog : IMover
    {
        public void Prepare(int index, IParticleStorage<Particle> storage, IRectangleGrid grid, double h)
        {
            this.Step(index, storage, grid, -0.5 * h);
        }

        public void Step(int index, IParticleStorage<Particle> storage, IRectangleGrid grid, double h)
        {
            var p = storage.At(index);
            var px = p.Px + h * Constants.Alfa * p.Ex;
            var py = p.Px + h * Constants.Alfa * p.Ey;
            var x = p.X + h * p.BetaX;
            var y = p.Y + h * p.BetaY;
            storage.Update(index, x, y, px, py);

            if (grid.OutOfGrid(x, y))
            {
                storage.RemoveAt(index);
                return;
            }
            var cell = grid.FindCell(x, y);
            storage.SetCell(index, cell);

        }
    }
}
