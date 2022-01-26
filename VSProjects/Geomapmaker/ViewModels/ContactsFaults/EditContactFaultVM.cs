using ArcGIS.Desktop.Editing.Templates;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.ContactsFaults
{
    public class EditContactFaultVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ContactsFaultsViewModel ParentVM { get; set; }

        public ICommand CommandUpdate => new RelayCommand(() => UpdateTemplate(), () => IsValid());

        private void UpdateTemplate()
        {
            // TODO
            throw new NotImplementedException();
        }

        private bool IsValid()
        {
            // TODO
            return true;
        }

        public EditContactFaultVM(ContactsFaultsViewModel parentVM)
        {
            ParentVM = parentVM;
        }

        private EditingTemplate _selected;
        public EditingTemplate Selected
        {
            get => _selected;
            set
            {
                SetProperty(ref _selected, value, () => Selected);
                NotifyPropertyChanged("Visibility");
            }
        }

        public string Visibility => Selected == null ? "Hidden" : "Visible";

        #region ### Validation ####

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

        private void ValidateChangeWasMade()
        {
            // Error message isn't display on a field. Just prevents user from hitting update until a change is made.
            const string propertyKey = "SilentError";

            if (Selected == null)
            {
                _validationErrors.Remove(propertyKey);
                return;
            }

            //if (Selected.Name == Name && Selected.Description == Description)
            //{
            //    _validationErrors[propertyKey] = new List<string>() { "No changes have been made." };
            //}
            //else
            //{
            //    _validationErrors.Remove(propertyKey);
            //}

            RaiseErrorsChanged(propertyKey);
        }

        #endregion
    }
}
