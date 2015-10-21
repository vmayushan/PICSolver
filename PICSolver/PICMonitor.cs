using PICSolver.Abstract;
using PICSolver.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICSolver
{
    public class PICMonitor
    {
        private IRectangleGrid _grid;
        private IParticleStorage<Particle> _particles;
        public PICMonitor(IRectangleGrid grid, IParticleStorage<Particle> particles)
        {
            this._grid = grid;
            this._particles = particles;
        }

        public void StepStart()
        {

        }
        public void StepEnd ()
        {

        }
    }
}
