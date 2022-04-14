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
            Result1 = Check1();
            Result2 = Check2();
            Result3 = Check3();
        }

        public string Result1 { get; set; } = "Checking..";
        public string Result2 { get; set; } = "Checking..";
        public string Result3 { get; set; } = "Checking..";

        private string Check1()
        {
            List<string> errors = new List<string>();

            errors.Add("Error 1");
            errors.Add("Error 2");
            errors.Add("Error 3");

            _validationErrors["Result1"] = Geomapmaker._helpers.Helpers.ErrorListTooltip(errors);

            RaiseErrorsChanged("Result1");

            return "Failed";
        }

        private string Check2()
        {
            return "Skipped";
        }

        private string Check3()
        {
            return "Skipped";
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
