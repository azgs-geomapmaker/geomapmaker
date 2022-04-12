using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Geomapmaker.ViewModels.Validation
{
    public class AZGSVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ValidationViewModel ParentVM { get; set; }

        public AZGSVM(ValidationViewModel parentVM)
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
