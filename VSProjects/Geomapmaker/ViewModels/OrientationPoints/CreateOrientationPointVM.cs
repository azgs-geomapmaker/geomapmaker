using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
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
        }

        private bool _stationPicker;
        public bool StationPicker
        {
            get => _stationPicker;
            set
            {
                SetProperty(ref _stationPicker, value, () => StationPicker);

                // if the toggle-btn is active
                if (value)
                {
                    // Active the station picker tool
                    //FrameworkApplication.SetCurrentToolAsync("Geomapmaker_PopulateCFTool");
                }
                else
                {
                    // Switch back to map explore tool
                    FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
                }
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

        #endregion
    }
}
