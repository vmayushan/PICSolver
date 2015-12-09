using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using OxyPlot.Wpf;
using PICSolver;
using PICSolver.Abstract;
using PICSolver.Monitor;
using SimulationTool.Commands;
using SimulationTool.Helpers;
using SimulationTool.View;
using LineSeries = OxyPlot.Series.LineSeries;
using PlotType = PICSolver.Monitor.PlotType;

namespace SimulationTool.ViewModel
{
    //при нажатии на создание новой вкладки 
    //открывается окно с параметрами
    //кол-во графиков (может быть)
    //типы графиков
    //цветовая схема
    public class MainViewModel : ViewModelBase
    {

        private readonly IDialogCoordinator _dialogCoordinator;
        public List<AccentColorMenuData> AccentColors { get; set; }
        public List<AppThemeMenuData> AppThemes { get; set; }
        private PICProject Project { get; set; }
        private IPICSolver Solver { get; set; }
        private CancellationTokenSource TokenSource { get; set; }
        #region Constructor

        public MainViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;
            Workspaces = new ObservableCollection<WorkspaceViewModel>();
            Workspaces.CollectionChanged += Workspaces_CollectionChanged;

            // create accent color menu items for the demo
            this.AccentColors = ThemeManager.Accents
                                            .Select(a => new AccentColorMenuData() { Name = a.Name, ColorBrush = a.Resources["AccentColorBrush"] as Brush, Model = this})
                                            .ToList();

            // create metro theme color menu items for the demo
            this.AppThemes = ThemeManager.AppThemes
                                           .Select(a => new AppThemeMenuData() { Name = a.Name, BorderColorBrush = a.Resources["BlackColorBrush"] as Brush, ColorBrush = a.Resources["WhiteColorBrush"] as Brush, Model = this })
                                           .ToList();
        }

        private void LoadProject()
        {
            Project = new PICProject()
            {
                Backscattering = true,
                BackscatteringAlfa = 0.5,
                BackscatteringBeta = 0.9,
                GridM = 101,
                GridN = 101,
                Method = PICMethod.Iterative,
                ParticlesCount = 100,
                Step = 1E-12,
                Voltage = 100000,
                Height = 0.5,
                Length = 0.1,
                Relaxation = 0.9,
                EmitterTop = 0.3,
                EmitterBottom = 0.2
            };
            Solver = (Project.Method == PICMethod.ParticleInCell) ? (IPICSolver)new PICSolver2D() : new IterativePICSolver2D();
            Solver.Prepare(Project);
        }

