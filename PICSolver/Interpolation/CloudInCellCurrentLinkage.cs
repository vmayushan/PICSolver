using System;
using System.Diagnostics;
using PICSolver.Abstract;
using PICSolver.Domain;
using PICSolver.Storage;

namespace PICSolver.Interpolation
{
    //old version: http://pastebin.com/w4kUC3K0
    public class CloudInCellCurrentLinkage : IInterpolationScheme
    {
        private readonly IParticleStorage<Particle> particles;
        private readonly IGrid2D grid;
        private readonly IMesh mesh;
        private readonly double step;

        public CloudInCellCurrentLinkage(IParticleStorage<Particle> particles, IGrid2D grid, IMesh mesh, double step)
        {
            this.particles = particles;
            this.grid = grid;
            this.mesh = mesh;
            this.step = step;
        }

        public void InterpolateToGrid()
        {
            foreach (var particleId in particles.EnumerateIndexes())
            {
                var current = particles.Get(Field.Q, particleId) * step / grid.CellSquare;

                var x = particles.Get(Field.X, particleId);
                var y = particles.Get(Field.Y, particleId);
                var cellId = grid.FindCell(x, y);

                var prevX = particles.Get(Field.PrevX, particleId);
                var prevY = particles.Get(Field.PrevY, particleId);
                var prevCell = grid.FindCell(prevX, prevY);


                if (cellId == prevCell)
                {
                    AddCurrentLinkageToCell(cellId, current, x, y, prevX, prevY);
                }
                else
                {
                    //http://stackoverflow.com/questions/3788605/if-debug-vs-conditionaldebug глянуть
#if DEBUG
                    var controlLength = Math.Sqrt(Math.Pow(x - prevX, 2) + Math.Pow(y - prevY, 2));
                    var length = 0.0;
#endif
                    while (true)
                    {
                        var cell = grid.GetCell(cellId);
                        var cellX = cell[0];
                        var cellY = cell[1];
                        var dx = prevX - x;
                        var dy = prevY - y;
                        var intersectX = dx > 0 ? cellX + grid.Hx : cellX;
                        var intersectY = dy > 0 ? cellY + grid.Hy : cellY;

                        int newCell;
                        if (Math.Abs(dx / (intersectX - x)) >= Math.Abs(dy / (intersectY - y)))
                        {
                            intersectY = y + (dy / dx) * (intersectX - x);
                            newCell = grid.FindCell(intersectX + Math.Sign(dx) * 0.5 * grid.Hx, intersectY);
                        }
                        else
                        {
                            intersectX = x + (dx / dy) * (intersectY - y);
                            newCell = grid.FindCell(intersectX, intersectY + Math.Sign(dy) * 0.5 * grid.Hy);
                        }
#if DEBUG
                        length += Math.Sqrt(Math.Pow(x - intersectX, 2) + Math.Pow(y - intersectY, 2));
#endif
                        AddCurrentLinkageToCell(cellId, current, x, y, intersectX, intersectY);
                        cellId = newCell;
                        x = intersectX;
                        y = intersectY;
                        if (cellId == prevCell)
                        {
#if DEBUG
                            length += Math.Sqrt(Math.Pow(prevX - x, 2) + Math.Pow(prevY - y, 2));
                             Debug.Assert(Math.Abs(length - controlLength) < double.Epsilon, "Interpolation error");
#endif

                            AddCurrentLinkageToCell(cellId, current, x, y, prevX, prevY);
                            break;
                        }
                    }

                }


            }
        }

        private void AddCurrentLinkageToCell(int cellId, double current, double x1, double y1, double x2, double y2)
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
