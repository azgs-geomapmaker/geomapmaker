using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Geomapmaker._helpers;

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

                            if (dmuFields.Any(a => a.Name == "RelativeAge"))
                            {
                                mapUnit.RelativeAge = Helpers.RowValueToString(row["RelativeAge"]);
                            }

                            if (dmuFields.Any(a => a.Name == "hexcolor"))
                            {
                                mapUnit.HexColor = Helpers.RowValueToString(row["HexColor"]);
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

        public static List<Field> Fields { get; set; }

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
