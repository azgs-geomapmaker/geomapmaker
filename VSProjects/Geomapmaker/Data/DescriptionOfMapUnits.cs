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
        public static bool DmuTableExists()
        {
            StandaloneTable table = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "DescriptionOfMapUnits");

            return table != null;
        }

        public static async Task<List<string>> GetDuplicateMapUnitsAsync()
        {
            List<string> mapUnits = await GetAllMapUnitValuesAsync();

            // Return duplicates
            return mapUnits.GroupBy(a => a).Where(b => b.Count() > 1).Select(c => c.Key).ToList();
        }

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
                });
            }

            return mapUnits;
        }

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

                Table enterpriseTable = dmu.GetTable();

                using (RowCursor rowCursor = enterpriseTable.Search())
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

                            mapUnit.HexColor = _helpers.ColorConverter.RGBtoHex(mapUnit.AreaFillRGB);

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
            });

            return MapUnitList;
        }

        public static async Task<List<Field>> GetFieldsAsync()
        {
            List<Field> dmuFields = null;

            await QueuedTask.Run(() =>
            {
                StandaloneTable dmu = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "DescriptionOfMapUnits");

                if (dmu != null)
                {
                    Table enterpriseTable = dmu.GetTable();

                    dmuFields = enterpriseTable.GetDefinition().GetFields().ToList();
                }
            });

            return dmuFields;
        }
    }
}