        private void Run()
        {
            TokenSource?.Dispose();
            TokenSource = new CancellationTokenSource();
            Task.Run(() =>
            {
                if (Project == null) LoadProject();
                Solver.Monitor.Status = PICStatus.Computing;
                for (var i = 0; i < int.MaxValue; i++)
                {
                    if (TokenSource.IsCancellationRequested) TokenSource.Token.ThrowIfCancellationRequested();
                    Epsilon = Solver.Step();
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => { UpdatePlots(Solver.Monitor); }));
                }
            }, TokenSource.Token).ContinueWith((prevTask) => { Solver.Monitor.Status = PICStatus.Pause; });
        }

        private void Step()
        {
            Task.Run(() =>
            {
                if (Project == null) LoadProject();
                Solver.Monitor.Status = PICStatus.Computing;
                Epsilon = Solver.Step();
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render,
                    new Action(() => { UpdatePlots(Solver.Monitor); }));
            }).ContinueWith((prevTask) => { Solver.Monitor.Status = PICStatus.Pause; });

        }

        private void Abort()
        {
            TokenSource.Cancel();
        }


        #endregion

        #region Event Handlers

        void Workspaces_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
                foreach (WorkspaceViewModel workspace in e.NewItems)
                    workspace.RequestClose += this.OnWorkspaceRequestClose;

            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (WorkspaceViewModel workspace in e.OldItems)
                    workspace.RequestClose -= this.OnWorkspaceRequestClose;
        }

        private void OnWorkspaceRequestClose(object sender, EventArgs e)
        {
            CloseWorkspace();
        }

        #endregion

        #region Commands

        private DelegateCommand _exitCommand;
        public ICommand ExitCommand => _exitCommand ?? (_exitCommand = new DelegateCommand(() => Application.Current.Shutdown()));

        private DelegateCommand _runCommand;
        public ICommand RunCommand => _runCommand ?? (_runCommand = new DelegateCommand(this.Run, () => Solver?.Monitor?.Status != PICStatus.Computing));

        private DelegateCommand _abortCommand;
        public ICommand AbortCommand => _abortCommand ?? (_abortCommand = new DelegateCommand(this.Abort, () => Solver?.Monitor?.Status == PICStatus.Computing));

        private DelegateCommand _stepCommand;
        public ICommand StepCommand => _stepCommand ?? (_stepCommand = new DelegateCommand(this.Step, () => Solver?.Monitor?.Status != PICStatus.Computing));

        private DelegateCommand _newWorkspaceCommand;
        public ICommand NewWorkspaceCommand => _newWorkspaceCommand ?? (_newWorkspaceCommand = new DelegateCommand(this.RunCustomFromVm));



        private DelegateCommand _projectPropertiesCommand;
        public ICommand ProjectPropertiesCommand => _projectPropertiesCommand ?? (_projectPropertiesCommand = new DelegateCommand(NewProjectWorkspace, () => Workspaces == null || !Workspaces.Any(x => x is PreferencesViewModel)));

        private void NewProjectWorkspace()
        {
            var workspace = new PreferencesViewModel() { Header = "Properties" };
            Workspaces.Add(workspace);
            SelectedIndex = Workspaces.IndexOf(workspace);
        }

        private DelegateCommand _closeWorkspaceCommand;
        public ICommand CloseWorkspaceCommand => _closeWorkspaceCommand ?? (_closeWorkspaceCommand = new DelegateCommand(CloseWorkspace, () => Workspaces?.Count > 0));

        private void CloseWorkspace()
        {
            Workspaces.RemoveAt(SelectedIndex);
            SelectedIndex = 0;
        }


        private DelegateCommand _onGitHub;
        public ICommand OnGitHub => _onGitHub ?? (_onGitHub = new DelegateCommand(LaunchOnGitHub));

        private void LaunchOnGitHub()
        {
            System.Diagnostics.Process.Start("https://github.com/vlad294/PICSolver");
        }

        private async void RunCustomFromVm()
        {
            var customDialog = new CustomDialog() { Title = "New plot" };

            var addPlotDialogViewModel = new AddPlotDialogViewModel(instance => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }) { MainViewModel = this };
            customDialog.Content = new AddPlot { DataContext = addPlotDialogViewModel };
            await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
        }
        #endregion

        #region Public Members

        public ObservableCollection<WorkspaceViewModel> Workspaces { get; set; }

        private int _selectedIndex = 0;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                OnPropertyChanged("SelectedIndex");
            }
        }

        private double _epsilon = 0;
        public double Epsilon
        {
            get { return _epsilon; }
            set
            {
                _epsilon = value;
                OnPropertyChanged("Epsilon");
            }
        }

        #endregion

        public void AddPlot(PICPlot plot)
        {
            string header;
            switch (plot.PlotType)
            {
                case PlotType.Line:
                    header = $"{plot.PlotSource.Description()} {plot.PlotType.Description()}";
                    break;
                case PlotType.Trajectories:
                    header = plot.PlotType.Description();
                    break;
                default:
                    header = plot.PlotSource.Description();
                    break;
            }
            var workspace = new PlotViewModel(plot) { Header = header };
            Workspaces.Add(workspace);
            SelectedIndex = Workspaces.IndexOf(workspace);
        }

        public void UpdatePlots(PICMonitor monitor)
        {
            Task.Run(() =>
            {
                foreach (var model in Workspaces.Where(p => p.GetType() == typeof(PlotViewModel)).Cast<PlotViewModel>())
                {
                    switch (model.PICPlot.PlotType)
                    {
                        case PlotType.Heatmap:
                            switch (model.PICPlot.PlotSource)
                            {
                                case PlotSource.Density:
                                    model.HeatMapSeries.Data = monitor.Rho;
                                    break;
                                case PlotSource.Potential:
                                    model.HeatMapSeries.Data = monitor.Potential;
                                    break;
                                case PlotSource.ElectricFieldX:
                                    model.HeatMapSeries.Data = monitor.Ex;
                                    break;
                                case PlotSource.ElectricFieldY:
                                    model.HeatMapSeries.Data = monitor.Ey;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            model.HeatMapSeries.Invalidate();
                            model.PlotModel.InvalidatePlot(true);
                            break;
                        case PlotType.Line:
                            var lineSeries = new LineSeries { MarkerType = MarkerType.Circle };
                            var data = monitor.GetLine(model.PICPlot.PlotSource, model.PICPlot.LinePlotAlignment, model.PICPlot.LinePlotСoordinate);
                            for (var i = 0; i < data.Length; i++) lineSeries.Points.Add(new DataPoint(monitor.GridX[i], data[i]));
                            model.PlotModel.Series.Clear();
                            model.PlotModel.Series.Add(lineSeries);
                            model.PlotModel.InvalidatePlot(true);
                            break;
                        case PlotType.Trajectories:

                            model.PlotModel.Series.Clear();
                            foreach (var trajectory in monitor.Trajectories.OrderBy(x => x.First().Item3).Where((c, i) => i % 5 == 0))
                            {
                                var series = new LineSeries { MarkerType = MarkerType.None, LineStyle = LineStyle.Solid, Color = OxyColors.Black };
                                foreach (var point in trajectory) series.Points.Add(new DataPoint(point.Item2, point.Item3));
                                model.PlotModel.Series.Add(series);
                            }
                            model.PlotModel.InvalidatePlot(true);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            });
        }

        public void UpdateTheme()
        {
            foreach (var model in Workspaces.Where(p => p.GetType() == typeof (PlotViewModel)).Cast<PlotViewModel>())
            {
                var backgroundBrush = (Brush)ThemeManager.DetectAppStyle(Application.Current).Item1.Resources["WindowBackgroundBrush"];
                model.PlotModel.Background = backgroundBrush.ToOxyColor();
                model.PlotModel.InvalidatePlot(true);
            }
        }
    }

    public class AccentColorMenuData
    {
        public string Name { get; set; }
        public Brush BorderColorBrush { get; set; }
        public Brush ColorBrush { get; set; }
        public MainViewModel Model { get; set; }

        private ICommand changeAccentCommand;

        public ICommand ChangeAccentCommand => changeAccentCommand ?? (changeAccentCommand = new DelegateCommand(this.DoChangeTheme));

        protected virtual void DoChangeTheme()
        {
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            var accent = ThemeManager.GetAccent(this.Name);
            ThemeManager.ChangeAppStyle(Application.Current, accent, theme.Item1);
            Model.UpdateTheme();
        }
    }

    public class AppThemeMenuData : AccentColorMenuData
    {
        protected override void DoChangeTheme()
        {
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            var appTheme = ThemeManager.GetAppTheme(this.Name);
            ThemeManager.ChangeAppStyle(Application.Current, theme.Item2, appTheme);
            Model.UpdateTheme();
        }
    }
}
