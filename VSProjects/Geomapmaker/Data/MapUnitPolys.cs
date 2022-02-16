using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Editing.Templates;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Linq;

namespace Geomapmaker.Data
{
    public class MapUnitPolys
    {
        public static async void RebuildMUPSymbologyAndTemplates()
        {
            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "MapUnitPolys");

            if (layer == null)
            {
                return;
            }

            ProgressorSource ps = new ProgressorSource("Rebuilding Map Unit Poly Symbology...");

            await QueuedTask.Run(async () =>
            {
                // Remove all existing symbols
                layer.SetRenderer(null);

                // Remove all templates
                foreach (EditingTemplate temp in layer.GetTemplates())
                {
                    layer.RemoveTemplate(temp);
                }

                // Get all DMUs
                List<MapUnit> DMU = await DescriptionOfMapUnits.GetMapUnitsAsync();

                // Init new list of Unique Value CIMs
                List<CIMUniqueValueClass> listUniqueValueClasses = new List<CIMUniqueValueClass>();

                foreach (MapUnit mu in DMU)
                {
                    //
                    // Update Renderer
                    //

                    // DMU's MapUnit field is the key
                    string key = mu.MU;

                    CIMUniqueValue[] listUniqueValues = new CIMUniqueValue[] {
                        new CIMUniqueValue {
                            FieldValues = new string[] { key }
                        }
                    };

                    CIMStroke outline = SymbolFactory.Instance.ConstructStroke(CIMColor.NoColor());

                    CIMFill fill = SymbolFactory.Instance.ConstructSolidFill(ColorFactory.Instance.CreateRGBColor(mu.RGB.Item1, mu.RGB.Item2, mu.RGB.Item3));

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

                    //
                    // Create Template
                    //

                    // load the schema
                    Inspector insp = new Inspector();
                    insp.LoadSchema(layer);

                    insp["MapUnit"] = mu.MU;
                    insp["DataSourceID"] = mu.DescriptionSourceID;

                    // Tags
                    string[] tags = new[] { "MapUnitPoly" };

                    string defaultTool = "esri_editing_ConstructPolygonsTool";

                    // Create CIM template 
                    EditingTemplate newTemplate = layer.CreateTemplate(mu.MU, mu.MU, insp, defaultTool, tags);
                }

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

            }, ps.Progressor);

        }
    }
}
