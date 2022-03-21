using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public class OrientationPoints
    {
        public static List<GemsSymbol> OrientationPointSymbols;

        public static async Task RefreshOPSymbolOptions()
        {
            List<GemsSymbol> orientationSymbols = new List<GemsSymbol>();

            StandaloneTable orientationSymbologyTable = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "orientationsymbology");

            // Check if the table exists
            if (orientationSymbologyTable == null)
            {
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

                                var json = row["symbol"].ToString();

                                // Wrap the symbol JSON in CIMSymbolReference, so we can use that class to deserialize it.
                                json = json.Insert(0, "{\"type\": \"CIMSymbolReference\", \"symbol\": ");
                                json = json.Insert(json.Length, "}");

                                GemsSymbol newSymbol = new GemsSymbol
                                {
                                    Key = row["key"].ToString(),
                                    Description = row["description"]?.ToString(),
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

        public static async void RebuildOrientationPointsSymbology()
        {
            FeatureLayer opLayer = MapView.Active?.Map.FindLayers("OrientationPoints").FirstOrDefault() as FeatureLayer;

            if (opLayer == null)
            {
                return;
            }

            // Check if the symbol list has been populated 
            if (OrientationPointSymbols == null)
            {
                await RefreshOPSymbolOptions();
            }

            ProgressorSource ps = new ProgressorSource("Rebuilding Orientation Points Symbology...");

            await QueuedTask.Run(() =>
            {
                Table opTable = opLayer.GetTable();

                QueryFilter queryFilter = new QueryFilter
                {
                    PrefixClause = "DISTINCT",
                    PostfixClause = "ORDER BY symbol",
                    SubFields = "symbol"
                };

                using (RowCursor rowCursor = opTable.Search(queryFilter))
                {
                    while (rowCursor.MoveNext())
                    {
                        using (Row row = rowCursor.Current)
                        {
                            string cfSymbolKey = row["symbol"]?.ToString();

                            GemsSymbol Symbol = OrientationPointSymbols.FirstOrDefault(a => a.Key == cfSymbolKey);

                            if (Symbol != null)
                            {
                                // Add symbology for existing CF polylines
                                AddSymbolToRenderer(Symbol.Key, Symbol.SymbolJson);
                            }
                        }
                    }
                }

                OperationManager opManager = MapView.Active.Map.OperationManager;

                List<Operation> mapUnitPolyLayerUndos = opManager.FindUndoOperations(a => a.Name == "Update layer renderer: OrientationPoints");
                foreach (Operation undoOp in mapUnitPolyLayerUndos)
                {
                    opManager.RemoveUndoOperation(undoOp);
                }

            }, ps.Progressor);
        }

        public static async void AddSymbolToRenderer(string key, string symbolJson)
        {
            // Find the OrientationPoints layer
            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "OrientationPoints");

            if (layer == null)
            {
                return;
            }

            await QueuedTask.Run(() =>
            {
                //
                // Update Renderer
                //

                CIMUniqueValueRenderer layerRenderer = layer.GetRenderer() as CIMUniqueValueRenderer;

                CIMUniqueValueGroup layerGroup = layerRenderer?.Groups?.FirstOrDefault();

                List<CIMUniqueValueClass> listUniqueValueClasses = layerGroup == null ? new List<CIMUniqueValueClass>() : new List<CIMUniqueValueClass>(layerGroup.Classes);

                // Check if the renderer already has symbology for that key
                if (listUniqueValueClasses.Any(a => a.Label == key))
                {
                    return;
                }

                CIMUniqueValue[] listUniqueValues = new CIMUniqueValue[] {
                        new CIMUniqueValue {
                            FieldValues = new string[] { key }
                        }
                    };

                CIMUniqueValueClass uniqueValueClass = new CIMUniqueValueClass
                {
                    Editable = false,
                    Label = key,
                    Description = key,
                    Patch = PatchShape.Point,
                    Symbol = CIMSymbolReference.FromJson(symbolJson, null),
                    Visible = true,
                    Values = listUniqueValues,
                };
                listUniqueValueClasses.Add(uniqueValueClass);

                CIMUniqueValueGroup uvg = new CIMUniqueValueGroup
                {
                    Classes = listUniqueValueClasses.ToArray(),
                };
                CIMUniqueValueGroup[] listUniqueValueGroups = new CIMUniqueValueGroup[] { uvg };

                CIMUniqueValueRenderer updatedRenderer = new CIMUniqueValueRenderer
                {
                    UseDefaultSymbol = false,
                    Groups = listUniqueValueGroups,
                    Fields = new string[] { "symbol" }
                };

                layer.SetRenderer(updatedRenderer);
            });
        }

    }
}
