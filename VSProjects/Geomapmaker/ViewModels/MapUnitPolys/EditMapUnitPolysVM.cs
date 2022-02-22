using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Geomapmaker.ViewModels.MapUnitPolys
{
    public class EditMapUnitPolysVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public MapUnitPolysViewModel ParentVM { get; set; }

        public EditMapUnitPolysVM(MapUnitPolysViewModel parentVM)
        {
            ParentVM = parentVM;
        }






        #region Validation

        private readonly Dictionary<string, ICollection<string>> _validationErrors = new Dictionary<string, ICollection<string>>();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            // Return null if parameters is null/empty OR there are no errors for that parameter
            // Otherwise, return the errors for that parameter.
            return string.IsNullOrEmpty(propertyName) || !_validationErrors.ContainsKey(propertyName) ?
                null : (System.Collections.IEnumerable)_validationErrors[propertyName];
        }

        public bool HasErrors => _validationErrors.Count > 0;

        #endregion
    }
}
