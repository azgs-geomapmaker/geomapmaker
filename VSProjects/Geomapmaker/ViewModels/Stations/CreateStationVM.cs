using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.Stations
{
    public class CreateStationVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ICommand CommandSave => new RelayCommand(() => SaveAsync(), () => CanSave());

        private string _type;
        public string Type
        {
            get => _type;
            set
            {
                SetProperty(ref _type, value, () => Type);
            }
        }

        private string _xCoordinate;
        public string XCoordinate
        {
            get => _xCoordinate;
            set
            {
                SetProperty(ref _xCoordinate, value, () => XCoordinate);
            }
        }

        private string _yCoordinate;
        public string YCoordinate
        {
            get => _yCoordinate;
            set
            {
                SetProperty(ref _yCoordinate, value, () => YCoordinate);
            }
        }

        private string _locationConfidenceMeters;
        public string LocationConfidenceMeters
        {
            get => _locationConfidenceMeters;
            set
            {
                SetProperty(ref _locationConfidenceMeters, value, () => LocationConfidenceMeters);
            }
        }

        private string _plotAtScale;
        public string PlotAtScale
        {
            get => _plotAtScale;
            set
            {
                SetProperty(ref _plotAtScale, value, () => PlotAtScale);
            }
        }

        private string _notes;
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value, () => Notes);
        }

        private bool CanSave()
        {
            return true;
        }

        private void SaveAsync()
        {
            //throw new NotImplementedException();
        }

        public StationsViewModel ParentVM { get; set; }

        public CreateStationVM(StationsViewModel parentVM)
        {
            ParentVM = parentVM;
        }

        #region Validation

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private readonly Dictionary<string, ICollection<string>> _validationErrors = new Dictionary<string, ICollection<string>>();

        public bool HasErrors => _validationErrors.Count > 0;

        private void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public IEnumerable GetErrors(string propertyName)
        {
            // Return null if parameters is null/empty OR there are no errors for that parameter
            // Otherwise, return the errors for that parameter.
            return string.IsNullOrEmpty(propertyName) || !_validationErrors.ContainsKey(propertyName) ?
                null : (IEnumerable)_validationErrors[propertyName];
        }

        #endregion
    }
}
