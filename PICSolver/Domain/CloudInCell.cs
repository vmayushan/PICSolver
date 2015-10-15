using PICSolver.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICSolver.Domain
{
    public class CloudInCell2d
    {
        private IParticleStorage<IParticle> particles;
        private IRectangleGrid grid;

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

                particles.AddForceToParticle(p.Id, grid.GetEx(id) * grid.InterpolateWeight(p.X, p.Y, top + 1), grid.GetEy(id) * grid.InterpolateWeight(p.X, p.Y, top + 1));
                particles.AddForceToParticle(p.Id, grid.GetEx(id + 1) * grid.InterpolateWeight(p.X, p.Y, top), grid.GetEy(id + 1) * grid.InterpolateWeight(p.X, p.Y, top));
                particles.AddForceToParticle(p.Id, grid.GetEx(top) * grid.InterpolateWeight(p.X, p.Y, id + 1), grid.GetEy(top) * grid.InterpolateWeight(p.X, p.Y, id + 1));
                particles.AddForceToParticle(p.Id, grid.GetEx(top + 1) * grid.InterpolateWeight(p.X, p.Y, id), grid.GetEy(top + 1) * grid.InterpolateWeight(p.X, p.Y, id));
            }
        }
    }
}
