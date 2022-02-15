using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Data;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Geomapmaker.RibbonElements
{
    internal class ResetRenderers : Button
    {
        protected override void OnClick()
        {
            ResetContactsFaultsSymbology();
            ResetMapUnitPolygonsSymbology();
        }

        private async void ResetMapUnitPolygonsSymbology()
        {
            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "MapUnitPolys");

            if (layer == null)
            {
                return;
            }

            List<MapUnit> DMU = await DescriptionOfMapUnits.GetMapUnitsAsync();

            await QueuedTask.Run(async () =>
           {
               Table mupTable = layer.GetTable();

               // Remove existing symbols
               layer.SetRenderer(null);

               QueryFilter queryFilter = new QueryFilter
               {
                   PrefixClause = "DISTINCT",
                   PostfixClause = "ORDER BY MapUnit",
                   SubFields = "MapUnit"
               };

               using (RowCursor rowCursor = mupTable.Search(queryFilter))
               {
                   while (rowCursor.MoveNext())
                   {
                       using (Row row = rowCursor.Current)
                       {
                           string mapUnitKey = row["MapUnit"]?.ToString();

                           MapUnit mapUnit = DMU.FirstOrDefault(a => a.MU == mapUnitKey);

                           if (mapUnit != null)
                           {
                               await MapUnitPolys.AddSymbolToRenderer(mapUnit.MU, mapUnit.RGB.Item1, mapUnit.RGB.Item2, mapUnit.RGB.Item3);
                           }
                       }
                   }
               }

           });

        }

        private async void ResetContactsFaultsSymbology()
        {
            // CF Layer
            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "ContactsAndFaults");

            if (layer == null)
            {
                return;
            }

            // Check if the symbol list has been populated 
            if (CFSymbology.CFSymbolOptionsList == null)
            {
                await CFSymbology.RefreshCFSymbolOptions();
            }

            // Get the CF Symbology Options
            List<CFSymbol> SymbolOptions = CFSymbology.CFSymbolOptionsList;

            await QueuedTask.Run(async () =>
            {
                Table cfTable = layer.GetTable();

                // Remove existing symbols
                layer.SetRenderer(null);

                QueryFilter queryFilter = new QueryFilter
                {
                    PrefixClause = "DISTINCT",
                    PostfixClause = "ORDER BY symbol",
                    SubFields = "symbol"
                };

                using (RowCursor rowCursor = cfTable.Search(queryFilter))
                {
                    while (rowCursor.MoveNext())
                    {
                        using (Row row = rowCursor.Current)
                        {
                            string cfSymbolKey = row["symbol"]?.ToString();

                            CFSymbol Symbol = SymbolOptions.FirstOrDefault(a => a.Key == cfSymbolKey);

                            if (Symbol != null)
                            {
                                await CFSymbology.AddSymbolToRenderer(Symbol.Key, Symbol.SymbolJson);
                            }
                        }
                    }
                }
            });
        }
    }
}
