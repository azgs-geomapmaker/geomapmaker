using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.MapUnitPolysCreate
{
    public class MapUnitPolysCreateVM : ProWindow, INotifyPropertyChanged
    {
        public event EventHandler WindowCloseEvent;

        public ICommand CommandCancel => new RelayCommand(() => CloseProwindow());

        public ICommand CommandRefreshDMU => new RelayCommand(() => RefreshDMU());

        public void CloseProwindow()
        {
            WindowCloseEvent(this, new EventArgs());
        }

        private List<MapUnitPolyTemplate> _mapUnits { get; set; }
        public List<MapUnitPolyTemplate> MapUnits
        {
            get => _mapUnits;
            set
            {
                _mapUnits = value;
                NotifyPropertyChanged();
            }
        }

        public async void RefreshDMU()
        {
            Data.MapUnitPolys.RebuildMUPSymbologyAndTemplates();
            MapUnits = await Data.MapUnitPolys.GetMapUnitPolyTemplatesAsync();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    internal class ShowMapUnitPolysCreate : Button
    {
        private Views.MapUnitPolysCreate.MapUnitPolysCreate _mapunitpolyscreate = null;

        protected override void OnClick()
        {
            //already open?
            if (_mapunitpolyscreate != null)
            {
                _mapunitpolyscreate.Close();
                return;
            }

            _mapunitpolyscreate = new Views.MapUnitPolysCreate.MapUnitPolysCreate
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            _mapunitpolyscreate.Closed += (o, e) =>
            {
                // Reset the map tool to explore
                FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");

                _mapunitpolyscreate = null;
            };

            _mapunitpolyscreate.mapUnitPolysCreateVM.WindowCloseEvent += (s, e) => _mapunitpolyscreate.Close();

            _mapunitpolyscreate.Show();
        }
    }
}
