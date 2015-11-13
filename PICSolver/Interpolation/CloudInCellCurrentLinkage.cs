using System;
using System.Diagnostics;
using System.Linq;
using PICSolver.Abstract;
using PICSolver.Domain;
using PICSolver.Extensions;
using PICSolver.Storage;

namespace PICSolver.Interpolation
{
    public class CloudInCellCurrentLinkage : IInterpolationScheme
    {
        private readonly IParticleStorage<Particle> particles;
        private readonly IGrid2D grid;
        private readonly IMesh mesh;

        public CloudInCellCurrentLinkage(IParticleStorage<Particle> particles, IGrid2D grid, IMesh mesh)
        {
            this.particles = particles;
            this.grid = grid;
            this.mesh = mesh;
        }

        public void InterpolateToGrid()
        {
            foreach (var particleId in particles.EnumerateIndexes())
            {
                var current = particles.Get(Field.Q, particleId) / grid.CellSquare;

                var x = particles.Get(Field.X, particleId);
                var y = particles.Get(Field.Y, particleId);
                var cellId = grid.FindCell(x, y);

                var x1 = particles.Get(Field.PrevX, particleId);
                var y1 = particles.Get(Field.PrevY, particleId);
                var prevCell = grid.FindCell(x1, y1);

                var length = Helpers.Length(x, y, x1, y1);

                var diff = cellId - prevCell;
                double x2, y2, intervalLength, currentRatio;
                switch (diff)
                {
                    case 0:
                        CurrentToCell(cellId, current, x, y, x1, y1);
                        break;
                    case -1: //prevcell - right
                        x2 = grid.GetCell(prevCell)[0];
                        y2 = Helpers.LineY(x1, x, y1, y, x2);
                        intervalLength = Helpers.Length(x, y, x2, y2);
                        currentRatio = intervalLength / length;
                        CurrentToCell(cellId, currentRatio * current, x, y, x2, y2);
                        CurrentToCell(prevCell, (1 - currentRatio) * current, x2, y2, x1, y1);
                        break;
                    case 1: //prevcell - left
                        x2 = grid.GetCell(cellId)[0];
                        y2 = Helpers.LineY(x1, x, y1, y, x2);
                        intervalLength = Helpers.Length(x1, y1, x2, y2);
                        currentRatio = intervalLength / length;
                        CurrentToCell(prevCell, currentRatio * current, x1, y1, x2, y2);
                        CurrentToCell(cellId, (1 - currentRatio) * current, x2, y2, x, y);
                        break;
                    default:
                        if (grid.UpperCell(cellId) == prevCell)
                        {
                            y2 = grid.GetCell(prevCell)[1];
                            x2 = Helpers.LineX(x1, x, y1, y, y2);
                            intervalLength = Helpers.Length(x, y, x2, y2);
                            currentRatio = intervalLength / length;
                            CurrentToCell(cellId, currentRatio * current, x, y, x2, y2);
                            CurrentToCell(prevCell, (1 - currentRatio) * current, x2, y2, x1, y1);
                        }
                        else if (grid.UpperCell(prevCell) == cellId)
                        {
                            y2 = grid.GetCell(cellId)[1];
                            x2 = Helpers.LineX(x1, x, y1, y, y2);
                            intervalLength = Helpers.Length(x1, y1, x2, y2);
                            currentRatio = intervalLength / length;
                            CurrentToCell(prevCell, currentRatio * current, x1, y1, x2, y2);
                            CurrentToCell(cellId, (1 - currentRatio) * current, x2, y2, x, y);
                        }
                        else
                        {
                            var intersect = grid.X.Where(p => p > Math.Min(x, x1) && p < Math.Max(x, x1))
                                    .Select(p => new { X = p, Y = Helpers.LineY(x1, x, y1, y, p) })
                                    .Concat(grid.Y.Where(p => p > Math.Min(y, y1) && p < Math.Max(y, y1))
                                    .Select(p => new { X = Helpers.LineX(x1, x, y1, y, p), Y = p }))
                                    .Concat(new[] { new { X = x, Y = y }, new { X = x1, Y = y1 } })
                                    .OrderBy(p => p.X).ToArray();
                            for (var i = 0; i < intersect.Length - 1; i++)
                            {
                                intervalLength = Helpers.Length(intersect[i].X, intersect[i].Y, intersect[i + 1].X, intersect[i + 1].Y);
                                currentRatio = intervalLength / length;
                                CurrentToCell(grid.FindCell(intersect[i].X, intersect[i].Y), currentRatio * current, intersect[i].X, intersect[i].Y, intersect[i + 1].X, intersect[i + 1].Y);
                            }
                        }
                        break;
                }
            }
        }

        private void CurrentToCell(int cellId, double current, double x1, double y1, double x2, double y2)
        {
            var topCellId = grid.UpperCell(cellId);
            mesh.AddDensity(cellId, 0.5 * current * (InterpolateWeight(x1, y1, topCellId + 1) + InterpolateWeight(x2, y2, topCellId + 1)));
            mesh.AddDensity(cellId + 1, 0.5 * current * (InterpolateWeight(x1, y1, topCellId) + InterpolateWeight(x2, y2, topCellId)));
            mesh.AddDensity(topCellId, 0.5 * current * (InterpolateWeight(x1, y1, cellId + 1) + InterpolateWeight(x2, y2, cellId + 1)));
            mesh.AddDensity(topCellId + 1, 0.5 * current * (InterpolateWeight(x1, y1, cellId) + InterpolateWeight(x2, y2, cellId)));
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
