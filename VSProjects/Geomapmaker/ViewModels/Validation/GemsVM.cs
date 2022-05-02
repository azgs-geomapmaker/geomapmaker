using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Geomapmaker.ViewModels.Validation
{
    public class GemsVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ValidationViewModel ParentVM { get; set; }

        public GemsVM(ValidationViewModel parentVM)
        {
            ParentVM = parentVM;
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
        public string Result10 { get; set; } = "Checking..";
        public string Result11 { get; set; } = "Checking..";
        public string Result12 { get; set; } = "Checking..";
        public string Result13 { get; set; } = "Checking..";

        public async Task Validate()
        {
            Result1 = await Check1Async("Result1");
            NotifyPropertyChanged("Result1");

            Result2 = await Check2Async("Result2");
            NotifyPropertyChanged("Result2");

            Result3 = await Check3Async("Result3");
            NotifyPropertyChanged("Result3");

            ParentVM.UpdateGemsResults(_validationErrors.Count);
        }

        // Tooltip
        public string Check1Tooltip => "Check that the table exists.<br>" +
                                       "Check table for any missing fields.<br>" +
                                       "Check for empty/null values in required fields.<br>" +
                                       "Check for any duplicate IDs.<br>Check for unused data sources.<br>" +
                                       "Check for missing data sources.";

        // 1. Validate DataSources table
        private async Task<string> Check1Async(string propertyKey)
        {
            List<string> errors = new List<string>();

            // Check if the table exists
            if (await Data.DataSources.DataSourceExistsAsync() == false)
            {
                errors.Add("Table not found: DataSources");
            }
            else // Table was found
            {
                //
                // Check table for any missing fields 
                //
                List<string> missingFields = await Data.DataSources.GetMissingFieldsAsync();
                if (missingFields.Count != 0)
                {
                    foreach (string field in missingFields)
                    {
                        errors.Add($"Field not found: {field}");
                    }
                }

                //
                // Check for empty/null values in required fields
                //
                List<string> fieldsWithMissingValues = await Data.DataSources.GetRequiredFieldsWithNullValues();
                if (fieldsWithMissingValues.Count != 0)
                {
                    foreach (string field in fieldsWithMissingValues)
                    {
                        errors.Add($"Null value found in field: {field}");
                    }
                }

                //
                // Check for any duplicate ids
                //
                List<string> duplicateIds = await Data.DataSources.GetDuplicateIdsAsync();
                if (duplicateIds.Count != 0)
                {
                    foreach (string id in duplicateIds)
                    {
                        errors.Add($"Duplicate datasources_id: {id}");
                    }
                }

                //
                // Check for unused data sources
                //
                List<string> unusedDataSources = await Data.DataSources.GetUnnecessaryDataSources();
                if (unusedDataSources.Count != 0)
                {
                    foreach (string ds in unusedDataSources)
                    {
                        errors.Add($"Unused data source: {ds}");
                    }
                }

                //
                // Check for missing data sources
                //
                List<string> missingDataSources = await Data.DataSources.GetMissingDataSources();
                if (missingDataSources.Count != 0)
                {
                    foreach (string ds in missingDataSources)
                    {
                        errors.Add($"Missing data source: {ds}");
                    }
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

        // Tooltip
        public string Check2Tooltip => "Check that the table exists.<br>" +
                                       "Check table for any missing fields.<br>" +
                                       "Check for duplicate MapUnit values.<br>" +
                                       "Check for empty/null values in required fields.";

        // 2. Validate DescriptionOfMapUnits table
        private async Task<string> Check2Async(string propertyKey)
        {
            List<string> errors = new List<string>();

            // Check if the table exists
            if (await Data.DescriptionOfMapUnits.DmuTableExistsAsync() == false)
            {
                errors.Add("Table not found: DescriptionOfMapUnits");
            }
            else // Table was found
            {
                //
                // Check table for any missing fields 
                //
                List<string> missingFields = await Data.DescriptionOfMapUnits.GetMissingFieldsAsync();
                if (missingFields.Count != 0)
                {
                    foreach (string field in missingFields)
                    {
                        errors.Add($"Field not found: {field}");
                    }
                }

                //
                // Check for duplicate mapunit values
                //
                List<string> duplicateMapUnits = await Data.DescriptionOfMapUnits.GetDuplicateMapUnitsAsync();
                if (duplicateMapUnits.Count != 0)
                {
                    foreach (string duplicate in duplicateMapUnits)
                    {
                        errors.Add($"Duplicate MapUnit value: {duplicate}");
                    }
                }

                //
                // Check for empty/null values in required fields for ALL DMU ROWS
                //
                List<string> fieldsWithMissingValues = await Data.DescriptionOfMapUnits.GetRequiredFieldsWithNullValues();
                if (fieldsWithMissingValues.Count != 0)
                {
                    foreach (string field in fieldsWithMissingValues)
                    {
                        errors.Add($"Null value found in field: {field}");
                    }
                }

                //
                // Check for empty/null values in required fields for MAPUNIT dmu rows (not headings)
                //
                List<string> mapUnitfieldsWithMissingValues = await Data.DescriptionOfMapUnits.GetMapUnitRequiredFieldsWithNullValues();
                if (fieldsWithMissingValues.Count != 0)
                {
                    foreach (string field in mapUnitfieldsWithMissingValues)
                    {
                        errors.Add($"Null value found in field: {field}");
                    }
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

        // Tooltip
        public string Check3Tooltip => "Check that the layer exists.<br>";

        // 3. Validate MapUnitPolys layer
        private async Task<string> Check3Async(string propertyKey)
        {
            List<string> errors = new List<string>();

            // Check if the layer exists
            if (await Data.MapUnitPolys.MapUnitPolysExistsAsync() == false)
            {
                errors.Add("Feature layer not found: MapUnitPolys");
            }
            else // Layer was found
            {

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

        // 3.4 No missing terms in Glossary
        private string Check4()
        {
            return "Skipped";
        }

        // 3.5 No unnecessary terms in Glossary
        private string Check5()
        {
            return "Skipped";
        }

        // 3.6 No missing sources in DataSources
        private string Check6()
        {
            return "Skipped";
        }

        // 3.7 No unnecessary sources in DataSources
        private string Check7()
        {
            return "Skipped";
        }

        // 3.8 No map units without entries in DescriptionOfMapUnits
        private string Check8()
        {
            return "Skipped";
        }

        // 3.9 No unnecessary map units in DescriptionOfMapUnits
        private string Check9()
        {
            return "Skipped";
        }

        // 3.10 HierarchyKey values in DescriptionOfMapUnits are unique and well formed
        private string Check10()
        {
            return "Skipped";
        }

        // 3.11 All values of GeoMaterial are defined in GeoMaterialDict. GeoMaterialDict is as specified in the GeMS standard
        private string Check11()
        {
            return "Skipped";
        }

        // 3.12 No duplicate _ID values
        private string Check12()
        {
            return "Skipped";
        }

        // 3.13 No zero-length or whitespace-only strings
        private string Check13()
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
