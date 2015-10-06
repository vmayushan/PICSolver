using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICSolver.Abstract
{
    public interface IParticle
    {
        double X { get; set; }
        double Y { get; set; }
        double Px { get; set; }
        double Py { get; set; }
        double Q { get; set; }
    }
}
