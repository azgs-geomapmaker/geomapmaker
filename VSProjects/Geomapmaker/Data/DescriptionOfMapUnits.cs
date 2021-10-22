using ArcGIS.Core.Data;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public static class DescriptionOfMapUnits
    {
        public static List<Field> Fields { get; set; }

        // All headings and map units
        public static List<MapUnit> DMUs { get; set; } = new List<MapUnit>();

        // Returns only headings
        public static List<MapUnit> Headings => DMUs.Where(a => a.ParagraphStyle == "Heading").OrderBy(a => a.Name).ToList();

        // Returns only map units
        public static List<MapUnit> MapUnits => DMUs.Where(a => a.ParagraphStyle == "Standard").OrderBy(a => a.MU).ToList();

        // Map units in a tree structure based on HierarchyKey
        public static List<MapUnit> Tree;

        // Map units not in the tree
        public static List<MapUnit> Unassigned;

        // Convert row object to a string. 
        private static string RowValueToString(object value)
        {
            // Nulls are converted to an empty string
            return value == null ? "" : value.ToString();
        }

        public static void RefreshTreeStructure()
        {
            // Empty the tree
            Tree = new List<MapUnit>();
            // Set all DMUs as unassigned
            Unassigned = DMUs;

            BuildTree();
        }

        // Build the tree stucture by looping over the dmus
        private static void BuildTree()
        {
            // Temp list for unassigned DMUS
            List<MapUnit> tmpUnassigned = new List<MapUnit>();

            // Order DMUs by HierarchyKey length then by HierarchyKey so we always process children before parents 
            List<MapUnit> hierarchyList = Unassigned.OrderBy(a => a.HierarchyKey.Length).ThenBy(a => a.HierarchyKey).ToList();

            // Loop over the DMUs
            foreach (MapUnit mu in hierarchyList)
            {

                // Check the HierarchyKey string for a dash
                // Children will always have a dash (001-001 for example)
                if (mu.HierarchyKey.IndexOf("-") != -1)
                {
                    // Remove the last dash and last index to find their parent's HierarchyKey (001-001 becomes 001)
                    string parentHierarchyKey = mu.HierarchyKey.Substring(0, mu.HierarchyKey.LastIndexOf("-"));

                    // Look for a map unit that matches the parent HierarchyKey
                    MapUnit parent = hierarchyList.FirstOrDefault(a => a.HierarchyKey == parentHierarchyKey);

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
                        Tree.Add(mu);
                    }

                }
            }

            Unassigned = tmpUnassigned;
        }

        public static async Task GetFields()
        {
            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                using (Geodatabase geodatabase = new Geodatabase(DbConnectionProperties.GetProperties()))
                {
                    var dmuTableDefinition = geodatabase.GetDefinition<TableDefinition>("DescriptionOfMapUnits");

                    Fields = dmuTableDefinition.GetFields().ToList();
                }
            });
        }

        // Refresh Map Units
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
                                    AreaFillRGB = RowValueToString(row["AreaFillRGB"]),
                                    DescriptionSourceID = RowValueToString(row["DescriptionSourceID"]),
                                    GeoMaterial = RowValueToString(row["GeoMaterial"]),
                                    GeoMaterialConfidence = RowValueToString(row["GeoMaterialConfidence"]),
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
