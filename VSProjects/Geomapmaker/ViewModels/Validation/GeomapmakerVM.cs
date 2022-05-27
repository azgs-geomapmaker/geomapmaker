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

            Result2 = await Check2Async("Result2");
            NotifyPropertyChanged("Result2");

            Result3 = await Check3Async("Result3");
            NotifyPropertyChanged("Result3");

            Result4 = await Check4Async("Result4");
            NotifyPropertyChanged("Result4");

            Result5 = await Check5Async("Result5");
            NotifyPropertyChanged("Result5");

            Result6 = await Check6Async("Result6");
            NotifyPropertyChanged("Result6");

            Result7 = await Check7Async("Result7");
            NotifyPropertyChanged("Result7");

            Result8 = await Check8Async("Result8");
            NotifyPropertyChanged("Result8");

            Result9 = await Check9Async("Result9");
            NotifyPropertyChanged("Result9");

            ParentVM.UpdateGeomapmakerResults(_validationErrors.Count);
        }

        public string Result1 { get; set; } = "Checking..";
        public string Result2 { get; set; } = "Checking..";
        public string Result3 { get; set; } = "Checking..";
        public string Result4 { get; set; } = "Checking..";
        public string Result5 { get; set; } = "Checking..";
        public string Result6 { get; set; } = "Checking..";
        public string Result7 { get; set; } = "Checking..";
        public string Result8 { get; set; } = "Checking..";
        public string Result9 { get; set; } = "Checking..";

        // Symbology
        public string Check1Tooltip => "Table exists.<br>" +
                                       "No duplicate tables.<br>" +
                                       "No missing fields.<br>";

        // Symbology
        private async Task<string> Check1Async(string propertyKey)
        {
            List<string> errors = new List<string>();

            // Check if the table exists
            if (await General.StandaloneTableExistsAsync("Symbology") == false)
            {
                errors.Add("Table not found: Symbology");
            }
            else // Table was found
            {
                //
                // Check for duplicate tables
                //
                int tableCount = General.StandaloneTableCount("Symbology");
                if (tableCount > 1)
                {
                    errors.Add($"Multiple Symbology tables found");
                }

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
                    errors.Add($"Field not found: {field}");
                }

                //
                // Check for any missing CF symbols
                //
                List<string> missingCFsymbols = await Symbology.GetMissingContactsAndFaultsSymbologyAsync();
                foreach (string key in missingCFsymbols)
                {
                    errors.Add($"Missing line symbology: {key}");
                }

                //
                // Check for any missing OP symbols
                //
                List<string> missingOPsymbols = await Symbology.GetMissingOrientationPointsSymbologyAsync();
                foreach (string key in missingOPsymbols)
                {
                    errors.Add($"Missing point symbology: {key}");
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

        // DataSources
        public string Check2Tooltip => "";

        // DataSources
        private async Task<string> Check2Async(string propertyKey)
        {
            List<string> errors = new List<string>();

            // Check if the table exists
            if (await General.StandaloneTableExistsAsync("DescriptionOfMapUnits"))
            {
                // Validation
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

        // DescriptionOfMapUnits
        public string Check3Tooltip => "Check for toolbar fields.";

        // DescriptionOfMapUnits
        private async Task<string> Check3Async(string propertyKey)
        {
            List<string> errors = new List<string>();

            // Check if the table exists
            if (await General.StandaloneTableExistsAsync("DescriptionOfMapUnits"))
            {
                //
                // Check table for missing toolbar fields
                //

                // List of required fields to check
                List<string> dmuRequiredFields = new List<string>() { "relativeage", "hexcolor" };

                // Get misssing required fields
                List<string> missingFields = await General.StandaloneTableGetMissingFieldsAsync("DescriptionOfMapUnits", dmuRequiredFields);
                foreach (string field in missingFields)
                {
                    errors.Add($"Field not found: {field}");
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

        // Glossary
        public string Check4Tooltip => "";

        // Glossary
        private async Task<string> Check4Async(string propertyKey)
        {
            List<string> errors = new List<string>();

            // Check if the table exists
            if (await General.StandaloneTableExistsAsync("Glossary"))
            {
                // Validation
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

        // GeoMaterialDict
        public string Check5Tooltip => "";

        // GeoMaterialDict
        private async Task<string> Check5Async(string propertyKey)
        {
            List<string> errors = new List<string>();

            // Check if the table exists
            if (await General.StandaloneTableExistsAsync("GeoMaterialDict"))
            {
                // Validation
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

        // MapUnitPolys
        public string Check6Tooltip => "";

        // MapUnitPolys
        private async Task<string> Check6Async(string propertyKey)
        {
            List<string> errors = new List<string>();

            // Check if the table exists
            if (await General.FeatureLayerExistsAsync("MapUnitPolys"))
            {
                // Validation
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

        // ContactsAndFaults
        public string Check7Tooltip => "";

        // ContactsAndFaults
        private async Task<string> Check7Async(string propertyKey)
        {
            List<string> errors = new List<string>();

            // Check if the table exists
            if (await General.FeatureLayerExistsAsync("ContactsAndFaults"))
            {
                // Validation
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

        // Stations
        public string Check8Tooltip => "";

        // Stations
        private async Task<string> Check8Async(string propertyKey)
        {
            List<string> errors = new List<string>();

            // Check if the table exists
            if (await General.FeatureLayerExistsAsync("Stations"))
            {
                // Validation
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

        // OrientationPoints
        public string Check9Tooltip => "";

        // OrientationPoints
        private async Task<string> Check9Async(string propertyKey)
        {
            List<string> errors = new List<string>();

            // Check if the table exists
            if (await General.FeatureLayerExistsAsync("OrientationPoints"))
            {
                // Validation
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

        #endregion Validation
    }
}