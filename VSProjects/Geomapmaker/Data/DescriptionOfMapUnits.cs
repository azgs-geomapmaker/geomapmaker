using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Geomapmaker._helpers;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using Nelibur.ObjectMapper;
using System;
using ArcGIS.Desktop.Editing;

namespace Geomapmaker.Data
{
    public class DescriptionOfMapUnits
    {
        public static async Task<List<ValidationRule>> GetValidationResultsAsync()
        {
            List<ValidationRule> results = new List<ValidationRule>
            {
                new ValidationRule{ Description="Table exists."},
                new ValidationRule{ Description="No duplicate tables."},
                new ValidationRule{ Description="No missing fields."},
                new ValidationRule{ Description="No missing toolbar fields."},
                new ValidationRule{ Description="No duplicate MapUnit values."},
                new ValidationRule{ Description="No duplicate DescriptionOfMapUnits_ID values."},
                new ValidationRule{ Description="No duplicate Name values."},
                new ValidationRule{ Description="No duplicate FullName values."},
                new ValidationRule{ Description="No duplicate AreaFillRGB values."},
                new ValidationRule{ Description="No duplicate HierarchyKey values."},
                new ValidationRule{ Description="No bad HierarchyKey values."},
                new ValidationRule{ Description="No empty/null values in required fields (All Rows)."},
                new ValidationRule{ Description="No empty/null values in required fields (MapUnits)."},
                new ValidationRule{ Description="No unused MapUnits."},
                new ValidationRule{ Description="GeoMaterial are defined in GeoMaterialDict"},
            };

            if (await AnyStandaloneTable.DoesTableExistsAsync("DescriptionOfMapUnits") == false)
            {
                results[0].Status = ValidationStatus.Failed;
                results[0].Errors.Add("Table not found");
                return results;
            }
            else // Table was found
            {
                results[0].Status = ValidationStatus.Passed;

                //
                // Check for duplicate tables
                //
                int tableCount = AnyStandaloneTable.GetTableCount("DescriptionOfMapUnits");
                if (tableCount > 1)
                {
                    results[1].Status = ValidationStatus.Failed;
                    results[1].Errors.Add($"{tableCount} tables found");
                }
                else
                {
                    results[1].Status = ValidationStatus.Passed;
                }

                //
                // Check table for any missing fields 
                //

                // List of required fields to check
                List<string> dmuRequiredFields = new List<string>() { "mapunit", "name", "fullname", "age", "description", "hierarchykey", "paragraphstyle", "label", "symbol", "areafillrgb",
                                                               "areafillpatterndescription", "descriptionsourceid", "geomaterial", "geomaterialconfidence", "descriptionofmapunits_id" };

                // Get misssing required fields
                List<string> missingFields = await AnyStandaloneTable.GetMissingFieldsAsync("DescriptionOfMapUnits", dmuRequiredFields);
                if (missingFields.Count == 0)
                {
                    results[2].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[2].Status = ValidationStatus.Failed;
                    foreach (string field in missingFields)
                    {
                        results[2].Errors.Add($"Field not found: {field}");
                    }
                }

                //
                // Check table for missing toolbar fields
                //
                // List of required fields to check
                List<string> toolbarRequiredFields = new List<string>() { "relativeage", "hexcolor" };

                // Get misssing required fields
                List<string> toolbarMissingFields = await AnyStandaloneTable.GetMissingFieldsAsync("DescriptionOfMapUnits", toolbarRequiredFields);
                if (toolbarMissingFields.Count == 0)
                {
                    results[3].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[3].Status = ValidationStatus.Failed;
                    foreach (string field in toolbarMissingFields)
                    {
                        results[3].Errors.Add($"Field not found: {field}");
                    }
                }

                //
                // Check for duplicate mapunit values
                //
                List<string> duplicateMapUnits = await AnyStandaloneTable.GetDuplicateValuesInFieldAsync("DescriptionOfMapUnits", "MapUnit", "MapUnit IS NOT NULL");
                if (duplicateMapUnits.Count == 0)
                {
                    results[4].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[4].Status = ValidationStatus.Failed;
                    foreach (string duplicate in duplicateMapUnits)
                    {
                        results[4].Errors.Add($"Duplicate MapUnit value: {duplicate}");
                    }
                }

                //
                // Check for duplicate DescriptionOfMapUnits_ID values
                //
                List<string> duplicateIds = await AnyStandaloneTable.GetDuplicateValuesInFieldAsync("DescriptionOfMapUnits", "DescriptionOfMapUnits_ID");
                if (duplicateIds.Count == 0)
                {
                    results[5].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[5].Status = ValidationStatus.Failed;
                    foreach (string id in duplicateIds)
                    {
                        results[5].Errors.Add($"Duplicate DescriptionOfMapUnits_ID value: {id}");
                    }
                }

                //
                // Check for duplicate name values
                //
                List<string> duplicateNames = await AnyStandaloneTable.GetDuplicateValuesInFieldAsync("DescriptionOfMapUnits", "name");
                if (duplicateNames.Count == 0)
                {
                    results[6].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[6].Status = ValidationStatus.Failed;
                    foreach (string name in duplicateNames)
                    {
                        results[6].Errors.Add($"Duplicate Name value: {name}");
                    }
                }

                //
                // Check for duplicate fullname values
                //
                List<string> duplicateFullNames = await AnyStandaloneTable.GetDuplicateValuesInFieldAsync("DescriptionOfMapUnits", "fullname");
                if (duplicateFullNames.Count == 0)
                {
                    results[7].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[7].Status = ValidationStatus.Failed;
                    foreach (string fullName in duplicateFullNames)
                    {
                        results[7].Errors.Add($"Duplicate FullName value: {fullName}");
                    }
                }

                //
                // Check for duplicate rgb values
                //
                List<string> duplicateRGB = await AnyStandaloneTable.GetDuplicateValuesInFieldAsync("DescriptionOfMapUnits", "areafillrgb");
                if (duplicateRGB.Count == 0)
                {
                    results[8].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[8].Status = ValidationStatus.Failed;
                    foreach (string rgb in duplicateRGB)
                    {
                        results[8].Errors.Add($"Duplicate AreaFillRGB value: {rgb}");
                    }
                }

                //
                // Check for duplicate hierarchykey values
                //
                List<string> duplicateHierarchyKeys = await AnyStandaloneTable.GetDuplicateValuesInFieldAsync("DescriptionOfMapUnits", "hierarchykey");
                if (duplicateHierarchyKeys.Count == 0)
                {
                    results[9].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[9].Status = ValidationStatus.Failed;
                    foreach (string hkey in duplicateHierarchyKeys)
                    {
                        results[9].Errors.Add($"Duplicate HierarchyKey value: {hkey}");
                    }
                }

                //
                // Check for HierarchyKey values that don't fit in the tree 
                //
                // Get the tree and unassigned list
                Tuple<List<MapUnitTreeItem>, List<MapUnitTreeItem>> tuple = await Data.DescriptionOfMapUnits.GetHierarchyTreeAsync();

                // Filter out the null/empty HierarchyKeys from the list of unsassigned rows
                List<MapUnitTreeItem> unassignedNotNull = tuple.Item2.Where(a => !string.IsNullOrEmpty(a.HierarchyKey)).ToList();
                if (unassignedNotNull.Count == 0)
                {
                    results[10].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[10].Status = ValidationStatus.Failed;
                    foreach (MapUnitTreeItem row in unassignedNotNull)
                    {
                        results[10].Errors.Add($"Bad HierarchyKey: {row.HierarchyKey}");
                    }
                }

                //
                // Check for empty/null values in required fields for ALL DMU ROWS
                //
                // DMU fields that can't have nulls
                List<string> dmuNotNull = new List<string>() { "name", "hierarchykey", "paragraphstyle", "descriptionsourceid", "descriptionofmapunits_id" };

                // Get required fields with a null value
                List<string> fieldsWithMissingValues = await AnyStandaloneTable.GetRequiredFieldIsNullAsync("DescriptionOfMapUnits", dmuNotNull);
                if (fieldsWithMissingValues.Count == 0)
                {
                    results[11].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[11].Status = ValidationStatus.Failed;
                    foreach (string field in fieldsWithMissingValues)
                    {
                        results[11].Errors.Add($"Null value found in field: {field}");
                    }
                }

                //
                // Check for empty/null values in required fields for MAPUNIT dmu rows (not headings)
                //
                // List of fields that can't be null
                List<string> mapUnitNotNull = new List<string>() { "mapunit", "fullname", "age", "areafillrgb",
                                                              "geomaterial", "geomaterialconfidence" };

                // Get required fields with null values. Using the 'MapUnit is not null' where clause to only check MapUnit rows
                List<string> mapUnitfieldsWithMissingValues = await AnyStandaloneTable.GetRequiredFieldIsNullAsync("DescriptionOfMapUnits", mapUnitNotNull, "MapUnit IS NOT NULL");
                if (mapUnitfieldsWithMissingValues.Count == 0)
                {
                    results[12].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[12].Status = ValidationStatus.Failed;
                    foreach (string field in mapUnitfieldsWithMissingValues)
                    {
                        results[12].Errors.Add($"Null value found in MapUnit field: {field}");
                    }
                }

                //
                // Check for any MapUnits defined in DMU, but not used in MapUnitPolys
                //
                List<string> unusedDMU = await DescriptionOfMapUnits.GetUnusedMapUnitsAsync();
                if (unusedDMU.Count == 0)
                {
                    results[13].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[13].Status = ValidationStatus.Failed;
                    foreach (string mu in unusedDMU)
                    {
                        results[13].Errors.Add($"Unused MapUnit: {mu}");
                    }
                }

                //
                // GeoMaterial are defined in GeoMaterialDict
                //

                // Get missing GeoMaterials
                List<string> missingGeoMaterials = await DescriptionOfMapUnits.GetMissingGeoMaterialAsync();
                if (missingGeoMaterials.Count == 0)
                {
                    results[14].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[14].Status = ValidationStatus.Failed;
                    foreach (string missing in missingGeoMaterials)
                    {
                        results[14].Errors.Add($"GeoMaterial not defined: {missing}");
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Get undefined terms that must be defined in the Glossary
        /// </summary>
        /// <param name="definedTerms">List of defined terms in the glossary</param>
        /// <returns>List of missing glossary terms</returns>
        public static async Task<List<GlossaryTerm>> GetTermsUndefinedInGlossaryAsync(List<string> definedTerms)
        {
            List<GlossaryTerm> undefinedTerms = new List<GlossaryTerm>();

            List<string> ParagraphStyleTerms = await AnyStandaloneTable.GetDistinctValuesForFieldAsync("DescriptionOfMapUnits", "ParagraphStyle");

            IEnumerable<string> undefinedParagraphStyle = ParagraphStyleTerms.Except(definedTerms);

            foreach (string term in undefinedParagraphStyle)
            {
                undefinedTerms.Add(await PredefinedTerms.GetPrepopulatedDefinitionAsync("DescriptionOfMapUnits", "ParagraphStyle", term));
            }

            List<string> GeoMaterialConfidenceTerms = await AnyStandaloneTable.GetDistinctValuesForFieldAsync("DescriptionOfMapUnits", "GeoMaterialConfidence");

            IEnumerable<string> undefinedGeoMaterialConfidenceTerms = GeoMaterialConfidenceTerms.Except(definedTerms);

            foreach (string term in undefinedGeoMaterialConfidenceTerms)
            {
                undefinedTerms.Add(await PredefinedTerms.GetPrepopulatedDefinitionAsync("DescriptionOfMapUnits", "GeoMaterialConfidence", term));
            }

            return undefinedTerms;
        }

        /// <summary>
        /// Compare the MapUnitPolys layer with the DescriptionOfMapUnits table. Return any unused MapUnits from DescriptionOfMapUnits. 
        /// </summary>
        /// <returns>List of MapUnits not used</returns>
        public static async Task<List<string>> GetUnusedMapUnitsAsync()
        {
            List<string> mapUnitPolys = await AnyFeatureLayer.GetDistinctValuesForFieldAsync("MapUnitPolys", "MapUnit");

            List<string> mapUnitDescriptions = await AnyStandaloneTable.GetDistinctValuesForFieldAsync("DescriptionOfMapUnits", "MapUnit");

            return mapUnitDescriptions.Except(mapUnitPolys).ToList();
        }

        /// <summary>
        /// Check for missing GeoMaterials in the GeoMaterialDict
        /// </summary>
        /// <returns>Returns list of missing GeoMaterials</returns>
        public static async Task<List<string>> GetMissingGeoMaterialAsync()
        {
            List<string> missingGeoMaterial = new List<string>();

            // Get the GeoMaterial values from DMU
            List<string> dmuGeoMaterials = await AnyStandaloneTable.GetDistinctValuesForFieldAsync("DescriptionOfMapUnits", "GeoMaterial");

            foreach (string geo in dmuGeoMaterials)
            {
                // Check GeoMaterialDict for value
                if (!GeoMaterialDict.GeoMaterialOptions.Any(a => a.GeoMaterial == geo))
                {
                    missingGeoMaterial.Add(geo);
                }
            }

            return missingGeoMaterial;
        }

        /// <summary>
        /// Get list of MapUnit (objects) from DMU
        /// </summary>
        /// <returns>List of MapUnits</returns>
        public static async Task<List<MapUnit>> GetMapUnitsAsync()
        {
            List<Field> dmuFields = await GetFieldsAsync();

            List<MapUnit> MapUnitList = new List<MapUnit>();

            await QueuedTask.Run(() =>
            {
                StandaloneTable dmu = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "DescriptionOfMapUnits");

                if (dmu == null)
                {
                    return;
                }

                using (Table table = dmu.GetTable())
                {
                    if (table != null)
                    {
                        using (RowCursor rowCursor = table.Search())
                        {
                            while (rowCursor.MoveNext())
                            {
                                using (Row row = rowCursor.Current)
                                {
                                    // Create map unit from row 
                                    MapUnit mapUnit = new MapUnit
                                    {
                                        ObjectID = Helpers.RowValueToString(row["ObjectID"]),
                                        MU = Helpers.RowValueToString(row["MapUnit"]),
                                        Name = Helpers.RowValueToString(row["Name"]),
                                        FullName = Helpers.RowValueToString(row["FullName"]),
                                        Age = Helpers.RowValueToString(row["Age"]),
                                        Description = Helpers.RowValueToString(row["Description"]),
                                        HierarchyKey = Helpers.RowValueToString(row["HierarchyKey"]),
                                        ParagraphStyle = Helpers.RowValueToString(row["ParagraphStyle"]),
                                        Label = row["Label"]?.ToString() ?? null,//Helpers.RowValueToString(row["Label"]),
                                        AreaFillRGB = Helpers.RowValueToString(row["AreaFillRGB"]),
                                        GeoMaterial = Helpers.RowValueToString(row["GeoMaterial"]),
                                        GeoMaterialConfidence = Helpers.RowValueToString(row["GeoMaterialConfidence"]),
                                        DescriptionSourceID = Helpers.RowValueToString(row["DescriptionSourceID"]),
                                    };

                                    mapUnit.HexColor = ColorConverter.RGBtoHex(mapUnit.AreaFillRGB);

                                    if (dmuFields.Any(a => a.Name == "RelativeAge"))
                                    {
                                        mapUnit.RelativeAge = Helpers.RowValueToString(row["RelativeAge"]);
                                    }

                                    if (dmuFields.Any(a => a.Name == "GeoMaterial"))
                                    {
                                        mapUnit.GeoMaterial = Helpers.RowValueToString(row["GeoMaterial"]);
                                    }

                                    if (dmuFields.Any(a => a.Name == "GeoMaterialConfidence"))
                                    {
                                        mapUnit.GeoMaterialConfidence = Helpers.RowValueToString(row["GeoMaterialConfidence"]);
                                    }

                                    // Add it to temp list
                                    MapUnitList.Add(mapUnit);
                                }
                            }
                        }
                    }
                }
            });

            return MapUnitList;
        }

        /// <summary>
        /// Get fields from the DescriptionOfMapUnits table
        /// </summary>
        /// <returns>List of fields</returns>
        public static async Task<List<Field>> GetFieldsAsync()
        {
            List<Field> dmuFields = null;

            await QueuedTask.Run(() =>
            {
                StandaloneTable dmu = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "DescriptionOfMapUnits");

                if (dmu != null)
                {
                    using (Table table = dmu.GetTable())
                    {
                        if (table != null)
                        {
                            dmuFields = table.GetDefinition()?.GetFields()?.ToList();
                        }
                    }
                }
            });

            return dmuFields;
        }

        /// <summary>
        /// Build a tree using HierarchyKeys from the DMU rows 
        /// </summary>
        /// <returns>Returns a tuple with the built tree and unassigned units</returns>
        public static async Task<Tuple<List<MapUnitTreeItem>, List<MapUnitTreeItem>>> GetHierarchyTreeAsync()
        {
            // Temp list for unassigned DMUS
            List<MapUnitTreeItem> tmpUnassigned = new List<MapUnitTreeItem>();

            // Temp list for the tree
            List<MapUnitTreeItem> tmpTree = new List<MapUnitTreeItem>();

            // Create a mapper for converting MapUnits to MapUnitTreeItems
            TinyMapper.Bind<MapUnit, MapUnitTreeItem>();

            // Get all the rows
            List<MapUnit> DMUs = await GetMapUnitsAsync();

            // Order DMUs by HierarchyKey length then by HierarchyKey so we process children before their parents
            // Convert MapUnit to MapUnitTreeItem
            List<MapUnitTreeItem> hierarchyList = DMUs.OrderBy(a => a.HierarchyKey.Length).ThenBy(a => a.HierarchyKey).Select(a => TinyMapper.Map<MapUnitTreeItem>(a)).ToList();

            // Loop over the DMUs
            foreach (MapUnitTreeItem mu in hierarchyList)
            {
                // Check that the HKey is made up of digits and a dash.
                if (!mu.HierarchyKey.All(c => char.IsDigit(c) || c == '-'))
                {
                    // Add to the unassigned list.
                    tmpUnassigned.Add(mu);
                }
                // Check the HierarchyKey string for a dash
                // Children will always have a dash (001-001 for example)
                else if (mu.HierarchyKey.IndexOf("-") != -1)
                {
                    // Remove the last dash and last index to find their parent's HierarchyKey (002-001 becomes 002)
                    string parentHierarchyKey = mu.HierarchyKey.Substring(0, mu.HierarchyKey.LastIndexOf("-"));

                    // Look for a map unit that matches the parent HierarchyKey
                    MapUnitTreeItem parent = hierarchyList.FirstOrDefault(a => a.HierarchyKey == parentHierarchyKey);

                    if (parent == null)
                    {
                        // Parent not found. Add to the unassigned list.
                        tmpUnassigned.Add(mu);
                    }
                    else
                    {
                        // Add child to parent
                        parent.Children.Add(mu);
                    }
                }
                else
                {
                    // Check if the HierarchyKey is empty
                    if (string.IsNullOrWhiteSpace(mu.HierarchyKey))
                    {
                        // Add to the unassigned list.
                        tmpUnassigned.Add(mu);
                    }
                    else
                    {
                        // Map Unit must be a root node
                        tmpTree.Add(mu);
                    }
                }
            }

            // Sort unassigned
            tmpUnassigned = tmpUnassigned.OrderBy(a => !string.IsNullOrEmpty(a.MU)).ThenBy(b => b.Name).ToList();

            // Combine the lists into a single tuple
            return Tuple.Create(tmpTree, tmpUnassigned);
        }

        /// <summary>
        /// Update the HierarchyKey values to be zero-padded. Example, 1.1.1 => 001.001.001
        /// </summary>
        /// <returns>Number of rows updated</returns>
        public static async Task<int> ZeroPadHierarchyKeyValues()
        {
            int count = 0;

            StandaloneTable standaloneTable = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "DescriptionOfMapUnits");

            if (standaloneTable == null)
            {
                return 0;
            }

            await QueuedTask.Run(() =>
            {
                using (Table table = standaloneTable.GetTable())
                {
                    if (table != null)
                    {
                        QueryFilter queryFilter = new QueryFilter
                        {
                            SubFields = "hierarchykey",
                            WhereClause = "hierarchykey <> ''"
                        };

                        EditOperation editOperation = new EditOperation()
                        {
                            Name = "Zero-Pad DescriptionOfMapUnits Hierarchy Key Values",
                            ProgressMessage = "Updating DescriptionOfMapUnits",
                            ShowProgressor = true
                        };

                        editOperation.Callback(context =>
                        {
                            using (RowCursor rowCursor = table.Search(queryFilter, false))
                            {
                                while (rowCursor.MoveNext())
                                {
                                    using (Row row = rowCursor.Current)
                                    {
                                        string originalKey = row["hierarchykey"]?.ToString();

                                        // Replace semi-colons
                                        originalKey = originalKey.Replace(';','-');

                                        // Replace periods
                                        originalKey = originalKey.Replace('.', '-');

                                        // Original symbol must be up of only digits and dashes
                                        if (originalKey.All(c => char.IsDigit(c) || c == '-'))
                                        {
                                            string[] splitKeys = originalKey.Split('-');

                                            for (int i = 0; i < splitKeys.Length; i++)
                                            {
                                                // Parse the int
                                                if (int.TryParse(splitKeys[i], out int value))
                                                {
                                                    // Zero-pad 
                                                    splitKeys[i] = value.ToString("000");
                                                };
                                            }

                                            // Combine the zero-padded numbers
                                            string paddedKey = string.Join("-", splitKeys);

                                            if (originalKey != paddedKey)
                                            {
                                                count++;

                                                // In order to update the Map and/or the attribute table.
                                                // Has to be called before any changes are made to the row.
                                                context.Invalidate(row);

                                                row["hierarchykey"] = paddedKey;

                                                // After all the changes are done, persist it.
                                                row.Store();

                                                // Has to be called after the store too.
                                                context.Invalidate(row);
                                            }
                                        }
                                    }
                                }
                            }
                        }, table);

                        bool result = editOperation.Execute();
                    }
                }
            });

            return count;
        }
    }
}
