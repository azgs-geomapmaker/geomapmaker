using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public class MapUnitPolys
    {

        public static async void RebuildMapUnitPolygonsSymbology()
        {
            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "MapUnitPolys");

            if (layer == null)
            {
                return;
            }

            ProgressorSource ps = new ProgressorSource("Rebuilding Map Unit Poly Symbology...");

            await QueuedTask.Run(async () =>
            {
                // Remove existing symbols
                layer.SetRenderer(null);

                // Get all DMUs
                List<MapUnit> DMU = await DescriptionOfMapUnits.GetMapUnitsAsync();

                foreach (MapUnit mu in DMU)
                {
                    // Add symbology for all the DMUs
                    await AddSymbolToRenderer(mu.MU, mu.RGB.Item1, mu.RGB.Item2, mu.RGB.Item3);
                }

                //
                // We only need to add symbology from DMU, not existing map units
                //

                //Table mupTable = layer.GetTable();

                //QueryFilter queryFilter = new QueryFilter
                //{
                //    PrefixClause = "DISTINCT",
                //    PostfixClause = "ORDER BY MapUnit",
                //    SubFields = "MapUnit"
                //};

                //using (RowCursor rowCursor = mupTable.Search(queryFilter))
                //{
                //    while (rowCursor.MoveNext())
                //    {
                //        using (Row row = rowCursor.Current)
                //        {
                //            string mapUnitKey = row["MapUnit"]?.ToString();

                //            MapUnit mapUnit = DMU.FirstOrDefault(a => a.MU == mapUnitKey);

                //            if (mapUnit != null)
                //            {
                //                await AddSymbolToRenderer(mapUnit.MU, mapUnit.RGB.Item1, mapUnit.RGB.Item2, mapUnit.RGB.Item3);
                //            }
                //        }
                //    }
                //}

            }, ps.Progressor);

        }

        public static async Task AddSymbolToRenderer(string key, double R, double G, double B)
        {
            // Find the ContactsFaults layer
            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "MapUnitPolys");

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


                CIMStroke outline = SymbolFactory.Instance.ConstructStroke(CIMColor.NoColor());

                CIMFill fill = SymbolFactory.Instance.ConstructSolidFill(ColorFactory.Instance.CreateRGBColor(R, G, B));

                CIMSymbolLayer[] symbolLayers = new CIMSymbolLayer[]
                {
                    outline,
                    fill
                };

                CIMPolygonSymbol polySymbol = new CIMPolygonSymbol()
                {
                    SymbolLayers = symbolLayers
                };

                CIMSymbolReference symbolRef = new CIMSymbolReference()
                {
                    Symbol = polySymbol
                };

                CIMUniqueValueClass uniqueValueClass = new CIMUniqueValueClass
                {
                    Editable = false,
                    Label = key,
                    Description = key,
                    Patch = PatchShape.AreaPolygon,
                    Symbol = symbolRef,
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
                    Fields = new string[] { "MapUnit" }
                };

                layer.SetRenderer(updatedRenderer);
            });
        }
    }
}
