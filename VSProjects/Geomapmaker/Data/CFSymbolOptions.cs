﻿using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public class CFSymbolOptions
    {
        public static List<CFSymbol> CFSymbolOptionsList;

        public static async Task RefreshCFSymbolOptions()
        {
            List<CFSymbol> cfSymbols = new List<CFSymbol>();

            StandaloneTable CFSymbologyTable = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "cfsymbology");

            // Return an empty list if the cfsymbology table isn null
            if (CFSymbologyTable == null)
            {
                return;
            }

            // Process the cfsymbology table
            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
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
                            CFSymbol cfS = new CFSymbol
                            {
                                Key = row["key"].ToString(),
                                Description = row["description"] == null ? "" : row["description"].ToString(),
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
    }
}
