using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly bool parallel;

        public CloudInCell(IParticleStorage<Particle> particles, IGrid2D grid, IMesh mesh, bool parallel)
        {
            this.particles = particles;
            this.grid = grid;
            this.mesh = mesh;
            this.parallel = parallel;
        }

        public void InterpolateDensity()
        {
            if (parallel)
            {
                var rangePartitioner = Partitioner.Create(0, particles.Count, particles.Count / 4 + 1);
                var list = particles.EnumerateIndexes().ToArray();
                Parallel.ForEach(rangePartitioner, range =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                        InterpolateDensityToParticle(list[i]);
                });
            }
            else
            {
                foreach (var particleId in particles.EnumerateIndexes())
                    InterpolateDensityToParticle(particleId);
            }
        }
        public void InterpolateForces()
        {
            if (parallel)
            {
                var rangePartitioner = Partitioner.Create(0, particles.Count, particles.Count / 4 + 1);
                var list = particles.EnumerateIndexes().ToArray();
                Parallel.ForEach(rangePartitioner, range =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                        InterpolateForcesFromParticle(list[i]);
                });
            }
            else
            {
                foreach (var particleId in particles.EnumerateIndexes())
                    InterpolateForcesFromParticle(particleId);
            }
        }

        private void InterpolateDensityToParticle(int particleId)
        {
            int cellId = particles.GetParticleCell(particleId);
            var topCellId = grid.UpperCell(cellId);
            var density = particles.Get(Field.Q, particleId) / grid.CellSquare;

            var x = particles.Get(Field.X, particleId);
            var y = particles.Get(Field.Y, particleId);

            mesh.AddDensity(cellId, density * InterpolateWeight(x, y, topCellId + 1), parallel);
            mesh.AddDensity(cellId + 1, density * InterpolateWeight(x, y, topCellId), parallel);
            mesh.AddDensity(topCellId, density * InterpolateWeight(x, y, cellId + 1), parallel);
            mesh.AddDensity(topCellId + 1, density * InterpolateWeight(x, y, cellId), parallel);
        }

        private void InterpolateForcesFromParticle(int particleId)
        {
            var cellId = particles.GetParticleCell(particleId);
            var topCellId = grid.UpperCell(cellId);

            var x = particles.Get(Field.X, particleId);
            var y = particles.Get(Field.Y, particleId);

            var leftBottomWeight = InterpolateWeight(x, y, topCellId + 1);
            var rightBottomWeight = InterpolateWeight(x, y, topCellId);
            var leftTopWeight = InterpolateWeight(x, y, cellId + 1);
            var rightTopWeight = InterpolateWeight(x, y, cellId);

            particles.Increment(Field.Ex, particleId, mesh.GetEx(cellId) * leftBottomWeight +
                                                    mesh.GetEx(cellId + 1) * rightBottomWeight +
                                                    mesh.GetEx(topCellId) * leftTopWeight +
                                                    mesh.GetEx(topCellId + 1) * rightTopWeight);
            particles.Increment(Field.Ey, particleId, mesh.GetEy(cellId) * leftBottomWeight +
                                                    mesh.GetEy(cellId + 1) * rightBottomWeight +
                                                    mesh.GetEy(topCellId) * leftTopWeight +
                                                    mesh.GetEy(topCellId + 1) * rightTopWeight);
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
