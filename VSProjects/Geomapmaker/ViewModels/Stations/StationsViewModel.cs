using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.Stations
{
    public class StationsViewModel : ProWindow, INotifyPropertyChanged
    {
        public event EventHandler WindowCloseEvent;

        public ICommand CommandCancel => new RelayCommand(() => CloseProwindow());

        public void CloseProwindow()
        {
            WindowCloseEvent(this, new EventArgs());
        }

        public CreateStationsVM Create { get; set; }

        public StationsViewModel()
        {
            Create = new CreateStationsVM(this);
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    internal class ShowStations : Button
    {
        private Views.Stations.Stations _stations = null;

        protected override void OnClick()
        {
            //already open?
            if (_stations != null)
            {
                _stations.Close();
                return;
            }

            _stations = new Views.Stations.Stations();
            _stations.Owner = FrameworkApplication.Current.MainWindow;
            _stations.Closed += (o, e) => { _stations = null; };

            _stations.stationsVM.WindowCloseEvent += (s, e) => _stations.Close();

            _stations.Show();

        }

    }
}
