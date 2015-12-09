using System;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Wpf;
using PICSolver.Monitor;
using HeatMapSeries = OxyPlot.Series.HeatMapSeries;
using LinearAxis = OxyPlot.Axes.LinearAxis;
using LinearColorAxis = OxyPlot.Axes.LinearColorAxis;
using PlotType = PICSolver.Monitor.PlotType;

namespace SimulationTool.ViewModel
{
    class PlotViewModel : WorkspaceViewModel
    {
        public PlotViewModel(PICPlot plot)
        {
            this.PICPlot = plot;
            this.PlotModel = new PlotModel { };
            var backgroundBrush = (Brush)ThemeManager.DetectAppStyle(Application.Current).Item1.Resources["WindowBackgroundBrush"];
            PlotModel.Background = backgroundBrush.ToOxyColor();

            switch (plot.PlotType)
            {
                case PlotType.Heatmap:
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

                    PlotModel.InvalidatePlot(true);
                    OnPropertyChanged("PlotModel");
                    break;
                case PlotType.Trajectories:
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
