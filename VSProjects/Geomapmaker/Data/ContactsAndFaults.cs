using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing.Templates;
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
        public static async Task<List<ContactFaultTemplate>> GetContactFaultTemplatesAsync()
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

                foreach (EditingTemplate template in layerTemplates)
                {
                    // Skip over the default and the one-off sketch templates
                    if (template.Name != "ContactsAndFaults" && template.Name != GeomapmakerModule.CF_SketchTemplateName)
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
                }

            });

            return contactFaultTemplates;
        }

        public static async void ResetContactsFaultsSymbology()
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
                List<ContactFaultTemplate> cfTemplates = await GetContactFaultTemplatesAsync();

                foreach (var template in cfTemplates)
                {
                    CFSymbol Symbol = SymbolOptions.FirstOrDefault(a => a.Key == template.Symbol);

                    if (Symbol != null)
                    {
                        // Add symbology for templates
                        await AddSymbolToRenderer(Symbol.Key, Symbol.SymbolJson);
                    }
                }

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
                                // Add symbology for existing CF polylines
                                await AddSymbolToRenderer(Symbol.Key, Symbol.SymbolJson);
                            }
                        }
                    }
                }
            });
        }

        public static async Task AddSymbolToRenderer(string key, string symbolJson)
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