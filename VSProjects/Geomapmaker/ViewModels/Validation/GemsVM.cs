using ArcGIS.Desktop.Framework.Contracts;
using Geomapmaker.Data;
using Geomapmaker.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
            if (await General.StandaloneTableExistsAsync("DataSources") == false)
            {
                errors.Add("Table not found: DataSources");
            }
            else // Table was found
            {
                //
                // Check table for any missing fields 
                //

                // Required fields for DataSources
                List<string> dsRequiredFields = new List<string>() { "source", "datasources_id", "url", "notes" };

                // Get missing required fields
                List<string> missingFields = await General.StandaloneTableGetMissingFieldsAsync("DataSources", dsRequiredFields);
                foreach (string field in missingFields)
                {
                    errors.Add($"Field not found: {field}");
                }

                //
                // Check for empty/null values in required fields
                //
                // List of fields that can't have null values
                List<string> dsNotNullFields = new List<string>() { "source", "datasources_id" };
                // Get required fields with a null value
                List<string> fieldsWithMissingValues = await General.StandaloneTableGetRequiredFieldIsNullAsync("DataSources", dsNotNullFields);
                foreach (string field in fieldsWithMissingValues)
                {
                    errors.Add($"Null value found in field: {field}");
                }

                //
                // Check for any duplicate ids
                //
                List<string> duplicateIds = await General.StandaloneTableGetDuplicateValuesInFieldAsync("DataSources", "datasources_id");
                foreach (string id in duplicateIds)
                {
                    errors.Add($"Duplicate datasources_id: {id}");
                }

                //
                // Check for unused data sources
                //
                List<string> unusedDataSources = await Data.DataSources.GetUnnecessaryDataSources();
                foreach (string ds in unusedDataSources)
                {
                    errors.Add($"Unused data source: {ds}");
                }

                //
                // Check for missing data sources
                //
                List<string> missingDataSources = await Data.DataSources.GetMissingDataSources();
                foreach (string ds in missingDataSources)
                {
                    errors.Add($"Missing data source: {ds}");
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
                                       "No duplicate HierarchyKey values.<br>" +
                                       "No duplicate DescriptionOfMapUnits_ID values.<br>" +
                                       "HierarchyKeys unique and well-formed";

        // 2. Validate DescriptionOfMapUnits table
        private async Task<string> Check2Async(string propertyKey)
        {
            List<string> errors = new List<string>();

            // Check if the table exists
            if (await General.StandaloneTableExistsAsync("DescriptionOfMapUnits") == false)
            {
                errors.Add("Table not found: DescriptionOfMapUnits");
            }
            else // Table was found
            {
                //
                // Check table for any missing fields 
                //

                // List of required fields to check
                List<string> dmuRequiredFields = new List<string>() { "mapunit", "name", "fullname", "age", "description", "hierarchykey", "paragraphstyle", "label", "symbol", "areafillrgb",
                                                               "areafillpatterndescription", "descriptionsourceid", "geomaterial", "geomaterialconfidence", "descriptionofmapunits_id" };

                // Get misssing required fields
                List<string> missingFields = await General.StandaloneTableGetMissingFieldsAsync("DescriptionOfMapUnits", dmuRequiredFields);
                foreach (string field in missingFields)
                {
                    errors.Add($"Field not found: {field}");
                }

                //
                // Check for duplicate mapunit values
                //
                List<string> duplicateMapUnits = await General.StandaloneTableGetDuplicateValuesInFieldAsync("DescriptionOfMapUnits", "MapUnit", "MapUnit IS NOT NULL");
                foreach (string duplicate in duplicateMapUnits)
                {
                    errors.Add($"Duplicate MapUnit value: {duplicate}");
                }

                //
                // Check for duplicate DescriptionOfMapUnits_ID values
                //
                List<string> duplicateIds = await General.StandaloneTableGetDuplicateValuesInFieldAsync("DescriptionOfMapUnits", "DescriptionOfMapUnits_ID");
                foreach (string id in duplicateIds)
                {
                    errors.Add($"Duplicate DescriptionOfMapUnits_ID value: {id}");
                }

                //
                // Check for duplicate name values
                //
                List<string> duplicateNames = await General.StandaloneTableGetDuplicateValuesInFieldAsync("DescriptionOfMapUnits", "name");
                foreach (string name in duplicateNames)
                {
                    errors.Add($"Duplicate Name value: {name}");
                }

                //
                // Check for duplicate fullname values
                //
                List<string> duplicateFullNames = await General.StandaloneTableGetDuplicateValuesInFieldAsync("DescriptionOfMapUnits", "fullname");
                foreach (string fullName in duplicateFullNames)
                {
                    errors.Add($"Duplicate FullName value: {fullName}");
                }

                //
                // Check for duplicate rgb values
                //
                List<string> duplicateRGB = await General.StandaloneTableGetDuplicateValuesInFieldAsync("DescriptionOfMapUnits", "areafillrgb");
                foreach (string rgb in duplicateRGB)
                {
                    errors.Add($"Duplicate AreaFillRGB value: {rgb}");
                }

                //
                // Check for duplicate hierarchykey values
                //
                List<string> duplicateHierarchyKeys = await General.StandaloneTableGetDuplicateValuesInFieldAsync("DescriptionOfMapUnits", "hierarchykey");
                foreach (string hkey in duplicateHierarchyKeys)
                {
                    errors.Add($"Duplicate HierarchyKey value: {hkey}");
                }

                //
                // Check for empty/null values in required fields for ALL DMU ROWS
                //

                // DMU fields that can't have nulls
                List<string> dmuNotNull = new List<string>() { "name", "hierarchykey", "paragraphstyle", "descriptionsourceid", "descriptionofmapunits_id" };

                // Get required fields with a null value
                List<string> fieldsWithMissingValues = await General.StandaloneTableGetRequiredFieldIsNullAsync("DescriptionOfMapUnits", dmuNotNull);
                foreach (string field in fieldsWithMissingValues)
                {
                    errors.Add($"Null value found in field: {field}");
                }

                //
                // Check for empty/null values in required fields for MAPUNIT dmu rows (not headings)
                //

                // List of fields that can't be null
                List<string> mapUnitNotNull = new List<string>() { "mapunit", "fullname", "age", "areafillrgb",
                                                              "geomaterial", "geomaterialconfidence" };

                // Get required fields with null values. Using the 'MapUnit is not null' where clause to only check MapUnit rows
                List<string> mapUnitfieldsWithMissingValues = await General.StandaloneTableGetRequiredFieldIsNullAsync("DescriptionOfMapUnits", mapUnitNotNull, "MapUnit IS NOT NULL");
                foreach (string field in mapUnitfieldsWithMissingValues)
                {
                    errors.Add($"Null value found in MapUnit field: {field}");
                }

                //
                // Check for any MapUnits defined in DMU, but not used in MapUnitPolys
                //
                List<string> unusedDMU = await Data.DescriptionOfMapUnits.GetUnusedMapUnitsAsync();
                foreach (string mu in unusedDMU)
                {
                    errors.Add($"Unused MapUnit: {mu}");
                }

                //
                // Check for HierarchyKey values that don't fit in the tree 
                //

                // Get the tree and unassigned list
                Tuple<List<MapUnitTreeItem>, List<MapUnitTreeItem>> tuple = await Data.DescriptionOfMapUnits.GetHierarchyTreeAsync();

                // Filter out the null/empty HierarchyKeys from the list of unsassigned rows
                List<MapUnitTreeItem> unassignedNotNull = tuple.Item2.Where(a => !string.IsNullOrEmpty(a.HierarchyKey)).ToList();
                foreach (MapUnitTreeItem row in unassignedNotNull)
                {
                    errors.Add($"Bad HierarchyKey: {row.HierarchyKey}");
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
                foreach (string field in missingFields)
                {
                    errors.Add($"Field not found: {field}");
                }

                //
                // Check for empty/null values in required fields
                //
                List<string> fieldsWithMissingValues = await Data.Glossary.GetRequiredFieldsWithNullValues();
                foreach (string field in fieldsWithMissingValues)
                {
                    errors.Add($"Null value found in field: {field}");
                }

                //
                // Check for any duplicate ids
                //
                List<string> duplicateIds = await Data.Glossary.GetDuplicateIdsAsync();
                foreach (string id in duplicateIds)
                {
                    errors.Add($"Duplicate glossary_id: {id}");
                }

                //
                // Check for any duplicate terms
                //
                List<string> duplicateTerms = await Data.Glossary.GetDuplicateTermsAsync();
                foreach (string term in duplicateTerms)
                {
                    errors.Add($"Duplicate term: {term}");
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
                foreach (string field in missingFields)
                {
                    errors.Add($"Field not found: {field}");
                }

                //
                // Check for empty/null values in required fields
                //
                List<string> fieldsWithMissingValues = await Data.GeoMaterialDict.GetRequiredFieldsWithNullValues();
                foreach (string field in fieldsWithMissingValues)
                {
                    errors.Add($"Null value found in field: {field}");
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
                foreach (string field in missingFields)
                {
                    errors.Add($"Field not found: {field}");
                }

                //
                // Check for any missing MapUnit definitions in the DMU
                //
                List<string> missingDMU = await Data.MapUnitPolys.GetMapUnitsNotDefinedInDMUTableAsync();
                foreach (string mu in missingDMU)
                {
                    errors.Add($"MapUnit not defined in DMU: {mu}");
                }

                //
                // Check for empty/null values in required fields
                //
                List<string> fieldsWithMissingValues = await Data.MapUnitPolys.GetRequiredFieldsWithNullValues();
                foreach (string field in fieldsWithMissingValues)
                {
                    errors.Add($"Null value found in field: {field}");
                }

                //
                // Check for duplicate MapUnitPolys_ID values
                //
                List<string> duplicateIds = await Data.MapUnitPolys.GetDuplicateIdsAsync();
                foreach (string id in duplicateIds)
                {
                    errors.Add($"Duplicate MapUnitPolys_ID value: {id}");
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
            if (await General.FeatureLayerExistsAsync("ContactsAndFaults") == false)
            {
                errors.Add("Feature layer not found: ContactsAndFaults");
            }
            else // Layer was found
            {
                //
                // Check layer for any missing fields 
                //
                List<string> cfRequiredFields = new List<string>() { "type", "isconcealed", "locationconfidencemeters", "existenceconfidence",
                "identityconfidence", "label", "symbol", "datasourceid", "notes", "contactsandfaults_id" };

                List<string> missingFields = await General.FeatureLayerGetMissingFieldsAsync("ContactsAndFaults", cfRequiredFields);
                foreach (string field in missingFields)
                {
                    errors.Add($"Field not found: {field}");
                }

                //
                // Check for empty/null values in required fields
                //
                List<string> cfNotNullFields = new List<string>() { "type", "isconcealed", "locationconfidencemeters", "existenceconfidence", "identityconfidence", "datasourceid", "contactsandfaults_id" };
                List<string> fieldsWithMissingValues = await General.FeatureLayerGetRequiredFieldIsNullAsync("ContactsAndFaults", cfNotNullFields);
                foreach (string field in fieldsWithMissingValues)
                {
                    errors.Add($"Null value found in field: {field}");
                }

                //
                // Check for duplicate Label values
                //
                List<string> duplicateLabels = await General.FeatureLayerGetDuplicateValuesInFieldAsync("ContactsAndFaults", "Label");
                foreach (string label in duplicateLabels)
                {
                    errors.Add($"Duplicate Label value: {label}");
                }

                //
                // Check for duplicate ContactsAndFaults_ID values
                //
                List<string> duplicateIds = await General.FeatureLayerGetDuplicateValuesInFieldAsync("ContactsAndFaults", "ContactsAndFaults_ID");
                foreach (string id in duplicateIds)
                {
                    errors.Add($"Duplicate ContactsAndFaults_ID value: {id}");
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
                foreach (string field in missingFields)
                {
                    errors.Add($"Field not found: {field}");
                }

                //
                // Check for empty/null values in required fields
                //
                List<string> fieldsWithMissingValues = await Data.Stations.GetRequiredFieldsWithNullValues();
                foreach (string field in fieldsWithMissingValues)
                {
                    errors.Add($"Null value found in field: {field}");
                }

                //
                // Check for duplicate Stations_ID values
                //
                List<string> duplicateIds = await Data.Stations.GetDuplicateIdsAsync();
                foreach (string id in duplicateIds)
                {
                    errors.Add($"Duplicate Stations_ID value: {id}");
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
                foreach (string field in missingFields)
                {
                    errors.Add($"Field not found: {field}");
                }

                //
                // Check for empty/null values in required fields
                //
                List<string> fieldsWithMissingValues = await Data.OrientationPoints.GetRequiredFieldsWithNullValues();
                foreach (string field in fieldsWithMissingValues)
                {
                    errors.Add($"Null value found in field: {field}");
                }

                //
                // Check for duplicate OrientationPoints_ID values
                //
                List<string> duplicateIds = await Data.OrientationPoints.GetDuplicateIdsAsync();
                foreach (string id in duplicateIds)
                {
                    errors.Add($"Duplicate OrientationPoints_ID value: {id}");
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
