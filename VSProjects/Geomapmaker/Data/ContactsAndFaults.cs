using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
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
    public class ContactsAndFaults
    {
        /// <summary>
        /// Check if the ContactsAndFaults layer exists in Active Map
        /// </summary>
        /// <returns>Returns true if layer exists</returns>
        public static bool ContactsAndFaultsExists()
        {
            Layer layer = MapView.Active?.Map.FindLayers("ContactsAndFaults").FirstOrDefault();

            return layer != null;
        }

        /// <summary>
        /// Get Templates for Contacts and Faults
        /// </summary>
        /// <param name="filterSketch"></param>
        /// <returns>ContactFaultTemplate List</returns>
        public static async Task<List<ContactFaultTemplate>> GetContactFaultTemplatesAsync(bool filterSketch = false)
        {
            // List of templates to return
            List<ContactFaultTemplate> contactFaultTemplates = new List<ContactFaultTemplate>();

            IEnumerable<EditingTemplate> layerTemplates = new List<EditingTemplate>();

            // Find the ContactsFaults layer
            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "ContactsAndFaults");

            if (layer == null)
            {
                return contactFaultTemplates;
            }

            await QueuedTask.Run(() =>
            {
                // Get templates from CF layer
                layerTemplates = layer.GetTemplates();

                if (filterSketch)
                {
                    // Skip over the sketch template
                    layerTemplates = layerTemplates.Where(a => a.Name != GeomapmakerModule.CF_SketchTemplateName);
                }

                // Skip the default template
                layerTemplates = layerTemplates.Where(a => a.Name != "ContactsAndFaults");

                foreach (EditingTemplate template in layerTemplates)
                {
                    // Get CIMFeatureTemplate
                    CIMFeatureTemplate templateDef = template.GetDefinition() as CIMFeatureTemplate;

                    ContactFaultTemplate tmpTemplate = new ContactFaultTemplate()
                    {
                        Type = templateDef.DefaultValues["type"].ToString(),
                        Label = templateDef.DefaultValues["label"].ToString(),
                        Symbol = templateDef.DefaultValues["symbol"].ToString(),
                        IdentityConfidence = templateDef.DefaultValues["identityconfidence"].ToString(),
                        ExistenceConfidence = templateDef.DefaultValues["existenceconfidence"].ToString(),
                        LocationConfidenceMeters = templateDef.DefaultValues["locationconfidencemeters"].ToString(),
                        IsConcealed = templateDef.DefaultValues["isconcealed"].ToString() == "Y",
                        DataSource = templateDef.DefaultValues["datasourceid"].ToString(),
                        Template = template
                    };

                    // Notes is an optional field
                    if (templateDef.DefaultValues.ContainsKey("notes"))
                    {
                        tmpTemplate.Notes = templateDef.DefaultValues["notes"].ToString();
                    }

                    contactFaultTemplates.Add(tmpTemplate);
                }

            });

            return contactFaultTemplates;
        }

        /// <summary>
        /// Rebuld the symbology renderer for ContactsAndFaults
        /// </summary>
        public static async void RebuildContactsFaultsSymbology()
        {
            // CF Layer
            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "ContactsAndFaults");

            if (layer == null)
            {
                return;
            }

            // Check if the symbol list has been populated 
            if (Symbology.ContactsAndFaultsSymbols == null)
            {
                await Symbology.RefreshCFSymbolOptionsAsync();
            }

            // Get the CF Symbology Options
            List<GemsSymbol> SymbolOptions = Symbology.ContactsAndFaultsSymbols;

            if (SymbolOptions == null)
            {
                return;
            }

            ProgressorSource ps = new ProgressorSource("Rebuilding Contacts And Faults Symbology...");

            await QueuedTask.Run(async () =>
            {
                // Remove existing symbols
                layer.SetRenderer(layer.CreateRenderer(new SimpleRendererDefinition()));

                // Get all CF templates except the default
                List<ContactFaultTemplate> cfTemplates = await GetContactFaultTemplatesAsync(false);

                foreach (ContactFaultTemplate template in cfTemplates)
                {
                    GemsSymbol Symbol = SymbolOptions.FirstOrDefault(a => a.Key == template.Symbol);

                    if (Symbol != null)
                    {
                        // Add symbology for templates
                        AddSymbolToRenderer(Symbol.Key, Symbol.SymbolJson);
                    }
                }

                using (Table table = layer.GetTable())
                {
                    QueryFilter queryFilter = new QueryFilter
                    {
                        PrefixClause = "DISTINCT",
                        PostfixClause = "ORDER BY symbol",
                        SubFields = "symbol"
                    };

                    using (RowCursor rowCursor = table.Search(queryFilter))
                    {
                        while (rowCursor.MoveNext())
                        {
                            using (Row row = rowCursor.Current)
                            {
                                string cfSymbolKey = row["symbol"]?.ToString();

                                GemsSymbol Symbol = SymbolOptions.FirstOrDefault(a => a.Key == cfSymbolKey);

                                if (Symbol != null)
                                {
                                    // Add symbology for existing CF polylines
                                    AddSymbolToRenderer(Symbol.Key, Symbol.SymbolJson);
                                }
                            }
                        }
                    }
                }

                OperationManager opManager = MapView.Active.Map.OperationManager;

                List<Operation> mapUnitPolyLayerUndos = opManager.FindUndoOperations(a => a.Name == "Update layer renderer: ContactsAndFaults");
                foreach (Operation undoOp in mapUnitPolyLayerUndos)
                {
                    opManager.RemoveUndoOperation(undoOp);
                }

            }, ps.Progressor);
        }

        /// <summary>
        /// Add a symbol to the CF renderer 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="symbolJson"></param>
        public static async void AddSymbolToRenderer(string key, string symbolJson)
        {
            // Find the ContactsFaults layer
            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "ContactsAndFaults");

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
                    Patch = PatchShape.AreaPolygon,
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