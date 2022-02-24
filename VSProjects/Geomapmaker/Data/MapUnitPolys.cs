using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Editing.Templates;
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
                // Get all DMUs
                List<MapUnit> allDMUs = await DescriptionOfMapUnits.GetMapUnitsAsync();

                List<MapUnit> StandardDMUs = allDMUs.Where(a => a.ParagraphStyle == "Standard").OrderBy(a => a.Name).ToList();

                // Remove all existing symbols
                layer.SetRenderer(layer.CreateRenderer(new SimpleRendererDefinition()));

                IEnumerable<EditingTemplate> mupTemplates = layer.GetTemplates();

                // Loop over templates
                foreach (EditingTemplate temp in mupTemplates)
                {
                    layer.RemoveTemplate(temp);
                }

                // Init new list of Unique Value CIMs
                List<CIMUniqueValueClass> listUniqueValueClasses = new List<CIMUniqueValueClass>();

                //
                // Update Renderer for Unassigned MUPs
                //

                string unassignedMUP = GeomapmakerModule.MUP_UnassignedTemplateName;

                CIMUniqueValue[] listUniqueValues = new CIMUniqueValue[] {
                        new CIMUniqueValue {
                            FieldValues = new string[] { unassignedMUP }
                        }
                };

                CIMStroke outline = SymbolFactory.Instance.ConstructStroke(CIMColor.NoColor());

                CIMColor color = CIMColor.CreateRGBColor(255, 0, 0);

                SimpleFillStyle fillStyle = SimpleFillStyle.DiagonalCross;

                CIMPolygonSymbol polySymbol = SymbolFactory.Instance.ConstructPolygonSymbol(color, fillStyle, outline);

                CIMSymbolReference symbolRef = new CIMSymbolReference()
                {
                    Symbol = polySymbol
                };

                CIMUniqueValueClass unassignedCIM = new CIMUniqueValueClass
                {
                    Editable = false,
                    Label = unassignedMUP,
                    Description = unassignedMUP,
                    Patch = PatchShape.Default,
                    Symbol = symbolRef,
                    Visible = true,
                    Values = listUniqueValues,
                };

                //
                // Create Template
                //

                // load the schema
                Inspector insp = new Inspector();
                insp.LoadSchema(layer);

                insp["MapUnit"] = unassignedMUP;
                insp["DataSourceID"] = unassignedMUP;
                insp["IdentityConfidence"] = unassignedMUP;
                insp["Label"] = null;
                insp["Symbol"] = null;
                insp["Notes"] = unassignedMUP;

                // Tags
                string[] tags = new[] { "MapUnitPoly" };

                // Remove all default tools. Users should be using the geomapmaker tool.
                var toolFilter = new[] {
                                         "esri_editing_SketchPolygonTool",
                                         "esri_editing_SketchAutoCompletePolygonTool",
                                         "esri_editing_SketchRightPolygonTool",
                                         "esri_editing_SketchCirclePolygonTool",
                                         "esri_editing_SketchRectanglePolygonTool",
                                         "esri_editing_SketchRegularPolygonTool",
                                         "esri_editing_SketchEllipsePolygonTool",
                                         "esri_editing_SketchFreehandPolygonTool",
                                         "esri_editing_SketchAutoCompleteFreehandPolygonTool",
                                         "esri_editing_SketchTracePolygonTool",
                                         "esri_editing_SketchStreamPolygonTool",
                                         "esri_editing_SketchParcelSeedTool",
                                         "esri_defensemapping_createstructures",
                                         "esri_defensemapping_differencepolygon",
                };

                string defaultTool = "";

                // Create CIM template 
                EditingTemplate newTemplate = layer.CreateTemplate(unassignedMUP, unassignedMUP, insp, defaultTool, tags, toolFilter);

                //
                // LOOP OVER THE STANDARD DMUs
                //

                foreach (MapUnit mu in StandardDMUs)
                {
                    //
                    // Update Renderer
                    //

                    // DMU's MapUnit field is the key
                    string key = mu.MU;

                    listUniqueValues = new CIMUniqueValue[] {
                        new CIMUniqueValue {
                            FieldValues = new string[] { key }
                        }
                    };

                    CIMFill fill = SymbolFactory.Instance.ConstructSolidFill(ColorFactory.Instance.CreateRGBColor(mu.RGB.Item1, mu.RGB.Item2, mu.RGB.Item3));

                    CIMSymbolLayer[] symbolLayers = new CIMSymbolLayer[]
                    {
                    outline,
                    fill
                    };

                    polySymbol = new CIMPolygonSymbol()
                    {
                        SymbolLayers = symbolLayers
                    };

                    symbolRef = new CIMSymbolReference()
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
                    insp["MapUnit"] = mu.MU;
                    insp["DataSourceID"] = mu.DescriptionSourceID;
                    insp["IdentityConfidence"] = null;
                    insp["Label"] = null;
                    insp["Symbol"] = null;
                    insp["Notes"] = null;

                    // Create CIM template 
                    layer.CreateTemplate(mu.MU, mu.MU, insp, defaultTool, tags, toolFilter);
                }

                // Add unassigned renderer last
                listUniqueValueClasses.Add(unassignedCIM);

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

                OperationManager opManager = MapView.Active.Map.OperationManager;

                List<Operation> mapUnitPolyLayerUndos = opManager.FindUndoOperations(a => a.Name == "Update layer definition: MapUnitPolys" || a.Name == "Update layer renderer: MapUnitPolys");
                foreach (Operation undoOp in mapUnitPolyLayerUndos)
                {
                    opManager.RemoveUndoOperation(undoOp);
                }

                List<Operation> templateUndos = opManager.FindUndoOperations(a => a.Name == "New template" || a.Name == "Delete templates");
                foreach (Operation undoOp in templateUndos)
                {
                    opManager.RemoveUndoOperation(undoOp);
                }

            }, ps.Progressor);
        }

        public static async Task<List<MapUnitPolyTemplate>> GetMapUnitPolyTemplatesAsync()
        {
            // List of templates to return
            List<MapUnitPolyTemplate> mupTemplates = new List<MapUnitPolyTemplate>();

            IEnumerable<EditingTemplate> layerTemplates = new List<EditingTemplate>();

            // Find the ContactsFaults layer
            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "MapUnitPolys");

            if (layer == null)
            {
                return mupTemplates;
            }

            List<MapUnit> dmu = await DescriptionOfMapUnits.GetMapUnitsAsync();

            await QueuedTask.Run(() =>
            {
                // Get templates from CF layer
                layerTemplates = layer.GetTemplates();

                foreach (EditingTemplate template in layerTemplates)
                {
                    // Skip over the default template and Unassigned (for now)
                    if (template.Name != "MapUnitPolys" && template.Name != "Unassigned")
                    {
                        // Get CIMFeatureTemplate
                        CIMFeatureTemplate templateDef = template.GetDefinition() as CIMFeatureTemplate;

                        string muKey = templateDef.DefaultValues["mapunit"].ToString();

                        MapUnit mapUnit = dmu.Where(a => a.MU == muKey).FirstOrDefault();

                        MapUnitPolyTemplate tmpTemplate = new MapUnitPolyTemplate()
                        {
                            MapUnit = muKey,
                            HexColor = mapUnit.HexColor,
                            Tooltip = mapUnit.Tooltip,
                            DataSourceID = templateDef.DefaultValues["datasourceid"].ToString(),
                            Template = template
                        };

                        mupTemplates.Add(tmpTemplate);
                    }
                }

            });

            return mupTemplates;
        }
    }
}
