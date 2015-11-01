using PICSolver.Abstract;

namespace PICSolver.Domain
{
    public class CloudInCell : IInterpolationScheme
    {
        private IParticleStorage<Particle> particles;
        private IRectangleGrid grid;
        private IMesh mesh;

        public CloudInCell(IParticleStorage<Particle> particles, IRectangleGrid grid, IMesh mesh)
        {
            this.particles = particles;
            this.grid = grid;
            this.mesh = mesh;
        }

        public void InterpolateToGrid()
        {
            foreach (var p in particles)
            {
                int id = particles.GetParticleCell(p.Id);
                int top = grid.GetTopIndex(id);
                var density = p.Q / grid.CellSquare;

                mesh.AddDensity(id, density * grid.InterpolateWeight(p.X, p.Y, top + 1));
                mesh.AddDensity(id + 1, density * grid.InterpolateWeight(p.X, p.Y, top));
                mesh.AddDensity(top, density * grid.InterpolateWeight(p.X, p.Y, id + 1));
                mesh.AddDensity(top + 1, density * grid.InterpolateWeight(p.X, p.Y, id));
            }
        }
        public void InterpolateForces()
        {
            foreach (var p in particles)
            {
                int id = particles.GetParticleCell(p.Id);
                int top = grid.GetTopIndex(id);

                particles.AddForce(p.Id, mesh.GetEx(id) * grid.InterpolateWeight(p.X, p.Y, top + 1), mesh.GetEy(id) * grid.InterpolateWeight(p.X, p.Y, top + 1));
                particles.AddForce(p.Id, mesh.GetEx(id + 1) * grid.InterpolateWeight(p.X, p.Y, top), mesh.GetEy(id + 1) * grid.InterpolateWeight(p.X, p.Y, top));
                particles.AddForce(p.Id, mesh.GetEx(top) * grid.InterpolateWeight(p.X, p.Y, id + 1), mesh.GetEy(top) * grid.InterpolateWeight(p.X, p.Y, id + 1));
                particles.AddForce(p.Id, mesh.GetEx(top + 1) * grid.InterpolateWeight(p.X, p.Y, id), mesh.GetEy(top + 1) * grid.InterpolateWeight(p.X, p.Y, id));
            }
        }
    }
}
