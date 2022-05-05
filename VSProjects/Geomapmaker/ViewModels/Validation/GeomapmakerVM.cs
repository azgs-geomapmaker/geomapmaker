using ArcGIS.Desktop.Framework.Contracts;
using Geomapmaker.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Geomapmaker.ViewModels.Validation
{
    public class GeomapmakerVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ValidationViewModel ParentVM { get; set; }

        public GeomapmakerVM(ValidationViewModel parentVM)
        {
            ParentVM = parentVM;
        }

        public async Task Validate()
        {
            Result1 = await Check1Async("Result1");
            NotifyPropertyChanged("Result1");

            ParentVM.UpdateGeomapmakerResults(_validationErrors.Count);
        }

        public string Result1 { get; set; } = "Checking..";

        public string Check1Tooltip => "Check that the table exists.<br>" +
                                       "Check table for any missing fields.";

        // 1 Symbology table exists
        private async Task<string> Check1Async(string propertyKey)
        {
            List<string> errors = new List<string>();

            // Check if the table exists
            if (await General.StandaloneTableExistsAsync("Symbology") == false)
            {
                errors.Add("Symbology table not found");
            }
            else // Table was found
            {
                //
                // Check for any missing fields 
                //

                // List of required fields
                List<string> symbologyRequiredFields = new List<string>() { "type", "key_", "description", "symbol" };

                // Get missing fields
                List<string> missingFields = await General.StandaloneTableGetMissingFieldsAsync("Symbology", symbologyRequiredFields);
                // Add errors for any missing fields
                foreach (string field in missingFields)
                {
                    errors.Add($"Field '{field}' not found");
                }
            }

            if (errors.Count == 0)
            {
                return "Passed";
            }
            else
            {
                _validationErrors[propertyKey] = _helpers.Helpers.ErrorListToTooltip(errors);
                RaiseErrorsChanged(propertyKey);
                return "Failed";
            }
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
