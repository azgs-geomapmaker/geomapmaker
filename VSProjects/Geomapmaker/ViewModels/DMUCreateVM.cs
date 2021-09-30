using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Geomapmaker.ViewModels
{
    public class DMUCreateVM : DockPane, INotifyDataErrorInfo
    {
        // Heading Name
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                SetProperty(ref _name, value, () => Name);
                //ValidateHeadingName(_name);
            }
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

        // Validate the Heading's name
        //private void ValidateHeadingName(string name)
        //{
        //    const string propertyKey = "Name";

        //    // Required field
        //    if (string.IsNullOrWhiteSpace(name))
        //    {
        //        _validationErrors[propertyKey] = new List<string>() { "" };
        //        RaiseErrorsChanged(propertyKey);
        //    }
        //    // Name must be unique 
        //    else if (DataHelper.MapUnits.Any(a => a.Name.ToLower() == name.ToLower()))
        //    {
        //        _validationErrors[propertyKey] = new List<string>() { "Name is taken." };
        //        RaiseErrorsChanged(propertyKey);
        //    }
        //    else
        //    {
        //        _validationErrors.Remove(propertyKey);
        //        RaiseErrorsChanged(propertyKey);
        //    }
        //}

        #endregion
    }
}
