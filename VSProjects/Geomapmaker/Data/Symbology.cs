using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker._helpers;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public class Symbology
    {
        // List of CF symbols
        public static List<GemsSymbol> ContactsAndFaultsSymbols;

        // List of OP symbols
        public static List<GemsSymbol> OrientationPointSymbols;

        /// <summary>
        /// Rebuild the CF symbols from the symbology table
        /// </summary>
        /// <returns>Returns async task</returns>
        public static async Task RefreshCFSymbolOptionsAsync()
        {
            List<GemsSymbol> cfSymbols = new List<GemsSymbol>();

            StandaloneTable SymbologyTable = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "Symbology");

            // Return an empty list if the cfsymbology table isn null
            if (SymbologyTable == null)
            {
                ContactsAndFaultsSymbols = cfSymbols;
                return;
            }

            // Process the cfsymbology table
            await QueuedTask.Run(() =>
            {
                using (Table enterpriseTable = SymbologyTable.GetTable())
                {
                    if (enterpriseTable != null)
                    {
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
                                        Key = Helpers.RowValueToString(row["key"]),
                                        Description = Helpers.RowValueToString(row["description"]),
                                        SymbolJson = Helpers.RowValueToString(row["symbol"])
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
                }
            });

            ContactsAndFaultsSymbols = cfSymbols;
        }

        /// <summary>
        /// Rebuild the OP symbols from the symbology table
        /// </summary>
        /// <returns>Returns async task</returns>
        public static async Task RefreshOPSymbolOptionsAsync()
        {
            List<GemsSymbol> orientationSymbols = new List<GemsSymbol>();

            StandaloneTable orientationSymbologyTable = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "Symbology");

            // Check if the table exists
            if (orientationSymbologyTable == null)
            {
                OrientationPointSymbols = orientationSymbols;
                return;
            }

            // Process the cfsymbology table
            await QueuedTask.Run(() =>
            {
                using (Table enterpriseTable = orientationSymbologyTable.GetTable())
                {
                    if (enterpriseTable != null)
                    {
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
                                        Key = Helpers.RowValueToString(row["key"]),
                                        Description = Helpers.RowValueToString(row["description"]),
                                        SymbolJson = Helpers.RowValueToString(row["symbol"])
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
                    }
                }
            });

            OrientationPointSymbols = orientationSymbols;
        }

        /// <summary>
        /// Check for any missing line symbols in the symbology table
        /// </summary>
        /// <returns>List of missing symbology keys</returns>
        public static async Task<List<string>> GetMissingContactsAndFaultsSymbologyAsync()
        {
            List<string> missingSymbol = new List<string>();

            // Check if the ContactsAndFaultsSymbols have been processed
            if (ContactsAndFaultsSymbols == null)
            {
                await RefreshCFSymbolOptionsAsync();
            }
            
            // Get the symbol values from the CF layer
            List<string> cfSymbolValues = await General.FeatureLayerGetDistinctValuesForFieldAsync("ContactsAndFaults", "symbol");

            // Loop over the CF symbols
            foreach (string symbol in cfSymbolValues)
            {
                // Check if symbology exists
                if (!ContactsAndFaultsSymbols.Any(a => a.Key == symbol))
                {
                    missingSymbol.Add(symbol);
                }
            }

            return missingSymbol;
        }

        /// <summary>
        /// Check for any missing point symbols in the symbology table
        /// </summary>
        /// <returns>List of missing symbology keys</returns>
        public static async Task<List<string>> GetMissingOrientationPointsSymbologyAsync()
        {
            List<string> missingSymbol = new List<string>();

            // Check if the OrientationPoints have been processed
            if (OrientationPointSymbols == null)
            {
                await RefreshOPSymbolOptionsAsync();
            }

            // Get the symbol values from the OP
            List<string> opSymbolValues = await General.FeatureLayerGetDistinctValuesForFieldAsync("OrientationPoints", "symbol");

            // Loop over the symbols
            foreach (string symbol in opSymbolValues)
            {
                // Check if symbology exists
                if (!OrientationPointSymbols.Any(a => a.Key == symbol))
                {
                    missingSymbol.Add(symbol);
                }
            }

            return missingSymbol;
        }
    }
}
