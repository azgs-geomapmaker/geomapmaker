using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
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
        /// <summary>
        /// Check if the MapUnitPolys layer exists in Active Map
        /// </summary>
        /// <returns>Returns true if layer exists</returns>
        public static async Task<bool> FeatureLayerExistsAsync()
        {
            return await General.FeatureLayerExistsAsync("MapUnitPolys");
        }

        /// <summary>
        /// Get a list of unique, non-null values for the field DataSourceID in the MapUnitPolys layer
        /// </summary>
        /// <returns>List of DataSourceID values</returns>
        public static async Task<List<string>> GetDistinctDataSourceIDValuesAsync()
        {
            return await General.FeatureLayerGetDistinctValuesForFieldAsync("MapUnitPolys", "datasourceid");
        }

        /// <summary>
        /// Check the layer for any missing fieldss
        /// </summary>
        /// <returns>Returns a list of fieldnames missing from the layer</returns>
        public static async Task<List<string>> GetMissingFieldsAsync()
        {
            // List of fields to check for
            List<string> requiredFields = new List<string>() { "mapunit", "identityconfidence", "label", "symbol", "datasourceid", "notes",
                "mapunitpolys_id" };

            return await General.FeatureLayerGetMissingFieldsAsync("MapUnitPolys", requiredFields);
        }

        /// <summary>
        /// Check the required fields for any missing values.
        /// </summary>
        /// <returns>Returns a list of fieldnames that contain a null/empty value</returns>
        public static async Task<List<string>> GetRequiredFieldsWithNullValues()
        {
            // List of fields to check for null values
            List<string> fieldsToCheck = new List<string>() { "mapunit", "identityconfidence", "datasourceid", "mapunitpolys_id" };

            return await General.FeatureLayerGetRequiredFieldIsNullAsync("MapUnitPolys", fieldsToCheck);
        }

        /// <summary>
        /// Compare the MapUnitPolys layer with the DescriptionOfMapUnits table. Return any MapUnitPolys not defined in DescriptionOfMapUnits. 
        /// </summary>
        /// <returns>List of MapUnits not defined</returns>
        public static async Task<List<string>> GetMapUnitsNotDefinedInDMUTableAsync()
        {
            List<string> mapUnitPolys = await General.FeatureLayerGetDistinctValuesForFieldAsync("MapUnitPolys", "MapUnit");

            List<string> mapUnitDescriptions = await General.StandaloneTableGetDistinctValuesForFieldAsync("DescriptionOfMapUnits", "MapUnit");

            return mapUnitPolys.Except(mapUnitDescriptions).ToList();
        }

        /// <summary>
        /// Get duplicate MapUnitPolys_ID
        /// </summary>
        /// <returns>List of any duplicate MapUnitPolys_ID</returns>
        public static async Task<List<string>> GetDuplicateIdsAsync()
        {
            // return duplicate ids
            return await General.FeatureLayerGetDuplicateValuesInFieldAsync("MapUnitPolys", "MapUnitPolys_ID");
        }

        /// <summary>
        /// Rebuild the renderer for MapUnitPolys from the DMU table. Rebuild templates for the MUP layer from the DMU table
        /// </summary>
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
                    // Remove each template
                    layer.RemoveTemplate(temp);
                }

                // Init new list of Unique Value CIMs
                List<CIMUniqueValueClass> listUniqueValueClasses = new List<CIMUniqueValueClass>();

                //
                // Create Templates
                //

                // load the schema
                Inspector insp = new Inspector();
                insp.LoadSchema(layer);

                // Tags
                string[] tags = new[] { "MapUnitPoly" };

                // Remove all default tools. Users should be using the geomapmaker tool.
                string[] toolFilter = new[] {
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

                // No Color for polygon outine
                CIMStroke outline = SymbolFactory.Instance.ConstructStroke(CIMColor.NoColor());

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

                    CIMUniqueValue[] listUniqueValues = new CIMUniqueValue[] {
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
                    insp["MapUnit"] = mu.MU;
                    insp["DataSourceID"] = mu.DescriptionSourceID;
                    insp["IdentityConfidence"] = null;
                    insp["Label"] = null;
                    insp["Symbol"] = null;
                    insp["Notes"] = null;

                    // Create CIM template 
                    layer.CreateTemplate(mu.MU, mu.MU, insp, defaultTool, tags, toolFilter);
                }

                CIMUniqueValueGroup uvg = new CIMUniqueValueGroup
                {
                    Classes = listUniqueValueClasses.ToArray(),
                };
                CIMUniqueValueGroup[] listUniqueValueGroups = new CIMUniqueValueGroup[] { uvg };

                CIMUniqueValueRenderer updatedRenderer = new CIMUniqueValueRenderer
                {
                    UseDefaultSymbol = true,
                    DefaultLabel = GeomapmakerModule.MUP_UnassignedTemplateName,
                    Groups = listUniqueValueGroups,
                    Fields = new string[] { "MapUnit" }
                };

                // Set default color fill for unassigned Map Units
                CIMColor defaultColor = CIMColor.CreateRGBColor(255, 0, 0);

                SimpleFillStyle fillStyle = SimpleFillStyle.DiagonalCross;

                CIMPolygonSymbol defaultPolySymbol = SymbolFactory.Instance.ConstructPolygonSymbol(defaultColor, fillStyle, outline);

                CIMSymbolReference defaultSymbolRef = new CIMSymbolReference()
                {
                    Symbol = defaultPolySymbol
                };

                updatedRenderer.DefaultSymbol = defaultSymbolRef;

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

        /// <summary>
        /// Get templates for the MapUnitPolys layer
        /// </summary>
        /// <returns>List of MapUnitPolys template</returns>
        public static async Task<List<MapUnitPolyTemplate>> GetMapUnitPolyTemplatesAsync()
        {
            // List of templates to return
            List<MapUnitPolyTemplate> mupTemplates = new List<MapUnitPolyTemplate>();

            IEnumerable<EditingTemplate> layerTemplates = new List<EditingTemplate>();

            // Find the MapUnitPolys layer
            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "MapUnitPolys");

            if (layer == null)
            {
                return mupTemplates;
            }

            List<MapUnit> dmu = await DescriptionOfMapUnits.GetMapUnitsAsync();

            await QueuedTask.Run(() =>
            {
                // Get templates from layer
                layerTemplates = layer.GetTemplates();

                foreach (EditingTemplate template in layerTemplates)
                {
                    // Get CIMFeatureTemplate
                    CIMFeatureTemplate templateDef = template.GetDefinition() as CIMFeatureTemplate;

                    // If the template has a mapunit value
                    if (templateDef.DefaultValues.ContainsKey("mapunit") && templateDef.DefaultValues.ContainsKey("datasourceid"))
                    {
                        string muKey = templateDef.DefaultValues["mapunit"]?.ToString();

                        // Find the matching DMU row
                        MapUnit mapUnit = dmu.FirstOrDefault(a => a.MU == muKey);

                        // Check if the mapUnt was found
                        if (mapUnit != null)
                        {
                            MapUnitPolyTemplate tmpTemplate = new MapUnitPolyTemplate()
                            {
                                MapUnit = muKey,
                                HexColor = _helpers.ColorConverter.RGBtoHex(mapUnit.AreaFillRGB),
                                Tooltip = mapUnit.Tooltip,
                                DataSourceID = templateDef.DefaultValues["datasourceid"]?.ToString(),
                                Template = template
                            };

                            mupTemplates.Add(tmpTemplate);
                        }
                    }
                }
            });

            return mupTemplates;
        }
    }
}
