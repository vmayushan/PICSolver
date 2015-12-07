namespace PICSolver.Abstract
{
    public interface IInterpolationScheme
    {
        void InterpolateDensity();
        void InterpolateForces();
    }
}
