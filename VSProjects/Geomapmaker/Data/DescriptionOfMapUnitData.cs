using ArcGIS.Core.Data;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public static class DescriptionOfMapUnitData
    {
        // All headings and map units
        public static List<MapUnit> AllDescriptionOfMapUnits { get; set; } = new List<MapUnit>();

        // Returns headings from all map units
        public static List<MapUnit> Headings => AllDescriptionOfMapUnits.Where(a => a.ParagraphStyle == "Heading").OrderBy(a => a.MU).ToList();

        // Returns map units from all map units
        public static List<MapUnit> MapUnits => AllDescriptionOfMapUnits.Where(a => a.ParagraphStyle == "Standard").OrderBy(a => a.MU).ToList();

        private static string RowValueToString(object value)
        {
            if (value == null)
            {
                return "";
            }
            else
            {
                return value.ToString();
            }

        }

        public static async Task RefreshMapUnitsAsync()
        {
            if (DbConnectionProperties.GetProperties() == null)
            {
                return;
            }

            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                List<MapUnit> tempMapUnits = new List<MapUnit>();

                using (Geodatabase geodatabase = new Geodatabase(DbConnectionProperties.GetProperties()))
                {
                    QueryDef mapUnitsQDef = new QueryDef
                    {
                        Tables = "DescriptionOfMapUnits",
                        PostfixClause = "order by objectid"
                    };

                    using (RowCursor rowCursor = geodatabase.Evaluate(mapUnitsQDef, false))
                    {
                        while (rowCursor.MoveNext())
                        {
                            using (Row row = rowCursor.Current)
                            {
                                // Create and load map unit
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
                                    Hexcolor = RowValueToString(row["Hexcolor"]),
                                    DescriptionSourceID = RowValueToString(row["DescriptionSourceID"]),
                                    GeoMaterial = RowValueToString(row["GeoMaterial"]),
                                    GeoMaterialConfidence = RowValueToString(row["GeoMaterialConfidence"]),
                                    ParentId = (int?)row["ParentId"]
                                };

                                // Add it to temp list
                                tempMapUnits.Add(mapUnit);
                            }
                        }
                    }
                }

                AllDescriptionOfMapUnits = tempMapUnits;
            });
        }
    }
}
