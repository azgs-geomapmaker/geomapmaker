using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Geomapmaker.Data
{
    public class Symbology
    {
        public static List<GemsSymbol> CFSymbolOptionsList;

        public static List<GemsSymbol> OrientationPointSymbols;

        public static async Task RefreshCFSymbolOptions()
        {
            List<GemsSymbol> cfSymbols = new List<GemsSymbol>();

            StandaloneTable SymbologyTable = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "Symbology");

            // Return an empty list if the cfsymbology table isn null
            if (SymbologyTable == null)
            {
                MessageBox.Show("Symbology table not found.");
                CFSymbolOptionsList = cfSymbols;
                return;
            }

            // Process the cfsymbology table
            await QueuedTask.Run(() =>
            {
                using (Table enterpriseTable = SymbologyTable.GetTable())
                {
                    if (enterpriseTable == null)
                    {
                        MessageBox.Show("Symbology table not found.");
                        CFSymbolOptionsList = cfSymbols;
                        return;
                    }

                    QueryFilter queryFilter = new QueryFilter
                    {
                        WhereClause = "TYPE = 'Line'",
                        PostfixClause = "ORDER BY key_"
                    };
                    using (RowCursor rowCursor = enterpriseTable.Search(queryFilter))
                    {
                        while (rowCursor.MoveNext())
                        {
                            using (Row row = rowCursor.Current)
                            {
                                GemsSymbol cfS = new GemsSymbol
                                {
                                    Key = row["key"].ToString(),
                                    Description = _helpers.Helpers.RowValueToString(row["description"]),
                                    SymbolJson = row["symbol"].ToString()
                                };

                                try
                                {
                                    // Create the preview image used in the ComboBox
                                    SymbolStyleItem sSI = new SymbolStyleItem()
                                    {
                                        Symbol = CIMSymbolReference.FromJson(cfS.SymbolJson).Symbol,
                                        PatchWidth = 250,
                                        PatchHeight = 25,
                                        SymbolPatchType = SymbolPatchType.HorizontalLine
                                    };
                                    cfS.Preview = sSI.PreviewImage;

                                    // Add to list
                                    cfSymbols.Add(cfS);
                                }
                                catch
                                {
                                    // Invalid CIM Symbol JSON
                                    Debug.WriteLine("Error prrocessing CIM Symbol JSON");
                                }
                            }
                        }
                    }
                }
            });

            CFSymbolOptionsList = cfSymbols;
        }

        public static async Task RefreshOPSymbolOptions()
        {
            List<GemsSymbol> orientationSymbols = new List<GemsSymbol>();

            StandaloneTable orientationSymbologyTable = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "Symbology");

            // Check if the table exists
            if (orientationSymbologyTable == null)
            {
                MessageBox.Show("Symbology table not found.");
                OrientationPointSymbols = orientationSymbols;
                return;
            }

            // Process the cfsymbology table
            await QueuedTask.Run(() =>
            {
                Table enterpriseTable = orientationSymbologyTable.GetTable();

                QueryFilter queryFilter = new QueryFilter
                {
                    WhereClause = "TYPE = 'Point'",
                    PostfixClause = "ORDER BY key_"
                };

                using (RowCursor rowCursor = enterpriseTable.Search(queryFilter))
                {
                    while (rowCursor.MoveNext())
                    {
                        using (Row row = rowCursor.Current)
                        {
                            GemsSymbol symbol = new GemsSymbol
                            {
                                Key = row["key"].ToString(),
                                Description = _helpers.Helpers.RowValueToString(row["description"]),
                                SymbolJson = row["symbol"].ToString()
                            };

                            try
                            {
                                // Create the preview image used in the ComboBox
                                SymbolStyleItem sSI = new SymbolStyleItem()
                                {
                                    Symbol = CIMSymbolReference.FromJson(symbol.SymbolJson).Symbol,
                                    PatchWidth = 25,
                                    PatchHeight = 25,
                                    SymbolPatchType = SymbolPatchType.Default
                                };
                                symbol.Preview = sSI.PreviewImage;

                                // Add to list
                                orientationSymbols.Add(symbol);
                            }
                            catch
                            {
                                // Invalid CIM Symbol JSON
                                Debug.WriteLine("Error prrocessing CIM Symbol JSON");
                            }
                        }
                    }
                }
            });

            OrientationPointSymbols = orientationSymbols;
        }
    }
}
