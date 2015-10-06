using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICSolver.Domain
{
    static class Constants
    {
        public const double LightVelocity = 299792458.0;
        public const double ElectronCharge = -1.602176565E-19;
        public const double ElectronMass = 9.10938291E-31;
        public const double VacuumPermittivity = 8.85418782E-12;
        /// <summary>
        /// ElectronCharge / (ElectronMass * LightVelocity^2)
        /// </summary>
        public const double Alfa = -1.9569512693314196E-6;
    }
}
