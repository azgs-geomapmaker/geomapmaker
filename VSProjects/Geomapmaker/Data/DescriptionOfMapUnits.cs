using ArcGIS.Core.Data;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

        public static List<MapUnit> Tree;

        public static List<MapUnit> Unassigned;

        // Convert row object to a string. 
        private static string RowValueToString(object value)
        {
            // Nulls are converted to an empty string
            return value == null ? "" : value.ToString();
        }

        public static void RefreshTreeStructure()
        {
            Tree = new List<MapUnit>();
            Unassigned = DMUs;

            // Strip any leading zeros from all of the indexes
            foreach (MapUnit mu in Unassigned)
            {
                string[] indexes = mu.HierarchyKey.Split('-');

                foreach (var ind in indexes)
                {
                    // Remove leading-zeros. 0001=>1, 0000=>0, etc
                    ind.TrimStart('0').PadLeft(1, '0');
                }

                // Join indexes with a dash. 0001-0001-0001
                mu.HierarchyKey = string.Join("-", indexes);
            }

            BuildTree();
        }

        private static void BuildTree()
        {
            var tmpUnassigned = new List<MapUnit>();
            var hierarchyList = Unassigned.OrderBy(a => a.HierarchyKey.Length).ThenBy(a => a.HierarchyKey).ToList();

            foreach (MapUnit mu in hierarchyList)
            {

                if (mu.HierarchyKey.LastIndexOf("-") != -1)
                {
                    string parentHierarchyKey = mu.HierarchyKey.Substring(0, mu.HierarchyKey.LastIndexOf("-"));

                    MapUnit parent = hierarchyList.FirstOrDefault(a => a.HierarchyKey == parentHierarchyKey);

                    if (parent != null)
                    {
                        parent.Children.Add(mu);
                    }
                }
                else
                {

                    if (string.IsNullOrWhiteSpace(mu.HierarchyKey))
                    {
                        tmpUnassigned.Add(mu);
                    }
                    else
                    {
                        Tree.Add(mu);
                    }

                }
            }

            Unassigned = tmpUnassigned;
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

            RefreshTreeStructure();
        }
    }
}
