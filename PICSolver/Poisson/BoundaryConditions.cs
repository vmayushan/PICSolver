using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICSolver.Poisson
{
    public enum BoundaryConditionType
    {
        Dirichlet,
        Neumann
    }
    public struct BoundaryCondition
    {
        public BoundaryConditionType Type { get; set; }
        public Func<double, double> Value { get; set; }

    }
    public class BoundaryConditions
    {

        public BoundaryConditions()
        {
            Top = new BoundaryCondition() { Value = (x) => 0 };
            Bottom = new BoundaryCondition() { Value = (x) => 0 };
            Left = new BoundaryCondition() { Value = (x) => 0 };
            Right = new BoundaryCondition() { Value = (x) => 0 };
        }
        public BoundaryCondition Top { get; set; }
        public BoundaryCondition Bottom { get; set; }
        public BoundaryCondition Left { get; set; }
        public BoundaryCondition Right { get; set; }

    }
}
