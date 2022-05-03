using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Geomapmaker._helpers;
using ArcGIS.Desktop.Framework.Threading.Tasks;

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

            return await General.StandaloneTableFieldsExistAsync("DescriptionOfMapUnits", requiredFields);
        }

        /// <summary>
        /// Check the required fields for any missing values.
        /// </summary>
        /// <returns>Returns a list of fieldnames that contain a null/empty value</returns>
        public static async Task<List<string>> GetRequiredFieldsWithNullValues()
        {
            List<string> fieldsToCheck = new List<string>() { "name", "hierarchykey", "paragraphstyle", "descriptionsourceid", "descriptionofmapunits_id" };

            return await General.StandaloneTableRequiredFieldIsNullAsync("DescriptionOfMapUnits", fieldsToCheck);
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
            return await General.StandaloneTableRequiredFieldIsNullAsync("DescriptionOfMapUnits", fieldsToCheck, "MapUnit IS NOT NULL");
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
        /// <returns>List of duplicate valuessss</returns>
        public static async Task<List<string>> GetDuplicateMapUnitsAsync()
        {
            // return duplicate map units 
            return await General.StandaloneTableFindDuplicateValuesInFieldAsync("DescriptionOfMapUnits", "MapUnit", "MapUnit IS NOT NULL");
        }

        /// <summary>
        /// Get duplicate DescriptionOfMapUnits_ID from DMU table
        /// </summary>
        /// <returns>List of any duplicate DescriptionOfMapUnits_ID in the DMU table</returns>
        public static async Task<List<string>> GetDuplicateIdsAsync()
        {
            // return duplicate ids
            return await General.StandaloneTableFindDuplicateValuesInFieldAsync("DescriptionOfMapUnits", "DescriptionOfMapUnits_ID");
        }

        /// <summary>s
        /// Get all MapUnits string value from DMU table
        /// </summary>
        /// <returns>List of MapUnits (string)</returns>
        public static async Task<List<string>> GetAllMapUnitValuesAsync()
        {
            List<string> mapUnits = new List<string>();

            StandaloneTable dmu = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "DescriptionOfMapUnits");

            if (dmu != null)
            {
                await QueuedTask.Run(() =>
                {
                    using (Table table = dmu.GetTable())
                    {
                        if (table != null)
                        {
                            QueryFilter queryFilter = new QueryFilter
                            {
                                SubFields = "MapUnit"
                            };

                            using (RowCursor rowCursor = table.Search(queryFilter))
                            {
                                while (rowCursor.MoveNext())
                                {
                                    using (Row row = rowCursor.Current)
                                    {
                                        if (row["MapUnit"] != null)
                                        {
                                            mapUnits.Add(row["MapUnit"]?.ToString());
                                        }
                                    }
                                }
                            }
                        }
                    }
                });
            }

            return mapUnits;
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
                        dmuFields = table.GetDefinition().GetFields().ToList();
                    }
                }
            });

            return dmuFields;
        }
    }
}
