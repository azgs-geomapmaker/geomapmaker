using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Geomapmaker.ViewModels.Validation
{
    public class TopoVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ValidationViewModel ParentVM { get; set; }

        public TopoVM(ValidationViewModel parentVM)
        {
            ParentVM = parentVM;
            Result1 = Check1();
            Result2 = Check2();
            Result3 = Check3();

            ParentVM.UpdateTopoResults(_validationErrors.Count);
        }

        public string Result1 { get; set; } = "Checking..";
        public string Result2 { get; set; } = "Checking..";
        public string Result3 { get; set; } = "Checking..";

        // 1.1 No overlaps or internal gaps in map-unit polygon layer
        private string Check1()
        {
            //List<string> errors = new List<string>();

            //errors.Add("Topo Error 1");
            //errors.Add("Topo Error 2");
            //errors.Add("Topo Error 3");

            //_validationErrors["Result1"] = _helpers.Helpers.ErrorListToTooltip(errors);

            //RaiseErrorsChanged("Result1");

            //return "Failed";

            return "Skipped";
        }

        // 1.2 Contacts and faults in single feature class
        private string Check2()
        {
            return "Skipped";
        }

        // 1.3 Map-unit polygon boundaries are covered by contacts and faults lines
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
