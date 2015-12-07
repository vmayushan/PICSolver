using System;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PICSolver.Monitor;
using PlotType = PICSolver.Monitor.PlotType;

namespace SimulationTool.ViewModel
{
    class PlotViewModel : WorkspaceViewModel
    {
        public PlotViewModel(PICPlot plot)
        {
            this.PICPlot = plot;

            switch (plot.PlotType)
            {
                case PlotType.Heatmap:
                    PlotModel = new PlotModel();
                    PlotModel.Axes.Add(new LinearColorAxis
                    {
                        HighColor = OxyColors.Gray,
                        LowColor = OxyColors.Black,
                        Position = AxisPosition.Right
                    });
                    PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom });
                    PlotModel.Axes.Add(new LinearAxis());
                    HeatMapSeries = new HeatMapSeries
                    {
                        X1 = 0.1,
                        Y0 = 0,
                        Y1 = 0.1,
                        Interpolate = true
                    };
                    PlotModel.Series.Add(HeatMapSeries);
                    break;
                case PlotType.Line:
                    this.PlotModel = new PlotModel { };
                    PlotModel.InvalidatePlot(true);
                    OnPropertyChanged("PlotModel");
                    break;
                case PlotType.Trajectories:
                    this.PlotModel = new PlotModel { };
                    PlotModel.InvalidatePlot(true);
                    OnPropertyChanged("PlotModel");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public PlotModel PlotModel { get; private set; }

        public HeatMapSeries HeatMapSeries { get; private set; }

        public PICPlot PICPlot { get; set; }
    }
}
