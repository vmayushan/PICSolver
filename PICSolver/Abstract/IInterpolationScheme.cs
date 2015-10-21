namespace PICSolver.Abstract
{
    public interface IInterpolationScheme
    {
        void InterpolateToGrid();
        void InterpolateForces();
    }
}
