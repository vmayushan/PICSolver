namespace PICSolver.Abstract
{
    interface IEmitter
    {
        double[] GetParticlesToInject();
        double Length { get; }
        int N { get; }
    }
}
