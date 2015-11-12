namespace PICSolver.Abstract
{
    public interface IParticle
    {
        int Id { get; set; }
        double X { get; set; }
        double Y { get; set; }
        double Px { get; set; }
        double Py { get; set; }
        double Q { get; set; }
        double BetaX { get; }
        double BetaY { get; }
        double Ex { get; set; }
        double Ey { get; set; }
        double PrevX { get; set; }
        double PrevY { get; set; }
    }
}
