using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.MapUnitPolys
{
    public class MapUnitPolysViewModel : ProWindow, INotifyPropertyChanged
    {
        public event EventHandler WindowCloseEvent;

        public ICommand CommandCancel => new RelayCommand(() => CloseProwindow());

        public void CloseProwindow()
        {
            WindowCloseEvent(this, new EventArgs());
        }

        public CreateMapUnitPolysVM Create { get; set; }
        public EditMapUnitPolysVM Edit { get; set; }
        public DeleteMapUnitPolysVM Delete { get; set; }

        public MapUnitPolysViewModel()
        {
            Create = new CreateMapUnitPolysVM(this);
            Edit = new EditMapUnitPolysVM(this);
            Delete = new DeleteMapUnitPolysVM(this);
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

        public async void RefreshMapUnitsAsync()
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

    internal class ShowMapUnitPolys : Button
    {
        private Views.MapUnitPolys.MapUnitPolys _mapunitpolys = null;

        protected override void OnClick()
        {
            //already open?
            if (_mapunitpolys != null)
            {
                _mapunitpolys.Close();
                return;
            }

            _mapunitpolys = new Views.MapUnitPolys.MapUnitPolys
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            _mapunitpolys.mapUnitPolysVM.RefreshMapUnitsAsync();

            _mapunitpolys.Closed += (o, e) =>
            {
                // Reset the map tool to explore
                FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");

                _mapunitpolys = null;
            };

            _mapunitpolys.mapUnitPolysVM.WindowCloseEvent += (s, e) => _mapunitpolys.Close();

            _mapunitpolys.Show();
        }

    }
}