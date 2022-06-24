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
                new ValidationRule{ Description="No empty/null values in required fields."},
                new ValidationRule{ Description="No duplicate Label values."},
                new ValidationRule{ Description="No duplicate ContactsAndFaults_ID values."}
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
                List<string> cfRequiredFields = new List<string>() { "type", "isconcealed", "locationconfidencemeters", "existenceconfidence",
                "identityconfidence", "label", "symbol", "datasourceid", "notes", "contactsandfaults_id" };

                List<string> missingFields = await AnyFeatureLayer.GetMissingFieldsAsync("ContactsAndFaults", cfRequiredFields);
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
                // Check for empty/null values in required fields
                //
                List<string> cfNotNullFields = new List<string>() { "type", "isconcealed", "locationconfidencemeters", "existenceconfidence", "identityconfidence", "datasourceid", "contactsandfaults_id" };
                List<string> fieldsWithMissingValues = await AnyFeatureLayer.GetRequiredFieldIsNullAsync("ContactsAndFaults", cfNotNullFields);
                if (fieldsWithMissingValues.Count == 0)
                {
                    results[3].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[3].Status = ValidationStatus.Failed;
                    foreach (string field in fieldsWithMissingValues)
                    {
                        results[3].Errors.Add($"Null value found in field: {field}");
                    }
                }

                //
                // Check for duplicate Label values
                //
                List<string> duplicateLabels = await AnyFeatureLayer.GetDuplicateValuesInFieldAsync("ContactsAndFaults", "Label");
                if (duplicateLabels.Count == 0)
                {
                    results[4].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[4].Status = ValidationStatus.Failed;
                    foreach (string label in duplicateLabels)
                    {
                        results[4].Errors.Add($"Duplicate Label value: {label}");
                    }
                }

                //
                // Check for duplicate ContactsAndFaults_ID values
                //
                List<string> duplicateIds = await AnyFeatureLayer.GetDuplicateValuesInFieldAsync("ContactsAndFaults", "ContactsAndFaults_ID");
                if (duplicateIds.Count == 0)
                {
                    results[5].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[5].Status = ValidationStatus.Failed;
                    foreach (string id in duplicateIds)
                    {
                        results[5].Errors.Add($"Duplicate ContactsAndFaults_ID value: {id}");
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

            List<string> TypeTerms = await AnyFeatureLayer.GetDistinctValuesForFieldAsync("ContactsAndFaults", "Type");

            IEnumerable<string> undefinedType = TypeTerms.Except(definedTerms);

            foreach (string term in undefinedType)
            {
                undefinedTerms.Add(new GlossaryTerm()
                {
                    DatasetName = "ContactsAndFaults",
                    FieldName = "Type",
                    Term = term
                });
            }

            List<string> ExistenceConfidenceTerms = await AnyFeatureLayer.GetDistinctValuesForFieldAsync("ContactsAndFaults", "ExistenceConfidence");

            IEnumerable<string> undefinedExistenceConfidence = ExistenceConfidenceTerms.Except(definedTerms);

            foreach (string term in undefinedExistenceConfidence)
            {
                undefinedTerms.Add(new GlossaryTerm()
                {
                    DatasetName = "ContactsAndFaults",
                    FieldName = "ExistenceConfidence",
                    Term = term
                });
            }

            List<string> IdentityConfidenceTerms = await AnyFeatureLayer.GetDistinctValuesForFieldAsync("ContactsAndFaults", "IdentityConfidence");

            IEnumerable<string> undefinedIdentityConfidence = IdentityConfidenceTerms.Except(definedTerms);

            foreach (string term in undefinedIdentityConfidence)
            {
                undefinedTerms.Add(new GlossaryTerm()
                {
                    DatasetName = "ContactsAndFaults",
                    FieldName = "IdentityConfidence",
                    Term = term
                });
            }

            return undefinedTerms;
        }

        /// <summary>
        /// Get Templates for Contacts and Faults layer
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
                        Type = templateDef.DefaultValues["type"]?.ToString(),
                        Label = templateDef.DefaultValues["label"]?.ToString(),
                        Symbol = templateDef.DefaultValues["symbol"]?.ToString(),
                        IdentityConfidence = templateDef.DefaultValues["identityconfidence"]?.ToString(),
                        ExistenceConfidence = templateDef.DefaultValues["existenceconfidence"]?.ToString(),
                        LocationConfidenceMeters = templateDef.DefaultValues["locationconfidencemeters"]?.ToString(),
                        IsConcealed = templateDef.DefaultValues["isconcealed"]?.ToString() == "Y",
                        DataSource = templateDef.DefaultValues["datasourceid"]?.ToString(),
                        Template = template
                    };

                    // Notes is an optional field
                    if (templateDef.DefaultValues.ContainsKey("notes"))
                    {
                        tmpTemplate.Notes = templateDef.DefaultValues["notes"]?.ToString();
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
                    if (table != null)
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