using System.ComponentModel;

namespace PICSolver.Monitor
{
    // ReSharper disable once InconsistentNaming
    public class PICPlot
    {
        public PlotSource PlotSource { get; set; }
        public PlotType PlotType { get; set; }
        public LinePlotAlignment LinePlotAlignment { get; set; }
        public int LinePlotСoordinate { get; set; }
    }

    public enum PlotSource
    {
        Density,
        Potential,
        [Description("Electric Field X")]
        ElectricFieldX,
        [Description("Electric Field Y")]
        ElectricFieldY
    }

    public enum PlotType
    {
        Heatmap,
        Line,
        Trajectories
    }

    public enum LinePlotAlignment
    {
        Horizontal,
        Vertical
    }

    public enum PICStatus
    {
        Created,
        Computing,
        Pause,
    }
}
