using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public class OrientationPoints
    {
        /// <summary>
        /// Check if the OrientationPoints layer exists in Active Map
        /// </summary>
        /// <returns>Returns true if layer exists</returns>
        public static async Task<bool> FeatureLayerExistsAsync()
        {
            return await General.FeatureLayerExistsAsync("OrientationPoints");
        }

        /// <summary>
        /// Check the layer for any missing fieldss
        /// </summary>
        /// <returns>Returns a list of fieldnames missing from the layer</returns>
        public static async Task<List<string>> GetMissingFieldsAsync()
        {
            // List of fields to check for
            List<string> requiredFields = new List<string>() { "type", "azimuth", "inclination", "symbol", "label", "locationconfidencemeters",
                "identityconfidence", "orientationconfidencedegrees", "plotatscale", "stationsid", "mapunit", "locationsourceid",
                "orientationsourceid", "notes", "orientationpoints_id" };

            return await General.FeatureLayerGetMissingFieldsAsync("OrientationPoints", requiredFields);
        }

        /// <summary>
        /// Check the required fields for any missing values.
        /// </summary>
        /// <returns>Returns a list of fieldnames that contain a null/empty value</returns>
        public static async Task<List<string>> GetRequiredFieldsWithNullValues()
        {
            // List of fields to check for null values
            List<string> fieldsToCheck = new List<string>() { "type", "azimuth", "inclination", "locationconfidencemeters", "identityconfidence", "orientationconfidencedegrees", "plotatscale", "mapunit", "locationsourceid", "orientationsourceid", "orientationpoints_id" };

            return await General.FeatureLayerGetRequiredFieldIsNullAsync("OrientationPoints", fieldsToCheck);
        }

        /// <summary>
        /// Get duplicate ContactsAndFaults_ID
        /// </summary>
        /// <returns>List of any duplicate ContactsAndFaults_ID</returns>
        public static async Task<List<string>> GetDuplicateIdsAsync()
        {
            // return duplicate ids
            return await General.FeatureLayerGetDuplicateValuesInFieldAsync("OrientationPoints", "orientationpoints_id");
        }

        /// <summary>
        /// Get a list of unique, non-null values for the field LocationSourceID in the OrientationPoints layer
        /// </summary>
        /// <returns>List of LocationSourceID values</returns>
        public static async Task<List<string>> GetDistinctLocationSourceIDValuesAsync()
        {
            return await General.FeatureLayerGetDistinctValuesForFieldAsync("OrientationPoints", "locationsourceid");
        }

        /// <summary>
        /// Get a list of unique, non-null values for the field OrientationSourceID in the OrientationPoints layer
        /// </summary>
        /// <returns>List of OrientationSourceID values</returns>
        public static async Task<List<string>> GetDistinctOrientationSourceIDValuesAsync()
        {
            return await General.FeatureLayerGetDistinctValuesForFieldAsync("OrientationPoints", "orientationsourceid");
        }

        /// <summary>
        /// Rebuild the symbols for the Orientation Points Layer
        /// </summary>
        public static async void RebuildOrientationPointsSymbology()
        {
            FeatureLayer opLayer = MapView.Active?.Map.FindLayers("OrientationPoints").FirstOrDefault() as FeatureLayer;

            if (opLayer == null)
            {
                return;
            }

            // Check if the symbol list has been populated 
            if (Symbology.OrientationPointSymbols == null)
            {
                await Symbology.RefreshOPSymbolOptionsAsync();
            }

            ProgressorSource ps = new ProgressorSource("Rebuilding Orientation Points Symbology...");

            await QueuedTask.Run(() =>
            {
                using (Table opTable = opLayer.GetTable())
                {
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

                                GemsSymbol Symbol = Symbology.OrientationPointSymbols.FirstOrDefault(a => a.Key == cfSymbolKey);

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
                }

            }, ps.Progressor);
        }

        /// <summary>
        /// Add symbolJson to the renderer for OrientationPoints
        /// </summary>
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
