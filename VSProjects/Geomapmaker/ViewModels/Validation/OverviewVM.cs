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
            Level1Results = "Skipped";
            Level2Results = "Skipped";
            Level3Results = "Skipped";
            GeomapmakerResults = "Skipped";
        }

        private string _level1Results = "Checking..";
        public string Level1Results
        {
            get => _level1Results;
            set => SetProperty(ref _level1Results, value, () => Level1Results);
        }

        private string _level2Results = "Checking..";
        public string Level2Results
        {
            get => _level2Results;
            set => SetProperty(ref _level2Results, value, () => Level2Results);
        }

        private string _level3Results = "Checking..";
        public string Level3Results
        {
            get => _level3Results;
            set => SetProperty(ref _level3Results, value, () => Level3Results);
        }

        private string _geoResults = "Checking..";
        public string GeomapmakerResults
        {
            get => _geoResults;
            set => SetProperty(ref _geoResults, value, () => GeomapmakerResults);
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
