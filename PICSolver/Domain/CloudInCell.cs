using PICSolver.Abstract;

namespace PICSolver.Domain
{
    public class CloudInCell : IInterpolationScheme
    {
        private IParticleStorage<Particle> particles;
        private IRectangleGrid grid;

        public CloudInCell(IParticleStorage<Particle> particles, IRectangleGrid grid)
        {
            this.particles = particles;
            this.grid = grid;
        }

        public void InterpolateToGrid()
        {
            foreach (var p in particles)
            {
                int id = particles.GetCell(p.Id);
                int top = grid.GetTopIndex(id);
                var density = p.Q / grid.CellSquare;

                grid.AddDensity(id, density * grid.InterpolateWeight(p.X, p.Y, top + 1));
                grid.AddDensity(id + 1, density * grid.InterpolateWeight(p.X, p.Y, top));
                grid.AddDensity(top, density * grid.InterpolateWeight(p.X, p.Y, id + 1));
                grid.AddDensity(top + 1, density * grid.InterpolateWeight(p.X, p.Y, id));
            }
        }
        public void InterpolateForces()
        {
            foreach (var p in particles)
            {
                int id = particles.GetCell(p.Id);
                int top = grid.GetTopIndex(id);

                particles.AddForce(p.Id, grid.GetEx(id) * grid.InterpolateWeight(p.X, p.Y, top + 1), grid.GetEy(id) * grid.InterpolateWeight(p.X, p.Y, top + 1));
                particles.AddForce(p.Id, grid.GetEx(id + 1) * grid.InterpolateWeight(p.X, p.Y, top), grid.GetEy(id + 1) * grid.InterpolateWeight(p.X, p.Y, top));
                particles.AddForce(p.Id, grid.GetEx(top) * grid.InterpolateWeight(p.X, p.Y, id + 1), grid.GetEy(top) * grid.InterpolateWeight(p.X, p.Y, id + 1));
                particles.AddForce(p.Id, grid.GetEx(top + 1) * grid.InterpolateWeight(p.X, p.Y, id), grid.GetEy(top + 1) * grid.InterpolateWeight(p.X, p.Y, id));
            }
        }
    }
}
