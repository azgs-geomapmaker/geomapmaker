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
        /// Check for missing GeoMaterials in the GeoMaterialDict
        /// </summary>
        /// <returns>Returns list of missing GeoMaterials</returns>
        public static async Task<List<string>> GetMissingGeoMaterialAsync()
        {
            List<string> missingGeoMaterial = new List<string>();

            // Get the GeoMaterial values from DMU
            List<string> dmuGeoMaterials = await General.StandaloneTableGetDistinctValuesForFieldAsync("DescriptionOfMapUnits", "GeoMaterial");

            foreach (string geo in dmuGeoMaterials)
            {
                // Check GeoMaterialDict for value
                if (!GeoMaterialDict.GeoMaterialOptions.Any(a => a.IndentedName == geo))
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
            tmpUnassigned = tmpUnassigned.OrderBy(a => a.ParagraphStyle).ThenBy(a => a.FullName).ToList();

            // Combine the lists into a single tuple
            return Tuple.Create(tmpTree, tmpUnassigned);
        }
    }
}
