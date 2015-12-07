using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PICSolver.Monitor;

namespace PICSolver.Abstract
{
    // ReSharper disable once InconsistentNaming
    public interface IPICSolver
    {
        void Prepare(PICProject project);
        double Step();
        PICMonitor Monitor { get; set; }
        List<Tuple<int, double, double>> Trajectories { get; set; }
    }
}
