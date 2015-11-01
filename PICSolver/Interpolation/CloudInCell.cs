using System;
using System.Diagnostics;
using PICSolver.Abstract;
using PICSolver.Domain;
using PICSolver.Storage;

namespace PICSolver.Interpolation
{
    public class CloudInCell : IInterpolationScheme
    {
        private readonly IParticleStorage<Particle> particles;
        private readonly IGrid2D grid;
        private readonly IMesh mesh;

        public CloudInCell(IParticleStorage<Particle> particles, IGrid2D grid, IMesh mesh)
        {
            this.particles = particles;
            this.grid = grid;
            this.mesh = mesh;
        }

        public void InterpolateToGrid()
        {
            foreach (var particleId in particles.EnumerateIndexes())
            {
                int cellId = particles.GetParticleCell(particleId);
                var topCellId = grid.UpperCell(cellId);
                var density = particles.Get(Field.Q, particleId) / grid.CellSquare;

                var x = particles.Get(Field.X, particleId);
                var y = particles.Get(Field.Y, particleId);

                mesh.AddDensity(cellId, density * InterpolateWeight(x, y, topCellId + 1));
                mesh.AddDensity(cellId + 1, density * InterpolateWeight(x, y, topCellId));
                mesh.AddDensity(topCellId, density * InterpolateWeight(x, y, cellId + 1));
                mesh.AddDensity(topCellId + 1, density * InterpolateWeight(x, y, cellId));
            }
        }
        public void InterpolateForces()
        {
            foreach (var particleId in particles.EnumerateIndexes())
            {
                var cellId = particles.GetParticleCell(particleId);
                var topCellId = grid.UpperCell(cellId);

                var x = particles.Get(Field.X, particleId);
                var y = particles.Get(Field.Y, particleId);

                var leftBottomWeight = InterpolateWeight(x, y, topCellId + 1);
                var rightBottomWeight = InterpolateWeight(x, y, topCellId);
                var leftTopWeight = InterpolateWeight(x, y, cellId + 1);
                var rightTopWeight = InterpolateWeight(x, y, cellId);

                particles.AddForce(particleId, mesh.GetEx(cellId) * leftBottomWeight, mesh.GetEy(cellId) * leftBottomWeight);
                particles.AddForce(particleId, mesh.GetEx(cellId + 1) * rightBottomWeight, mesh.GetEy(cellId + 1) * rightBottomWeight);
                particles.AddForce(particleId, mesh.GetEx(topCellId) * leftTopWeight, mesh.GetEy(topCellId) * leftTopWeight);
                particles.AddForce(particleId, mesh.GetEx(topCellId + 1) * rightTopWeight, mesh.GetEy(topCellId + 1) * rightTopWeight);
            }
        }

        private double InterpolateWeight(double x, double y, int cellId)
        {
            var dxdy = (grid.GetCell(cellId)[0] - x) * (grid.GetCell(cellId)[1] - y);
            var weight = Math.Abs(dxdy) / (grid.Hx * grid.Hy);
            Debug.Assert(weight >= 0 || weight <= 1);
            return weight;
        }
    }
}
