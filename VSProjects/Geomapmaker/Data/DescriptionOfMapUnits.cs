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

namespace Geomapmaker.Data
{
    public class DescriptionOfMapUnits
    {
        /// <summary>
        /// Check if the DescriptionOfMapUnits table exists
        /// </summary>
        /// <returns>True if the table exists</returns>
        public static async Task<bool> StandaloneTableExistsAsync()
        {
            return await General.StandaloneTableExistsAsync("DescriptionOfMapUnits");
        }

        /// <summary>
        /// Check the table for any missing fieldss
        /// </summary>
        /// <returns>Returns a list of fieldnames missing from the table</returns>
        public static async Task<List<string>> GetMissingFieldsAsync()
        {
            // List of fields to check for
            List<string> requiredFields = new List<string>() { "mapunit", "name", "fullname", "age", "description", "hierarchykey", "paragraphstyle", "label", "symbol", "areafillrgb",
                                                               "areafillpatterndescription", "descriptionsourceid", "geomaterial", "geomaterialconfidence", "descriptionofmapunits_id" };

            return await General.StandaloneTableGetMissingFieldsAsync("DescriptionOfMapUnits", requiredFields);
        }

        /// <summary>
        /// Check the required fields for any missing values.
        /// </summary>
        /// <returns>Returns a list of fieldnames that contain a null/empty value</returns>
        public static async Task<List<string>> GetRequiredFieldsWithNullValues()
        {
            List<string> fieldsToCheck = new List<string>() { "name", "hierarchykey", "paragraphstyle", "descriptionsourceid", "descriptionofmapunits_id" };

            return await General.StandaloneTableGetRequiredFieldIsNullAsync("DescriptionOfMapUnits", fieldsToCheck);
        }

        /// <summary>
        /// Check the required fields for MapUnits (Different requirements than headings)
        /// </summary>
        /// <returns>Returns a list of fieldnames that contain a null/empty value for MapUnits</returns>
        public static async Task<List<string>> GetMapUnitRequiredFieldsWithNullValues()
        {
            List<string> fieldsToCheck = new List<string>() { "mapunit", "fullname", "age", "areafillrgb",
                                                              "geomaterial", "geomaterialconfidence" };

            // Pass along the where clause to filter out heading DMU rows
            return await General.StandaloneTableGetRequiredFieldIsNullAsync("DescriptionOfMapUnits", fieldsToCheck, "MapUnit IS NOT NULL");
        }

        /// <summary>
        /// Get the unique, non-null DescriptionSourceID values from the DescriptionOfMapUnits table
        /// </summary> 
        /// <returns>Returns list of DescriptionSourceID values</returns>
        public static async Task<List<string>> GetUniqueDescriptionSourceIDValues()
        {
            return await General.StandaloneTableGetDistinctValuesForFieldAsync("DescriptionOfMapUnits", "DescriptionSourceID");
        }

        /// <summary>
        /// Get duplicate mapunit values in the DMU table
        /// </summary>
        /// <returns>List of duplicate values</returns>
        public static async Task<List<string>> GetDuplicateMapUnitsAsync()
        {
            // return duplicate map units 
            return await General.StandaloneTableGetDuplicateValuesInFieldAsync("DescriptionOfMapUnits", "MapUnit", "MapUnit IS NOT NULL");
        }

        /// <summary>
        /// Get duplicate name values in the DMU table
        /// </summary>
        /// <returns>List of duplicate values</returns>
        public static async Task<List<string>> GetDuplicateNamesAsync()
        {
            // return duplicates
            return await General.StandaloneTableGetDuplicateValuesInFieldAsync("DescriptionOfMapUnits", "name");
        }

        /// <summary>
        /// Get duplicate fullname values in the DMU table
        /// </summary>
        /// <returns>List of duplicate values</returns>
        public static async Task<List<string>> GetDuplicateFullNamesAsync()
        {
            // return duplicates
            return await General.StandaloneTableGetDuplicateValuesInFieldAsync("DescriptionOfMapUnits", "fullname");
        }

        /// <summary>
        /// Get duplicate rgb values in the DMU table
        /// </summary>
        /// <returns>List of duplicate values</returns>
        public static async Task<List<string>> GetDuplicateRGBAsync()
        {
            // return duplicates 
            return await General.StandaloneTableGetDuplicateValuesInFieldAsync("DescriptionOfMapUnits", "areafillrgb");
        }

        /// <summary>
        /// Get duplicate DescriptionOfMapUnits_ID from DMU table
        /// </summary>
        /// <returns>List of any duplicate DescriptionOfMapUnits_ID in the DMU table</returns>
        public static async Task<List<string>> GetDuplicateIdsAsync()
        {
            // return duplicate ids
            return await General.StandaloneTableGetDuplicateValuesInFieldAsync("DescriptionOfMapUnits", "DescriptionOfMapUnits_ID");
        }

        /// <summary>
        /// Compare the MapUnitPolys layer with the DescriptionOfMapUnits table. Return any unused MapUnits from DescriptionOfMapUnits. 
        /// </summary>
        /// <returns>List of MapUnits not used</returns>
        public static async Task<List<string>> GetUnusedMapUnitsAsync()
        {
            List<string> mapUnitPolys = await General.FeatureLayerGetDistinctValuesForFieldAsync("MapUnitPolys", "MapUnit");

            List<string> mapUnitDescriptions = await General.StandaloneTableGetDistinctValuesForFieldAsync("DescriptionOfMapUnits", "MapUnit");

            return mapUnitDescriptions.Except(mapUnitPolys).ToList();
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
                                        Label = Helpers.RowValueToString(row["Label"]),
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
                        dmuFields = table.GetDefinition()?.GetFields()?.ToList();
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
            tmpUnassigned = tmpUnassigned.OrderBy(a => a.ParagraphStyle).ThenBy(a => a.FullName).ToList();

            // Combine the lists into a single tuple
            return Tuple.Create(tmpTree, tmpUnassigned);
        }
    }
}
