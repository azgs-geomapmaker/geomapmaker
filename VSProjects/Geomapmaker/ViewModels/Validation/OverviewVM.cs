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

        public string GemsResults { get; set; } = "Checking..";
        public string TopoResults { get; set; } = "Checking..";
        public string GeomapmakerResults { get; set; } = "Checking..";

        public void UpdateGemsResults(int errorCount)
        {
            if (errorCount == 0)
            {
                GemsResults = "Passed";
            }
            else
            {
                GemsResults = $"{errorCount} Failed";
                _validationErrors["GemsResults"] = new List<string> { "Failed" };
                RaiseErrorsChanged("GemsResults");
            }
            NotifyPropertyChanged("GemsResults");
        }

        public void UpdateTopoResults(int errorCount)
        {
            if (errorCount == 0)
            {
                TopoResults = "Passed";
            }
            else
            {
                TopoResults = $"{errorCount} Failed";
                _validationErrors["TopoResults"] = new List<string> { "Failed" };
                RaiseErrorsChanged("TopoResults");
            }
            NotifyPropertyChanged("TopoResults");
        }

        public void UpdateGeomapmakerResults(int errorCount)
        {
            if (errorCount == 0)
            {
                GeomapmakerResults = "Passed";
            }
            else
            {
                GeomapmakerResults = $"{errorCount} Failed";
                _validationErrors["GeomapmakerResults"] = new List<string> { "Failed" };
                RaiseErrorsChanged("GeomapmakerResults");
            }
            NotifyPropertyChanged("GeomapmakerResults");
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
