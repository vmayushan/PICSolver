using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICSolver
{
    public class PICProject
    {
        public PICMethod Method { get; set; }
        public bool Backscattering { get; set; }
        public double BackscatteringAlfa { get; set; }
        public double BackscatteringBeta { get; set; }
        public int ParticlesCount { get; set; }
        public double Step { get; set; }
        public int GridN { get; set; }
        public int GridM { get; set; }
        public int Voltage { get; set; }
        public double Length { get; set; }
        public double Height { get; set; }
        public double EmitterBottom { get; set; }
        public double EmitterTop { get; set; }
        public double Relaxation { get; set; }

    }

    public enum PICMethod
    {
        ParticleInCell,
        Iterative
    }
}
