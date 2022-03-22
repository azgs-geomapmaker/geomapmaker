using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public class DescriptionOfMapUnits
    {
        public static async Task<List<MapUnit>> GetMapUnitsAsync()
        {
            List<Field> dmuFields = await GetFieldsAsync();

            List<MapUnit> MapUnitList = new List<MapUnit>();

            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
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
                                ID = int.Parse(row["ObjectID"].ToString()),
                                MU = RowValueToString(row["MapUnit"]),
                                Name = RowValueToString(row["Name"]),
                                FullName = RowValueToString(row["FullName"]),
                                Age = RowValueToString(row["Age"]),
                                Description = RowValueToString(row["Description"]),
                                HierarchyKey = RowValueToString(row["HierarchyKey"]),
                                ParagraphStyle = RowValueToString(row["ParagraphStyle"]),
                                Label = RowValueToString(row["Label"]),
                                AreaFillRGB = RowValueToString(row["AreaFillRGB"]),
                                GeoMaterial = RowValueToString(row["GeoMaterial"]),
                                GeoMaterialConfidence = RowValueToString(row["GeoMaterialConfidence"]),
                                DescriptionSourceID = RowValueToString(row["DescriptionSourceID"]),
                            };

                            if (dmuFields.Any(a => a.Name == "RelativeAge"))
                            {
                                mapUnit.RelativeAge = RowValueToString(row["RelativeAge"]);
                            }

                            if (dmuFields.Any(a => a.Name == "hexcolor"))
                            {
                                mapUnit.HexColor = RowValueToString(row["HexColor"]);
                            }

                            if (dmuFields.Any(a => a.Name == "GeoMaterial"))
                            {
                                mapUnit.GeoMaterial = RowValueToString(row["GeoMaterial"]);
                            }

                            if (dmuFields.Any(a => a.Name == "GeoMaterialConfidence"))
                            {
                                mapUnit.GeoMaterialConfidence = RowValueToString(row["GeoMaterialConfidence"]);
                            }

                            // Add it to temp list
                            MapUnitList.Add(mapUnit);
                        }
                    }
                }
            });

            return MapUnitList;
        }

        public static List<Field> Fields { get; set; }

        // Convert row object to a string. 
        private static string RowValueToString(object value)
        {
            // Nulls are converted to an empty string
            return value == null ? "" : value.ToString();
        }

        public static async Task<List<Field>> GetFieldsAsync()
        {
            List<Field> dmuFields = null;

            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
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
