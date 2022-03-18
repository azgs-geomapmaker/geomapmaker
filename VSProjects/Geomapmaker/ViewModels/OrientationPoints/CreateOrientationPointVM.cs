using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using Geomapmaker.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.OrientationPoints
{
    public class CreateOrientationPointVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ICommand CommandSave => new RelayCommand(() => SaveAsync(), () => CanSave());

        private bool CanSave()
        {
            return !HasErrors;
        }

        private void SaveAsync()
        {
            throw new NotImplementedException();
        }

        public OrientationPointsViewModel ParentVM { get; set; }

        public CreateOrientationPointVM(OrientationPointsViewModel parentVM)
        {
            ParentVM = parentVM;
            Type = "";
            PlotAtScale = "0";
            LocationConfidenceMeters = "";
        }

        private string _selectedStation;
        public string SelectedStation
        {
            get => _selectedStation;
            set
            {
                SetProperty(ref _selectedStation, value, () => SelectedStation);
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

        private string _type;
        public string Type
        {
            get => _type;
            set
            {
                SetProperty(ref _type, value, () => Type);
                ValidateRequiredString(Type, "Type");
            }
        }

        private string _plotAtScale;
        public string PlotAtScale
        {
            get => _plotAtScale;
            set
            {
                SetProperty(ref _plotAtScale, value, () => PlotAtScale);
                ValidatePlotAtScale(PlotAtScale, "PlotAtScale");
            }
        }
        // Holds the converted int value 
        private int PlotAtScaleInt;

        private string _locationConfidenceMeters;
        public string LocationConfidenceMeters
        {
            get => _locationConfidenceMeters;
            set
            {
                SetProperty(ref _locationConfidenceMeters, value, () => LocationConfidenceMeters);
                ValidateRequiredString(LocationConfidenceMeters, "LocationConfidenceMeters");
            }
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

        private void ValidatePlotAtScale(string text, string propertyKey)
        {
            // Required field
            if (string.IsNullOrEmpty(text))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            else if (!int.TryParse(text, out PlotAtScaleInt))
            {
                _validationErrors[propertyKey] = new List<string>() { "Scale must be a postive integer." };
            }
            else if (PlotAtScaleInt < 0)
            {
                _validationErrors[propertyKey] = new List<string>() { "Scale must be a postive integer." };
            }
            else
            {
                _validationErrors.Remove(propertyKey);
            }

            RaiseErrorsChanged(propertyKey);
        }

        #endregion
    }
}
