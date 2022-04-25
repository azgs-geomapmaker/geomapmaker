﻿using ArcGIS.Desktop.Framework.Contracts;
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
        public string GeomapmakerResults { get; set; } = "Skipped";
        public string Level1Results { get; set; } = "Skipped";
        public string Level2Results { get; set; } = "Skipped";
        public string Level3Results { get; set; } = "Skipped";

        public void UpdateGemsResults(int errorCount)
        {
            if (errorCount == 0)
            {
                GemsResults = "Passed";
            }
            else if (errorCount == 1)
            {
                GemsResults = "1 Error";
                _validationErrors["GemsResults"] = new List<string> { GeomapmakerResults };
                RaiseErrorsChanged("GemsResults");
            }
            else
            {
                GemsResults = $"{errorCount} Errors";
                _validationErrors["GemsResults"] = new List<string> { GeomapmakerResults };
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
            else if (errorCount == 1)
            {
                TopoResults = "1 Error";
                _validationErrors["TopoResults"] = new List<string> { TopoResults };
                RaiseErrorsChanged("TopoResults");
            }
            else
            {
                TopoResults = $"{errorCount} Errors";
                _validationErrors["TopoResults"] = new List<string> { TopoResults };
                RaiseErrorsChanged("TopoResults");
            }
            NotifyPropertyChanged("TopoResults");
        }

        public void UpdateLevel1Results(int errorCount)
        {
            if (errorCount == 0)
            {
                Level1Results = "Passed";
            }
            else if (errorCount == 1)
            {
                Level1Results = "1 Error";
                _validationErrors["Level1Results"] = new List<string> { Level1Results };
                RaiseErrorsChanged("Level1Results");
            }
            else
            {
                Level1Results = $"{errorCount} Errors";
                _validationErrors["Level1Results"] = new List<string> { Level1Results };
                RaiseErrorsChanged("Level1Results");
            }
            NotifyPropertyChanged("Level1Results");
        }

        public void UpdateLevel2Results(int errorCount)
        {
            if (errorCount == 0)
            {
                Level2Results = "Passed";
            }
            else if (errorCount == 1)
            {
                Level2Results = "1 Error";
                _validationErrors["Level2Results"] = new List<string> { Level2Results };
                RaiseErrorsChanged("Level2Results");
            }
            else
            {
                Level2Results = $"{errorCount} Errors";
                _validationErrors["Level2Results"] = new List<string> { Level2Results };
                RaiseErrorsChanged("Level2Results");
            }
            NotifyPropertyChanged("Level2Results");
        }

        public void UpdateLevel3Results(int errorCount)
        {
            if (errorCount == 0)
            {
                Level3Results = "Passed";
            }
            else if (errorCount == 1)
            {
                Level3Results = "1 Error";
                _validationErrors["Level3Results"] = new List<string> { Level3Results };
                RaiseErrorsChanged("Level3Results");
            }
            else
            {
                Level3Results = $"{errorCount} Errors";
                _validationErrors["Level3Results"] = new List<string> { Level3Results };
                RaiseErrorsChanged("Level3Results");
            }
            NotifyPropertyChanged("Level3Results");
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
