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
        /// <summary>
        /// Get validation report for layer
        /// </summary>
        /// <returns>List of Validation results</returns>
        public static async Task<List<ValidationRule>> GetValidationResultsAsync()
        {
            List<ValidationRule> results = new List<ValidationRule>
            {
                new ValidationRule{ Description="Layer exists."},
                new ValidationRule{ Description="No duplicate layers."},
                new ValidationRule{ Description="No missing fields."},
                new ValidationRule{ Description="No missing DescriptionOfMapUnits definitions."},
                new ValidationRule{ Description="No empty/null values in required fields."},
                new ValidationRule{ Description="No duplicate MapUnitPolys_ID values."}
            };

            if (await AnyFeatureLayer.DoesLayerExistsAsync("MapUnitPolys") == false)
            {
                results[0].Status = ValidationStatus.Failed;
                results[0].Errors.Add("Layer not found");
                return results;
            }
            else // Table was found
            {
                results[0].Status = ValidationStatus.Passed;

                //
                // Check for duplicate layers
                //
                int tableCount = AnyFeatureLayer.GetLayerCount("MapUnitPolys");
                if (tableCount > 1)
                {
                    results[1].Status = ValidationStatus.Failed;
                    results[1].Errors.Add($"{tableCount} layers found");
                }
                else
                {
                    results[1].Status = ValidationStatus.Passed;
                }

                //
                // Check layer for any missing fields 
                //

                // List of fields to check for
                List<string> mupRequiredFields = new List<string>() { "mapunit", "identityconfidence", "label", "symbol", "datasourceid", "notes",
                "mapunitpolys_id" };

                // Get the missing required fields
                List<string> missingFields = await AnyFeatureLayer.GetMissingFieldsAsync("MapUnitPolys", mupRequiredFields);
                if (missingFields.Count == 0)
                {
                    results[2].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[2].Status = ValidationStatus.Failed;
                    foreach (string field in missingFields)
                    {
                        results[2].Errors.Add($"Field not found: {field}");
                    }
                }

                //
                // Check for any missing MapUnit definitions in the DMU
                //
                List<string> missingDMU = await Data.MapUnitPolys.GetMapUnitsNotDefinedInDMUTableAsync();
                if (missingDMU.Count == 0)
                {
                    results[3].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[3].Status = ValidationStatus.Failed;
                    foreach (string mu in missingDMU)
                    {
                        results[3].Errors.Add($"MapUnit not defined in DMU: {mu}");
                    }
                }

                //
                // Check for empty/null values in required fields
                //

                // List of fields to check for null values
                List<string> mupNotNull = new List<string>() { "mapunit", "identityconfidence", "datasourceid", "mapunitpolys_id" };

                // Get required fields with null values
                List<string> fieldsWithMissingValues = await AnyFeatureLayer.GetRequiredFieldIsNullAsync("MapUnitPolys", mupNotNull);
                if (fieldsWithMissingValues.Count == 0)
                {
                    results[4].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[4].Status = ValidationStatus.Failed;
                    foreach (string field in fieldsWithMissingValues)
                    {
                        results[4].Errors.Add($"Null value found in field: {field}");
                    }
                }

                //
                // Check for duplicate MapUnitPolys_ID values
                //
                List<string> duplicateIds = await AnyFeatureLayer.GetDuplicateValuesInFieldAsync("MapUnitPolys", "MapUnitPolys_ID");
                if (duplicateIds.Count == 0)
                {
                    results[5].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[5].Status = ValidationStatus.Failed;
                    foreach (string id in duplicateIds)
                    {
                        results[5].Errors.Add($"Duplicate MapUnitPolys_ID value: {id}");
                    }
                }
            }

            return results;
        }

        /// <summary>
        ///  Get undefined terms that must be defined in the Glossary
        /// </summary>
        /// <param name="definedTerms">List of defined terms in the glossary</param>
        /// <returns>List of missing glossary terms</returns>
        public static async Task<List<GlossaryTerm>> GetTermsUndefinedInGlossaryAsync(List<string> definedTerms)
        {
            List<GlossaryTerm> undefinedTerms = new List<GlossaryTerm>();

            List<string> IdentityConfidenceTerms = await AnyFeatureLayer.GetDistinctValuesForFieldAsync("MapUnitPolys", "IdentityConfidence");

            IEnumerable<string> undefinedType = IdentityConfidenceTerms.Except(definedTerms);

            foreach (string term in undefinedType)
            {
                undefinedTerms.Add(new GlossaryTerm()
                {
                    DatasetName = "MapUnitPolys",
                    FieldName = "IdentityConfidence",
                    Term = term
                });
            }

            return undefinedTerms;
        }

        /// <summary>
        /// Compare the MapUnitPolys layer with the DescriptionOfMapUnits table. Return any MapUnitPolys not defined in DescriptionOfMapUnits. 
        /// </summary>
        /// <returns>List of MapUnits not defined</returns>
        public static async Task<List<string>> GetMapUnitsNotDefinedInDMUTableAsync()
        {
            List<string> mapUnitPolys = await AnyFeatureLayer.GetDistinctValuesForFieldAsync("MapUnitPolys", "MapUnit");

            List<string> mapUnitDescriptions = await AnyStandaloneTable.GetDistinctValuesForFieldAsync("DescriptionOfMapUnits", "MapUnit");

            return mapUnitPolys.Except(mapUnitDescriptions).ToList();
        }

        /// <summary>
        /// Rebuild the renderer for MapUnitPolys from the DMU table. Rebuild templates for the MUP layer from the DMU table
        /// </summary>
        public static async void RebuildMUPSymbologyAndTemplates()
        {
            FeatureLayer layer = MapView.Active?.Map?.GetLayersAsFlattenedList()?.OfType<FeatureLayer>()?.FirstOrDefault(l => l.Name == "MapUnitPolys");

            if (layer == null)
            {
                return;
            }

            ProgressorSource ps = new ProgressorSource("Rebuilding Map Unit Poly Symbology...");

            await QueuedTask.Run(async () =>
            {
                // Get all DMUs
                List<MapUnit> allDMUs = await DescriptionOfMapUnits.GetMapUnitsAsync();

                List<MapUnit> StandardDMUs = allDMUs.Where(a => !string.IsNullOrEmpty(a.MU)).OrderBy(a => a.MU).ToList();

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
                    insp["mapunit"] = mu.MU;
                    insp["datasourceid"] = mu.DescriptionSourceID;

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

                OperationManager opManager = MapView.Active?.Map?.OperationManager;

                if (opManager != null)
                {
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
            FeatureLayer layer = MapView.Active?.Map?.GetLayersAsFlattenedList()?.OfType<FeatureLayer>()?.FirstOrDefault(l => l.Name == "MapUnitPolys");

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
                    //CIMFeatureTemplate templateDef = template.GetDefinition() as CIMFeatureTemplate;
                    // NEW in ArcGIS 3.5 - Use built-in Inspector to get template default values:
                    Inspector templateInspector = template.Inspector;

                    // Ran into a case-sensitivity problem when accessing the default values by Key/Fieldname
                    // The casing seems to change based on whether it is a file-gdb or an enterprise gdb
                    // Handle case-sensitivity by trying different variations of field names
                    string muKey = null;
                    string dataSourceId = null;

                    // Try different case variations for mapunit field
                    string[] mapUnitVariations = { "mapunit", "MapUnit", "MAPUNIT" };
                    foreach (string fieldName in mapUnitVariations)
                    {
                        try
                        {
                            var value = templateInspector[fieldName];
                            if (value != null)
                            {
                                muKey = value.ToString();
                                break;
                            }
                        }
                        catch
                        {
                            // Continue to next variation if this one fails
                        }
                    }

                    // Try different case variations for datasourceid field
                    string[] dataSourceVariations = { "datasourceid", "DataSourceID", "DATASOURCEID", "DataSourceId" };
                    foreach (string fieldName in dataSourceVariations)
                    {
                        try
                        {
                            var value = templateInspector[fieldName];
                            if (value != null)
                            {
                                dataSourceId = value.ToString();
                                break;
                            }
                        }
                        catch
                        {
                            // Continue to next variation if this one fails
                        }
                    }

                    // If the template has a mapunit value (Unassigned template may not)
                    if (!string.IsNullOrEmpty(muKey))
                    {
                        // Find the matching DMU row
                        MapUnit mapUnit = dmu.FirstOrDefault(a => a.MU == muKey);

                        // Check if the mapUnit was found
                        if (mapUnit != null)
                        {
                            MapUnitPolyTemplate tmpTemplate = new MapUnitPolyTemplate()
                            {
                                MapUnit = muKey,
                                HexColor = _helpers.ColorConverter.RGBtoHex(mapUnit.AreaFillRGB),
                                Tooltip = mapUnit.Tooltip,
                                DataSourceID = dataSourceId,
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
