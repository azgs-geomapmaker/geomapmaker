using ArcGIS.Core.Data;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public static class DescriptionOfMapUnits
    {
        // All headings and map units
        public static List<MapUnit> DMUs { get; set; } = new List<MapUnit>();

        // Returns only headings
        public static List<MapUnit> Headings => DMUs.Where(a => a.ParagraphStyle == "Heading").OrderBy(a => a.Name).ToList();

        // Returns only map units
        public static List<MapUnit> MapUnits => DMUs.Where(a => a.ParagraphStyle == "Standard").OrderBy(a => a.MU).ToList();

        // Convert row object to a string. 
        private static string RowValueToString(object value)
        {
            // Nulls are converted to an empty string
            return value == null ? "" : value.ToString();
        }

        // Refresh Map Units. Might need to refresh everytime in the Getter instead..
        public static async Task RefreshMapUnitsAsync()
        {
            if (DbConnectionProperties.GetProperties() == null)
            {
                return;
            }

            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                List<MapUnit> MapUnitsList = new List<MapUnit>();

                using (Geodatabase geodatabase = new Geodatabase(DbConnectionProperties.GetProperties()))
                {
                    QueryDef mapUnitsQDef = new QueryDef
                    {
                        Tables = "DescriptionOfMapUnits",
                    };

                    using (RowCursor rowCursor = geodatabase.Evaluate(mapUnitsQDef, false))
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
                                    RelativeAge = RowValueToString(row["RelativeAge"]),
                                    Description = RowValueToString(row["Description"]),
                                    HierarchyKey = RowValueToString(row["HierarchyKey"]),
                                    ParagraphStyle = RowValueToString(row["ParagraphStyle"]),
                                    Label = RowValueToString(row["Label"]),
                                    HexColor = RowValueToString(row["Hexcolor"]),
                                    DescriptionSourceID = RowValueToString(row["DescriptionSourceID"]),
                                    GeoMaterial = RowValueToString(row["GeoMaterial"]),
                                    GeoMaterialConfidence = RowValueToString(row["GeoMaterialConfidence"]),
                                    ParentId = (int?)row["ParentId"]
                                };

                                // Add it to temp list
                                MapUnitsList.Add(mapUnit);
                            }
                        }
                    }
                }

                DMUs = MapUnitsList;
            });
        }
    }
}
