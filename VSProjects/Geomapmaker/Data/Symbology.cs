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
        //
        // TODO: Combine ContactsAndFaults and OrientationPoints into a single CSV
        //

        public static List<GemsSymbol> CFSymbolOptionsList;

        public static async Task RefreshCFSymbolOptions()
        {
            List<GemsSymbol> cfSymbols = new List<GemsSymbol>();

            StandaloneTable CFSymbologyTable = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "cfsymbology");

            // Return an empty list if the cfsymbology table isn null
            if (CFSymbologyTable == null)
            {
                MessageBox.Show("cfsymbology table not found.");
                return;
            }

            // Process the cfsymbology table
            await QueuedTask.Run(() =>
            {
                Table enterpriseTable = CFSymbologyTable.GetTable();

                QueryFilter queryFilter = new QueryFilter
                {
                    PostfixClause = "ORDER BY key"
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
                                    PatchHeight = 25
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

            });

            CFSymbolOptionsList = cfSymbols;
        }

        public static List<GemsSymbol> OrientationPointSymbols;

        public static async Task RefreshOPSymbolOptions()
        {
            List<GemsSymbol> orientationSymbols = new List<GemsSymbol>();

            StandaloneTable orientationSymbologyTable = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "orientationsymbology");

            // Check if the table exists
            if (orientationSymbologyTable == null)
            {
                OrientationPointSymbols = orientationSymbols;
                return;
            }

            // Process the cfsymbology table
            await QueuedTask.Run(() =>
            {
                Table enterpriseTable = orientationSymbologyTable.GetTable();

                QueryFilter queryFilter = new QueryFilter
                {
                    //PostfixClause = "ORDER BY key"
                };

                using (RowCursor rowCursor = enterpriseTable.Search(queryFilter))
                {
                    while (rowCursor.MoveNext())
                    {
                        using (Row row = rowCursor.Current)
                        {
                            if (row["key"] != null)
                            {

                                string json = row["symbol"].ToString();

                                // Wrap the symbol JSON in CIMSymbolReference, so we can use that class to deserialize it.
                                json = json.Insert(0, "{\"type\": \"CIMSymbolReference\", \"symbol\": ");
                                json = json.Insert(json.Length, "}");

                                GemsSymbol newSymbol = new GemsSymbol
                                {
                                    Key = row["key"].ToString(),
                                    Description = _helpers.Helpers.RowValueToString(row["description"]),
                                    SymbolJson = json
                                };

                                try
                                {
                                    // Create the preview image used in the ComboBox
                                    SymbolStyleItem sSI = new SymbolStyleItem()
                                    {
                                        Symbol = CIMSymbolReference.FromJson(newSymbol.SymbolJson).Symbol,
                                        PatchWidth = 25,
                                        PatchHeight = 25
                                    };
                                    newSymbol.Preview = sSI.PreviewImage;

                                    // Add to list
                                    orientationSymbols.Add(newSymbol);
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

            OrientationPointSymbols = orientationSymbols;
        }
    }
}
