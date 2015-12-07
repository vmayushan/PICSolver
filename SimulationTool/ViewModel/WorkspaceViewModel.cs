using System;
using System.Windows.Input;
using OxyPlot;
using OxyPlot.Series;
using SimulationTool.Commands;

namespace SimulationTool.ViewModel
{
    public abstract class WorkspaceViewModel : ViewModelBase
    {
        private string _header;
        public string Header
        {
            get { return _header; }
            set
            {
                _header = value;
                OnPropertyChanged("Header");
            }
        }

        private DelegateCommand _closeCommand;
        public ICommand CloseCommand
        {
            get { return _closeCommand ?? (_closeCommand = new DelegateCommand(OnRequestClose)); }
        }

        public event EventHandler RequestClose;
        private void OnRequestClose()
        {
            if (RequestClose != null)
                RequestClose(this, EventArgs.Empty);
        }
    }
}
