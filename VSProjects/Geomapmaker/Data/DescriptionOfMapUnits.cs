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
        public static async Task<bool> DmuTableExistsAsync()
        {
            return await Validation.StandaloneTableExistsAsync("DescriptionOfMapUnits");
        }

        public static async Task<List<string>> GetUniqueDescriptionSourceIDValues()
        {
            List<string> DescriptionSourceIDs = new List<string>();

            StandaloneTable dmu = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "DescriptionOfMapUnits");

            if (dmu != null)
            {
                await QueuedTask.Run(() =>
                {
                    using (Table table = dmu.GetTable())
                    {
                        QueryFilter queryFilter = new QueryFilter
                        {
                            SubFields = "DescriptionSourceID"
                        };

                        using (RowCursor rowCursor = table.Search(queryFilter))
                        {
                            while (rowCursor.MoveNext())
                            {
                                using (Row row = rowCursor.Current)
                                {
                                    if (row["DescriptionSourceID"] != null)
                                    {
                                        string descriptionId = row["DescriptionSourceID"]?.ToString();

                                        if (!DescriptionSourceIDs.Contains(descriptionId))
                                        {
                                            DescriptionSourceIDs.Add(descriptionId);
                                        }
                                    }
                                }
                            }
                        }
                    }
                });
            }

            return DescriptionSourceIDs;
        }

        /// <summary>
        /// Get duplicate MapUnits from DMU table
        /// </summary>
        /// <returns>List of any duplicate MapUnits in the DMU table</returns>
        public static async Task<List<string>> GetDuplicateMapUnitsAsync()
        {
            List<string> mapUnits = await GetAllMapUnitValuesAsync();

            // Return duplicates
            return mapUnits.GroupBy(a => a).Where(b => b.Count() > 1).Select(c => c.Key).ToList();
        }

        /// <summary>
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
