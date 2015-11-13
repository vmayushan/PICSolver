using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
                var current = particles.Get(Field.Q, particleId)  / grid.CellSquare;

                var x = particles.Get(Field.X, particleId);
                var y = particles.Get(Field.Y, particleId);
                var cellId = grid.FindCell(x, y);

                var prevX = particles.Get(Field.PrevX, particleId);
                var prevY = particles.Get(Field.PrevY, particleId);
                var prevCell = grid.FindCell(prevX, prevY);

                var diff = cellId - prevCell;
                double x2, y2;
                switch (diff)
                {
                    case 0:
                        AddCurrentLinkageToCell(cellId, current, x, y, prevX, prevY);
                        break;
                    case -1: //prevcell - right
                        x2 = grid.GetCell(prevCell)[0];
                        y2 = LineY(prevX, x, prevY, y, x2);
                        AddCurrentLinkageToCell(cellId, current, x, y, x2, y2);
                        AddCurrentLinkageToCell(prevCell, current, x2, y2, prevX, prevY);
                        break;
                    case 1: //prevcell - left
                        x2 = grid.GetCell(cellId)[0];
                        y2 = LineY(prevX, x, prevY, y, x2);
                        AddCurrentLinkageToCell(prevCell, current, prevX, prevY, x2, y2);
                        AddCurrentLinkageToCell(cellId, current, x2, y2, x, y);
                        break;
                    default:
                        if (grid.UpperCell(cellId) == prevCell)
                        {
                            y2 = grid.GetCell(prevCell)[1];
                            x2 = LineX(prevX, x, prevY, y, y2);

                            AddCurrentLinkageToCell(cellId, current, x, y, x2, y2);
                            AddCurrentLinkageToCell(prevCell, current, x2, y2, prevX, prevY);
                        }
                        else if (grid.UpperCell(prevCell) == cellId)
                        {
                            y2 = grid.GetCell(cellId)[1];
                            x2 = LineX(prevX, x, prevY, y, y2);

                            AddCurrentLinkageToCell(prevCell, current, prevX, prevY, x2, y2);
                            AddCurrentLinkageToCell(cellId, current, x2, y2, x, y);
                            
                        }
                        else
                        {
                            AddCurrentLinkageToCell(cellId, current, x, y, x, y);
                        }
                        break;
                }

                //var list = new List<Point>();
                //list.Add(new Point(x,y));
                //list.Add
                //var intersect = new double[] { minX, maxX }.
                //    Concat(grid.X.Where(p => p > minX && p < maxX)).
                //    Concat(grid.Y.Where(p => p > Math.Min(y, prevY) && p < Math.Max(y, prevY)).
                //    Select(p => LineX(prevX, x, prevY, y, p))).OrderBy(p => p).ToArray();
                //for (int i = 0; i < intersect.Length - 1; i++)
                //{
                //    var yy = LineY(prevX, x, prevY, y, intersect[i]);
                //    Console.WriteLine(yy);
                //    Console.WriteLine(grid.FindCell(intersect[i], yy));
                //}

                //var xx = LineX(prevX, x, prevY, y, y: 1);

                //Console.WriteLine(xx);

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

        private static double LineY(double x1, double x2, double y1, double y2, double x)
        {
            return ((x - x2) * y1 + (x1 - x) * y2) / (x1 - x2);
        }

        private static double LineX(double x1, double x2, double y1, double y2, double y)
        {
            return ((y1 - y) * x2 + (y - y2) * x1) / (y1 - y2);
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
