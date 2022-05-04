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

            ParentVM.UpdateGemsResults(_validationErrors.Count);
        }

        // 1. DataSources Tooltip
        public string Check1Tooltip => "Table exists.<br>" +
                                       "No missing fields.<br>" +
                                       "No empty/null values in required fields.<br>" +
                                       "No duplicate datasources_id values.<br>" +
                                       "No unused datasources_id values.<br>" +
                                       "No missing datasources_id values.";

        // 1. Validate DataSources table
        private async Task<string> Check1Async(string propertyKey)
        {
            List<string> errors = new List<string>();

            // Check if the table exists
            if (await Data.DataSources.StandaloneTableExistsAsync() == false)
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

        // 2. DescriptionOfMapUnits Tooltip
        public string Check2Tooltip => "Table exists.<br>" +
                                       "No missing fields.<br>" +
                                       "No empty/null values in required fields.<br>" +
                                       "No duplicate MapUnit values.<br>" +
                                       "No duplicate Name values.<br>" +
                                       "No duplicate FullName values.<br>" +
                                       "No duplicate AreaFillRGB values.<br>" +
                                       "No duplicate DescriptionOfMapUnits_ID values.<br>";

        // 2. Validate DescriptionOfMapUnits table
        private async Task<string> Check2Async(string propertyKey)
        {
            List<string> errors = new List<string>();

            // Check if the table exists
            if (await Data.DescriptionOfMapUnits.StandaloneTableExistsAsync() == false)
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
                // Check for duplicate DescriptionOfMapUnits_ID values
                //
                List<string> duplicateIds = await Data.DescriptionOfMapUnits.GetDuplicateIdsAsync();
                if (duplicateIds.Count != 0)
                {
                    foreach (string id in duplicateIds)
                    {
                        errors.Add($"Duplicate DescriptionOfMapUnits_ID value: {id}");
                    }
                }

                //
                // Check for duplicate name values
                //
                List<string> duplicateNames = await Data.DescriptionOfMapUnits.GetDuplicateNamesAsync();
                if (duplicateNames.Count != 0)
                {
                    foreach (string name in duplicateNames)
                    {
                        errors.Add($"Duplicate Name value: {name}");
                    }
                }

                //
                // Check for duplicate fullname values
                //
                List<string> duplicateFullNames = await Data.DescriptionOfMapUnits.GetDuplicateFullNamesAsync();
                if (duplicateFullNames.Count != 0)
                {
                    foreach (string fullName in duplicateFullNames)
                    {
                        errors.Add($"Duplicate FullName value: {fullName}");
                    }
                }

                //
                // Check for duplicate rgb values
                //
                List<string> duplicateRGB = await Data.DescriptionOfMapUnits.GetDuplicateRGBAsync();
                if (duplicateRGB.Count != 0)
                {
                    foreach (string rgb in duplicateRGB)
                    {
                        errors.Add($"Duplicate AreaFillRGB value: {rgb}");
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
                if (mapUnitfieldsWithMissingValues.Count != 0)
                {
                    foreach (string field in mapUnitfieldsWithMissingValues)
                    {
                        errors.Add($"Null value found in MapUnit field: {field}");
                    }
                }

                //
                // Check for any MapUnits defined in DMU, but not used in MapUnitPolys
                //
                List<string> unusedDMU = await Data.DescriptionOfMapUnits.GetUnusedMapUnitsAsync();
                if (unusedDMU.Count != 0)
                {
                    foreach (string mu in unusedDMU)
                    {
                        errors.Add($"Unused MapUnit: {mu}");
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

        // 3. Glossary Tooltip
        public string Check3Tooltip => "Table exists.<br>" +
                                       "No missing fields.<br>" +
                                       "No empty/null values in required fields.<br>" +
                                       "No duplicate Glossary_ID values.<br>" +
                                       "No duplicate Term values.<br>";

        // 3. Validate Glossary
        private async Task<string> Check3Async(string propertyKey)
        {
            List<string> errors = new List<string>();

            // Check if the table exists
            if (await Data.Glossary.StandaloneTableExistsAsync() == false)
            {
                errors.Add("Table not found: Glossary");
            }
            else // Table was found
            {
                //
                // Check table for any missing fields 
                //
                List<string> missingFields = await Data.Glossary.GetMissingFieldsAsync();
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
                List<string> fieldsWithMissingValues = await Data.Glossary.GetRequiredFieldsWithNullValues();
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
                List<string> duplicateIds = await Data.Glossary.GetDuplicateIdsAsync();
                if (duplicateIds.Count != 0)
                {
                    foreach (string id in duplicateIds)
                    {
                        errors.Add($"Duplicate glossary_id: {id}");
                    }
                }

                //
                // Check for any duplicate terms
                //
                List<string> duplicateTerms = await Data.Glossary.GetDuplicateTermsAsync();
                if (duplicateTerms.Count != 0)
                {
                    foreach (string term in duplicateTerms)
                    {
                        errors.Add($"Duplicate term: {term}");
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

        // 4. GeoMaterialDict Tooltip
        public string Check4Tooltip => "Table exists.<br>" +
                                       "No missing fields.<br>" +
                                       "No empty/null values in required fields.<br>";

        // 4. Validate GeoMaterialDict
        private async Task<string> Check4Async(string propertyKey)
        {
            List<string> errors = new List<string>();

            // Check if the table exists
            if (await Data.GeoMaterialDict.StandaloneTableExistsAsync() == false)
            {
                errors.Add("Table not found: GeoMaterialDict");
            }
            else // Table was found
            {
                //
                // Check table for any missing fields 
                //
                List<string> missingFields = await Data.GeoMaterialDict.GetMissingFieldsAsync();
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
                List<string> fieldsWithMissingValues = await Data.GeoMaterialDict.GetRequiredFieldsWithNullValues();
                if (fieldsWithMissingValues.Count != 0)
                {
                    foreach (string field in fieldsWithMissingValues)
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

        // 5. MapUnitPolys Tooltip
        public string Check5Tooltip => "Layer exists.<br>" +
                                       "No missing fields.<br>" +
                                       "No empty/null values in required fields.<br>" +
                                       "No duplicate MapUnitPolys_ID values";

        // 5. Validate MapUnitPolys layer
        private async Task<string> Check5Async(string propertyKey)
        {
            List<string> errors = new List<string>();

            // Check if the layer exists
            if (await Data.MapUnitPolys.FeatureLayerExistsAsync() == false)
            {
                errors.Add("Feature layer not found: MapUnitPolys");
            }
            else // Layer was found
            {
                //
                // Check layer for any missing fields 
                //
                List<string> missingFields = await Data.MapUnitPolys.GetMissingFieldsAsync();
                if (missingFields.Count != 0)
                {
                    foreach (string field in missingFields)
                    {
                        errors.Add($"Field not found: {field}");
                    }
                }

                //
                // Check for any missing MapUnit definitions in the DMU
                //
                List<string> missingDMU = await Data.MapUnitPolys.GetMapUnitsNotDefinedInDMUTableAsync();
                if (missingDMU.Count != 0)
                {
                    foreach (string mu in missingDMU)
                    {
                        errors.Add($"MapUnit not defined in DMU: {mu}");
                    }
                }

                //
                // Check for empty/null values in required fields
                //
                List<string> fieldsWithMissingValues = await Data.MapUnitPolys.GetRequiredFieldsWithNullValues();
                if (fieldsWithMissingValues.Count != 0)
                {
                    foreach (string field in fieldsWithMissingValues)
                    {
                        errors.Add($"Null value found in field: {field}");
                    }
                }

                //
                // Check for duplicate MapUnitPolys_ID values
                //
                List<string> duplicateIds = await Data.MapUnitPolys.GetDuplicateIdsAsync();
                if (duplicateIds.Count != 0)
                {
                    foreach (string id in duplicateIds)
                    {
                        errors.Add($"Duplicate MapUnitPolys_ID value: {id}");
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

        // 6. ContactsAndFaults Tooltip
        public string Check6Tooltip => "Layer exists.<br>" +
                                       "No missing fields.<br>" +
                                       "No empty/null values in required fields.<br>" +
                                       "No duplicate Label values" + 
                                       "No duplicate ContactsAndFaults_ID values";

        // 6. Validate ContactsAndFaults layer
        private async Task<string> Check6Async(string propertyKey)
        {
            List<string> errors = new List<string>();

            // Check if the layer exists
            if (await Data.ContactsAndFaults.FeatureLayerExistsAsync() == false)
            {
                errors.Add("Feature layer not found: ContactsAndFaults");
            }
            else // Layer was found
            {
                //
                // Check layer for any missing fields 
                //
                List<string> missingFields = await Data.ContactsAndFaults.GetMissingFieldsAsync();
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
                List<string> fieldsWithMissingValues = await Data.ContactsAndFaults.GetRequiredFieldsWithNullValues();
                if (fieldsWithMissingValues.Count != 0)
                {
                    foreach (string field in fieldsWithMissingValues)
                    {
                        errors.Add($"Null value found in field: {field}");
                    }
                }

                //
                // Check for duplicate Label values
                //
                List<string> duplicateLabels = await Data.ContactsAndFaults.GetDuplicateLabelsAsync();
                if (duplicateLabels.Count != 0)
                {
                    foreach (string label in duplicateLabels)
                    {
                        errors.Add($"Duplicate Label value: {label}");
                    }
                }

                //
                // Check for duplicate ContactsAndFaults_ID values
                //
                List<string> duplicateIds = await Data.ContactsAndFaults.GetDuplicateIdsAsync();
                if (duplicateIds.Count != 0)
                {
                    foreach (string id in duplicateIds)
                    {
                        errors.Add($"Duplicate ContactsAndFaults_ID value: {id}");
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

        // 7. Stations Tooltip
        public string Check7Tooltip => "Layer exists.<br>" +
                                       "No missing fields.<br>" +
                                       "No empty/null values in required fields.<br>" +
                                       "No duplicate Stations_ID values";

        // 7. Validate Stations layer
        private async Task<string> Check7Async(string propertyKey)
        {
            List<string> errors = new List<string>();

            // Check if the layer exists
            if (await Data.Stations.FeatureLayerExistsAsync() == false)
            {
                // Optional layer. No error if not found
                return "Skipped";
            }
            else // Layer was found
            {
                //
                // Check table for any missing fields 
                //
                List<string> missingFields = await Data.Stations.GetMissingFieldsAsync();
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
                List<string> fieldsWithMissingValues = await Data.Stations.GetRequiredFieldsWithNullValues();
                if (fieldsWithMissingValues.Count != 0)
                {
                    foreach (string field in fieldsWithMissingValues)
                    {
                        errors.Add($"Null value found in field: {field}");
                    }
                }

                //
                // Check for duplicate Stations_ID values
                //
                List<string> duplicateIds = await Data.Stations.GetDuplicateIdsAsync();
                if (duplicateIds.Count != 0)
                {
                    foreach (string id in duplicateIds)
                    {
                        errors.Add($"Duplicate Stations_ID value: {id}");
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

        // 8. OrientationPoints Tooltip
        public string Check8Tooltip => "Layer exists.<br>" +
                                       "No missing fields.<br>" +
                                       "No empty/null values in required fields.<br>" +
                                       "No duplicate OrientationPoints_ID values";

        // 8. Validate OrientationPoints layer
        private async Task<string> Check8Async(string propertyKey)
        {
            List<string> errors = new List<string>();

            // Check if the layer exists
            if (await Data.OrientationPoints.FeatureLayerExistsAsync() == false)
            {
                // Optional layer. No error if not found
                return "Skipped";
            }
            else // Layer was found
            {
                //
                // Check table for any missing fields 
                //
                List<string> missingFields = await Data.OrientationPoints.GetMissingFieldsAsync();
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
                List<string> fieldsWithMissingValues = await Data.OrientationPoints.GetRequiredFieldsWithNullValues();
                if (fieldsWithMissingValues.Count != 0)
                {
                    foreach (string field in fieldsWithMissingValues)
                    {
                        errors.Add($"Null value found in field: {field}");
                    }
                }

                //
                // Check for duplicate OrientationPoints_ID values
                //
                List<string> duplicateIds = await Data.OrientationPoints.GetDuplicateIdsAsync();
                if (duplicateIds.Count != 0)
                {
                    foreach (string id in duplicateIds)
                    {
                        errors.Add($"Duplicate OrientationPoints_ID value: {id}");
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
