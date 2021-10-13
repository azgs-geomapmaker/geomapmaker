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
                                    MU = row["MapUnit"]?.ToString(),
                                    Name = row["Name"].ToString(),
                                    FullName = row["FullName"]?.ToString(),
                                    Age = row["Age"]?.ToString(), //TODO: more formatting here
                                    //mapUnit.RelativeAge = row["RelativeAge"].ToString(); //TODO: this is column missing in the table right now
                                    Description = row["Description"]?.ToString(),
                                    HierarchyKey = row["HierarchyKey"]?.ToString(),
                                    ParagraphStyle = row["ParagraphStyle"]?.ToString(),
                                    //mapUnit.ParagraphStyle = JsonConvert.DeserializeObject<List<string>>(row["ParagraphStyle"].ToString());
                                    Label = row["Label"]?.ToString(),
                                    //mapUnit.Symbol = row["Symbol"].ToString();
                                    //mapUnit.AreaFillRGB = row["AreaFillRGB"].ToString(); //TODO: more formatting here
                                    Hexcolor = row["hexcolor"]?.ToString(),
                                    //mapUnit.Color = row["hexcolor"];
                                    DescriptionSourceID = row["DescriptionSourceID"]?.ToString(),
                                    GeoMaterial = row["GeoMaterial"]?.ToString(),
                                    GeoMaterialConfidence = row["GeoMaterialConfidence"]?.ToString(),
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
