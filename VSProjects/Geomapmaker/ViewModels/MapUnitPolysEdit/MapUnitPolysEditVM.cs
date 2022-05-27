using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.MapUnitPolysEdit
{
    public class MapUnitPolysEditVM : ProWindow, INotifyPropertyChanged
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

    internal class ShowMapUnitPolysEdit : Button
    {
        private Views.MapUnitPolysEdit.MapUnitPolysEdit _mapunitpolysedit = null;

        protected override void OnClick()
        {
            //already open?
            if (_mapunitpolysedit != null)
            {
                _mapunitpolysedit.Close();
                return;
            }

            _mapunitpolysedit = new Views.MapUnitPolysEdit.MapUnitPolysEdit
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            _mapunitpolysedit.Closed += (o, e) =>
            {
                // Reset the map tool to explore
                FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");

                _mapunitpolysedit = null;
            };

            _mapunitpolysedit.mapUnitPolysEditVM.WindowCloseEvent += (s, e) => _mapunitpolysedit.Close();

            _mapunitpolysedit.Show();
        }

    }
}
