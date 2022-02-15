using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Data;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
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

        private void ResetMapUnitPolygonsSymbology()
        {
            throw new NotImplementedException();
        }

        private async void ResetContactsFaultsSymbology()
        {
            // Check if the symbol list has been populated 
            if (CFSymbology.CFSymbolOptionsList == null)
            {
                await CFSymbology.RefreshCFSymbolOptions();
            }

            // Get the CF Symbology Options
            List<CFSymbol> SymbolOptions = CFSymbology.CFSymbolOptionsList;

            // CF Layer
            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "ContactsAndFaults");

            if (layer == null)
            {
                return;
            }

            var cimList = new List<CIMUniqueValueClass>();

            await QueuedTask.Run(async () =>
            {
                Table cfTable = layer.GetTable();

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
