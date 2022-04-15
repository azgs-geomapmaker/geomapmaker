using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Geomapmaker.ViewModels.Validation
{
    public class OverviewVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ValidationViewModel ParentVM { get; set; }

        public OverviewVM(ValidationViewModel parentVM)
        {
            ParentVM = parentVM;
        }

        public string Level1Results { get; set; } = "Checking..";
        public string Level2Results { get; set; } = "Checking..";
        public string Level3Results { get; set; } = "Checking..";
        public string GeomapmakerResults { get; set; } = "Checking..";

        private void UpdateLevel1Results()
        {
            Level1Results = ParentVM.Level1.GetErrorCount == 0
                ? "Passed"
                : ParentVM.Level1.GetErrorCount == 1 ? "1 Error" : $"{ParentVM.Level1.GetErrorCount} Errors";
        }

        private void UpdateLevel2Results()
        {
            Level1Results = ParentVM.Level2.GetErrorCount == 0
                ? "Passed"
                : ParentVM.Level1.GetErrorCount == 1 ? "1 Error" : $"{ParentVM.Level1.GetErrorCount} Errors";
        }

        private void UpdateLevel3Results()
        {
            Level1Results = ParentVM.Level3.GetErrorCount == 0
                ? "Passed"
                : ParentVM.Level1.GetErrorCount == 1 ? "1 Error" : $"{ParentVM.Level1.GetErrorCount} Errors";
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
