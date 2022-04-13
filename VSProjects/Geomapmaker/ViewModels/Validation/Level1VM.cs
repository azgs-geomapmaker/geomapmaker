using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Geomapmaker.ViewModels.Validation
{
    public class Level1VM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ValidationViewModel ParentVM { get; set; }

        public Level1VM(ValidationViewModel parentVM)
        {
            ParentVM = parentVM;
            Check1 = "Skipped";
            Check2 = "Skipped";
            Check3 = "Skipped";
        }

        private string _check1 = "Checking..";
        public string Check1
        {
            get => _check1;
            set => SetProperty(ref _check1, value, () => Check1);
        }

        private string _check2 = "Checking..";
        public string Check2
        {
            get => _check2;
            set => SetProperty(ref _check2, value, () => Check2);
        }

        private string _check3 = "Checking..";
        public string Check3
        {
            get => _check3;
            set => SetProperty(ref _check3, value, () => Check3);
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
