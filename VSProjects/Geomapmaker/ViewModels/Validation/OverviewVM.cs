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
            Stage1Results = "Skipped";
            Stage2Results = "Skipped";
            Stage3Results = "Skipped";
            AzgsResults = "Skipped";
        }

        private string _stage1Results = "Checking..";
        public string Stage1Results
        {
            get => _stage1Results;
            set => SetProperty(ref _stage1Results, value, () => Stage1Results);
        }

        private string _stage2Results = "Checking..";
        public string Stage2Results
        {
            get => _stage2Results;
            set => SetProperty(ref _stage2Results, value, () => Stage2Results);
        }

        private string _stage3Results = "Checking..";
        public string Stage3Results
        {
            get => _stage3Results;
            set => SetProperty(ref _stage3Results, value, () => Stage3Results);
        }

        private string _azgsResults = "Checking..";
        public string AzgsResults
        {
            get => _azgsResults;
            set => SetProperty(ref _azgsResults, value, () => AzgsResults);
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
