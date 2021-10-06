using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.MapUnits
{
    public class DeleteMapUnitVM : DockPane, INotifyDataErrorInfo
    {
        // Deletes's save button
        public ICommand CommandDelete { get; }
        public ICommand CommandReset { get; }

        public DeleteMapUnitVM()
        {
            // Init commands
            CommandDelete = new RelayCommand(() => DeleteMapUnitAsync(), () => CanDelete());
            CommandReset = new RelayCommand(() => ResetAsync());
        }

        private bool CanDelete()
        {
            return !HasErrors;
        }

        private void ResetAsync()
        {
            // TODO
            throw new NotImplementedException();
        }

        private void DeleteMapUnitAsync()
        {
            // TODO
            throw new NotImplementedException();
        }

        // Validation
        #region INotifyDataErrorInfo members

        // Error collection
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
