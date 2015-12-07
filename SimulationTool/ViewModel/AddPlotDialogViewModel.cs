using System;
using System.Collections.Generic;
using System.Windows.Input;
using PICSolver.Monitor;
using SimulationTool.Commands;
using SimulationTool.Helpers;
using SimulationTool.ViewModel;

namespace SimulationTool
{
    public class AddPlotDialogViewModel : ViewModelBase
    {
        private int _linePlot;

        public AddPlotDialogViewModel(Action<AddPlotDialogViewModel> closeHandler)
        {
            CloseCommand = new DelegateCommand(() => closeHandler(this));
        }

        public MainViewModel MainViewModel { get; set; }

        public int PlotLine
        {
            get { return _linePlot; }
            set
            {
                _linePlot = value;
                OnPropertyChanged(nameof(PlotLine));
            }
        }

        public IEnumerable<ValueDescription> PlotSourceList => EnumHelper.GetAllValuesAndDescriptions<PlotSource>();
        private PlotSource source;
        public PlotSource PlotSource
        {
            get
            {
                return source;
            }
            set
            {
                if (source == value) return;
                source = value;
                OnPropertyChanged(nameof(PlotSource));
            }
        }

        public IEnumerable<ValueDescription> PlotTypeList => EnumHelper.GetAllValuesAndDescriptions<PlotType>();
        private PlotType type;
        public PlotType PlotType
        {
            get
            {
                return type;
            }
            set
            {
                if (type == value) return;
                type = value;
                OnPropertyChanged(nameof(PlotType));
            }
        }

        public IEnumerable<ValueDescription> LinePlotAlignmentList => EnumHelper.GetAllValuesAndDescriptions<LinePlotAlignment>();
        private LinePlotAlignment plotAlignment;
        public LinePlotAlignment LinePlotAlignment
        {
            get
            {
                return plotAlignment;
            }
            set
            {
                if (plotAlignment == value) return;
                plotAlignment = value;
                OnPropertyChanged(nameof(LinePlotAlignment));
            }
        }

        public ICommand CloseCommand { get; }

        private DelegateCommand _saveCommand;
        
        public ICommand SaveCommand => _saveCommand ?? (_saveCommand = new DelegateCommand(() => { MainViewModel.AddPlot(new PICPlot { PlotSource = PlotSource, PlotType = PlotType, LinePlotAlignment = LinePlotAlignment, LinePlotСoordinate = PlotLine }); if (CloseCommand.CanExecute(this)) CloseCommand.Execute(this); }));

    }
}
