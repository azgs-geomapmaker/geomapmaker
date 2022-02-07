using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using Geomapmaker.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.MapUnitPolys
{
    public class MapUnitPolysViewModel : ProWindow, INotifyPropertyChanged, INotifyDataErrorInfo
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
            return Selected != null && HasErrors == false;
        }

        private void SubmitAsync()
        {
            //throw new NotImplementedException();
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

        private MapUnit _selected;
        public MapUnit Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                NotifyPropertyChanged("FieldVisibility");

                // Trigger validation
                IdentityConfidence = null;
            }
        }

        public string FieldVisibility => Selected == null ? "Hidden" : "Visible";

        private string _identityConfidence;
        public string IdentityConfidence
        {
            get => _identityConfidence;
            set
            {
                _identityConfidence = value;
                ValidateRequiredString(IdentityConfidence, "IdentityConfidence");
                NotifyPropertyChanged();
            }
        }

        private string _notes;
        public string Notes
        {
            get => _notes;
            set
            {
                _notes = value;
                NotifyPropertyChanged();
            }
        }

        private string _dataSource = GeomapmakerModule.DataSourceId;
        public string DataSource
        {
            get => _dataSource;
            set
            {
                _notes = value;
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

        #region Validation

        private readonly Dictionary<string, ICollection<string>> _validationErrors = new Dictionary<string, ICollection<string>>();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public IEnumerable GetErrors(string propertyName)
        {
            // Return null if parameters is null/empty OR there are no errors for that parameter
            // Otherwise, return the errors for that parameter.
            return string.IsNullOrEmpty(propertyName) || !_validationErrors.ContainsKey(propertyName) ?
                null : _validationErrors[propertyName];
        }

        public bool HasErrors => _validationErrors.Count > 0;

        private void ValidateRequiredString(string text, string propertyKey)
        {
            // Required field
            if (string.IsNullOrEmpty(text))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            else
            {
                _validationErrors.Remove(propertyKey);
            }

            RaiseErrorsChanged(propertyKey);
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
