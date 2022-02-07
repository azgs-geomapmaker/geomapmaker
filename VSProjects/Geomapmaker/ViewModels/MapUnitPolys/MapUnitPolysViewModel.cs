using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.MapUnitPolys
{
    public class MapUnitPolysViewModel : ProWindow, INotifyPropertyChanged
    {
        public ICommand CommandCancel => new RelayCommand((proWindow) =>
        {
            if (proWindow != null)
            {
                (proWindow as ProWindow).Close();
            }

        }, () => true);

        public ICommand CommandSave => new RelayCommand(() => SubmitAsync(), () => CanSave());

        private bool CanSave()
        {
            return Selected != null;
        }

        private void SubmitAsync()
        {
            //throw new NotImplementedException();
        }

        public MapUnitPolysViewModel()
        {

        }

        private MapUnit _selected;
        public MapUnit Selected
        {
            get => _selected;
            set
            {
                _selected = value;
            }
        }

        private List<MapUnit> _mapUnits { get; set; }
        public List<MapUnit> MapUnits
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
            List<MapUnit> mapUnitList = await Data.DescriptionOfMapUnits.GetMapUnitsAsync();

            MapUnits = mapUnitList.Where(a => a.ParagraphStyle == "Standard").OrderBy(a => a.Name).ToList();
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
                Owner = FrameworkApplication.Current.MainWindow
            };

            _mapunitpolys.mapUnitPolysVM.RefreshMapUnitsAsync();

            _mapunitpolys.Closed += (o, e) => { _mapunitpolys = null; };
            _mapunitpolys.Show();
        }

    }
}
