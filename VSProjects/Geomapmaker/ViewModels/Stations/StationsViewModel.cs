using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Geomapmaker.Models;

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

        public CreateStationVM Create { get; set; }
        public EditStationVM Edit { get; set; }
        public DeleteStationVM Delete { get; set; }

        public StationsViewModel()
        {
            Create = new CreateStationVM(this);
            Edit = new EditStationVM(this);
            Delete = new DeleteStationVM(this);
        }

        private List<Station> _stationOptions { get; set; }
        public List<Station> StationOptions
        {
            get => _stationOptions;
            set
            {
                _stationOptions = value;
                NotifyPropertyChanged();
            }
        }

        public async void RefreshStationOptions()
        {
            StationOptions = await Data.Stations.GetStationsAsync();
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

        protected override async void OnClick()
        {
            //already open?
            if (_stations != null)
            {
                _stations.Close();
                return;
            }

            _stations = new Views.Stations.Stations
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            await QueuedTask.Run(() =>
            {
                _stations.stationsVM.RefreshStationOptions();
            });

            _stations.Closed += (o, e) => { _stations = null; };

            _stations.stationsVM.WindowCloseEvent += (s, e) => _stations.Close();

            _stations.Show();
        }
    }
}
