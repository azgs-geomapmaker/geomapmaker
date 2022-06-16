using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using Geomapmaker.Data;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.OrientationPoints
{
    public class OrientationPointsViewModel : ProWindow, INotifyPropertyChanged
    {
        public event EventHandler WindowCloseEvent;

        public ICommand CommandCancel => new RelayCommand(() => CloseProwindow());

        public void CloseProwindow()
        {
            WindowCloseEvent(this, new EventArgs());
        }

        public CreateOrientationPointVM Create { get; set; }

        public OrientationPointsViewModel()
        {
            Create = new CreateOrientationPointVM(this);
        }

        private List<GemsSymbol> _symbolOptions { get; set; }
        public List<GemsSymbol> SymbolOptions
        {
            get => _symbolOptions;
            set
            {
                _symbolOptions = value;
                NotifyPropertyChanged();
            }
        }

        private List<string> _stationFieldIdOptions { get; set; }
        public List<string> StationFieldIdOptions
        {
            get => _stationFieldIdOptions;
            set
            {
                _stationFieldIdOptions = value;
                NotifyPropertyChanged();
            }
        }

        private List<string> _dataSourceOptions { get; set; }
        public List<string> DataSourceOptions
        {
            get => _dataSourceOptions;
            set
            {
                _dataSourceOptions = value;
                NotifyPropertyChanged();
            }
        }

        public async void RefreshOptions()
        {
            // Get symbology options if the list is null
            if (Data.Symbology.OrientationPointSymbols == null)
            {
                await Data.Symbology.RefreshOPSymbolOptionsAsync();
            }

            // ParentVM keeps a copy of the master list
            SymbolOptions = Data.Symbology.OrientationPointSymbols;

            // Push options to create vm
            Create.SymbolOptions = SymbolOptions;
            Create.SymbolsFilteredMessage = $"{SymbolOptions.Count()} symbols";

            // Field ID Options
            StationFieldIdOptions = await AnyFeatureLayer.GetDistinctValuesForFieldAsync("Stations", "fieldid");

            // Data Source Options
            DataSourceOptions = await AnyStandaloneTable.GetDistinctValuesForFieldAsync("DataSources", "datasources_id");
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }

    internal class ShowOrientationPoints : Button
    {
        private Views.OrientationPoints.OrientationPoints _orientationpoints = null;

        protected override async void OnClick()
        {
            //already open?
            if (_orientationpoints != null)
            {
                _orientationpoints.Close();
                return;
            }

            _orientationpoints = new Views.OrientationPoints.OrientationPoints
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            // Symbology takes a few seconds to load. Display progress dialog
            ProgressorSource ps = new ProgressorSource("Generating symbology for Orientation Points");

            await QueuedTask.Run(() =>
            {
                _orientationpoints.orientationPointsViewModelVM.RefreshOptions();
            }, ps.Progressor);

            _orientationpoints.Closed += (o, e) => { _orientationpoints = null; };

            _orientationpoints.orientationPointsViewModelVM.WindowCloseEvent += (s, e) => _orientationpoints.Close();

            _orientationpoints.Show();
        }
    }

}
