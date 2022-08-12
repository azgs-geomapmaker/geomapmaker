using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
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
                // Check for duplicate ContactsAndFaults_ID values
                //
                List<string> duplicateIds = await AnyFeatureLayer.GetDuplicateValuesInFieldAsync("ContactsAndFaults", "ContactsAndFaults_ID");
                if (duplicateIds.Count == 0)
                {
                    results[4].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[4].Status = ValidationStatus.Failed;
                    foreach (string id in duplicateIds)
                    {
                        results[4].Errors.Add($"Duplicate ContactsAndFaults_ID value: {id}");
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

            // Get Type and Symbol value
            Dictionary<string, string> TypeSymbolDict = await GetTypeAndSymbolsAsync();

            // Filter out types that have already been defined in the glossary
            TypeSymbolDict = TypeSymbolDict.Where(a => !definedTerms.Contains(a.Key)).ToDictionary(p => p.Key, p => p.Value);

            foreach (string key in TypeSymbolDict.Keys)
            {
                // Get the symbol description 
                string symbolDef = await AnyStandaloneTable.GetValueFromWhereClauseAsync("Symbology", $"Type = 'Line' AND Key = '{TypeSymbolDict[key]}'", "Description");

                undefinedTerms.Add(new GlossaryTerm()
                {
                    DatasetName = "ContactsAndFaults",
                    FieldName = "Type",
                    Term = key,
                    Definition = symbolDef
                });
            }

            // ExistenceConfidence
            List<string> ExistenceConfidenceTerms = await AnyFeatureLayer.GetDistinctValuesForFieldAsync("ContactsAndFaults", "ExistenceConfidence");

            IEnumerable<string> undefinedExistenceConfidence = ExistenceConfidenceTerms.Except(definedTerms);

            foreach (string term in undefinedExistenceConfidence)
            {
                undefinedTerms.Add(await PredefinedTerms.GetPrepopulatedDefinitionAsync("ContactsAndFaults", "ExistenceConfidence", term));
            }

            // IdentityConfidence
            List<string> IdentityConfidenceTerms = await AnyFeatureLayer.GetDistinctValuesForFieldAsync("ContactsAndFaults", "IdentityConfidence");

            IEnumerable<string> undefinedIdentityConfidence = IdentityConfidenceTerms.Except(definedTerms);

            foreach (string term in undefinedIdentityConfidence)
            {
                undefinedTerms.Add(await PredefinedTerms.GetPrepopulatedDefinitionAsync("ContactsAndFaults", "IdentityConfidence", term));
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
            FeatureLayer layer = MapView.Active?.Map?.GetLayersAsFlattenedList()?.OfType<FeatureLayer>()?.FirstOrDefault(l => l.Name == "ContactsAndFaults");

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

                    Dictionary<string, string> defaultValues = new Dictionary<string, string>();

                    // Rebuild the dictionary with lowercase keys to avoid casing-headaches
                    foreach (KeyValuePair<string, object> row in templateDef.DefaultValues)
                    {
                        defaultValues.Add(row.Key?.ToLower(), row.Value?.ToString());
                    }

                    ContactFaultTemplate tmpTemplate = new ContactFaultTemplate();

                    if (defaultValues.ContainsKey("label"))
                    {
                        tmpTemplate.Label = defaultValues["label"];

                        if (defaultValues.ContainsKey("type"))
                        {
                            tmpTemplate.Type = defaultValues["type"];
                        }
                        if (defaultValues.ContainsKey("symbol"))
                        {
                            tmpTemplate.Symbol = defaultValues["symbol"];
                        }
                        if (defaultValues.ContainsKey("identityconfidence"))
                        {
                            tmpTemplate.IdentityConfidence = defaultValues["identityconfidence"];
                        }
                        if (defaultValues.ContainsKey("existenceconfidence"))
                        {
                            tmpTemplate.ExistenceConfidence = defaultValues["existenceconfidence"];
                        }
                        if (defaultValues.ContainsKey("locationconfidencemeters"))
                        {
                            tmpTemplate.LocationConfidenceMeters = defaultValues["locationconfidencemeters"];
                        }
                        if (defaultValues.ContainsKey("isconcealed"))
                        {
                            tmpTemplate.IsConcealed = defaultValues["isconcealed"] == "Y";
                        }
                        if (defaultValues.ContainsKey("datasourceid"))
                        {
                            tmpTemplate.DataSource = defaultValues["datasourceid"];
                        }
                        if (defaultValues.ContainsKey("notes"))
                        {
                            tmpTemplate.Notes = defaultValues["notes"];
                        }

                        tmpTemplate.Template = template;

                        contactFaultTemplates.Add(tmpTemplate);
                    }

                }

            });

            return contactFaultTemplates.OrderBy(a => a.Label).ToList();
        }

        /// <summary>
        /// Rebuld the symbology renderer for ContactsAndFaults
        /// </summary>
        public static async void RebuildContactsFaultsSymbology()
        {
            // CF Layer
            FeatureLayer layer = MapView.Active?.Map?.GetLayersAsFlattenedList()?.OfType<FeatureLayer>()?.FirstOrDefault(l => l.Name == "ContactsAndFaults");

            if (layer == null)
            {
                return;
            }

            // Check if the symbol list has been populated 
            if (GeomapmakerModule.ContactsAndFaultsSymbols == null)
            {
                await Symbology.RefreshCFSymbolOptionsAsync();
            }

            // Get the CF Symbology Options
            List<GemsSymbol> SymbolOptions = GeomapmakerModule.ContactsAndFaultsSymbols;

            if (SymbolOptions == null)
            {
                return;
            }

            ProgressorSource ps = new ProgressorSource("Rebuilding Contacts And Faults Symbology...");

            await QueuedTask.Run(async () =>
            {
                // Prevent default templates from generating 
                CIMBasicFeatureLayer layerDef = layer.GetDefinition() as CIMBasicFeatureLayer;
                layerDef.AutoGenerateFeatureTemplates = false;
                layer.SetDefinition(layerDef);

                // Get all CF templates except the default
                List<ContactFaultTemplate> cfTemplates = await GetContactFaultTemplatesAsync(false);

                // Remove existing symbols
                layer.SetRenderer(layer.CreateRenderer(new SimpleRendererDefinition()));

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

                OperationManager opManager = MapView.Active?.Map?.OperationManager;

                if (opManager != null)
                {
                    List<Operation> mapUnitPolyLayerUndos = opManager.FindUndoOperations(a => a.Name == "Update layer renderer: ContactsAndFaults");
                    foreach (Operation undoOp in mapUnitPolyLayerUndos)
                    {
                        opManager.RemoveUndoOperation(undoOp);
                    }
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
            FeatureLayer layer = MapView.Active?.Map?.GetLayersAsFlattenedList()?.OfType<FeatureLayer>()?.FirstOrDefault(l => l.Name == "ContactsAndFaults");

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
                    Editable = true,
                    Label = key,
                    Description = key,
                    Patch = PatchShape.LineHorizontal,
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

        /// <summary>
        /// Get a dictionary with all of the ContactsAndFaults Type and Symbol pairs.
        /// </summary>
        /// <returns>Returns a Dictionary<string, string> of Type and Symbol</returns>
        public static async Task<Dictionary<string, string>> GetTypeAndSymbolsAsync()
        {
            Dictionary<string, string> typeDictionary = new Dictionary<string, string>();

            FeatureLayer layer = MapView.Active?.Map.FindLayers("ContactsAndFaults").FirstOrDefault() as FeatureLayer;

            if (layer != null)
            {
                await QueuedTask.Run(() =>
                {
                    using (Table table = layer.GetTable())
                    {
                        if (table != null)
                        {
                            QueryFilter queryFilter = new QueryFilter
                            {
                                WhereClause = "type <> '' AND symbol <> ''",
                                SubFields = "type, symbol"
                            };

                            using (RowCursor rowCursor = table.Search(queryFilter))
                            {
                                while (rowCursor.MoveNext())
                                {
                                    using (Row row = rowCursor.Current)
                                    {
                                        string type = row["type"]?.ToString();

                                        string symbol = row["symbol"]?.ToString();

                                        typeDictionary[type] = symbol;
                                    }
                                }
                            }
                        }
                    }
                });
            }

            return typeDictionary;
        }

        /// <summary>
        /// Update the symbol values to be zero-padded. Example, 1.1.1 => 001.001.001
        /// </summary>
        /// <returns>Number of rows updated</returns>
        public static async Task<int> ZeroPadSymbolValues()
        {
            int count = 0;

            FeatureLayer layer = (FeatureLayer)(MapView.Active?.Map.Layers.FirstOrDefault(a => a.Name == "ContactsAndFaults"));

            if (layer == null)
            {
                return 0;
            }

            await QueuedTask.Run(() =>
            {
                using (Table table = layer.GetTable())
                {
                    if (table != null)
                    {
                        QueryFilter queryFilter = new QueryFilter
                        {
                            SubFields = "symbol",
                            WhereClause = "symbol <> ''"
                        };

                        EditOperation editOperation = new EditOperation()
                        {
                            Name = "Zero-Pad ContactsAndFaults Symbols",
                            ProgressMessage = "Updating ContactsAndFaults Symbols",
                            ShowProgressor = true
                        };

                        editOperation.Callback(context =>
                        {
                            using (RowCursor rowCursor = table.Search(queryFilter, false))
                            {
                                while (rowCursor.MoveNext())
                                {
                                    using (Row row = rowCursor.Current)
                                    {
                                        string originalSymbol = row["symbol"]?.ToString();

                                        // Original symbol must be up of only digits and periods
                                        if (originalSymbol.All(c => char.IsDigit(c) || c == '.'))
                                        {
                                            string[] splitSymbols = originalSymbol.Split('.');

                                            for (int i = 0; i < splitSymbols.Length; i++)
                                            {
                                                // Parse the int
                                                if (int.TryParse(splitSymbols[i], out int value))
                                                {
                                                    // Zero-pad 
                                                    splitSymbols[i] = value.ToString("000");
                                                };
                                            }

                                            // Combine the zero-padded numbers
                                            string paddedSymbol = string.Join(".", splitSymbols);

                                            if (originalSymbol != paddedSymbol)
                                            {
                                                count++;

                                                // In order to update the Map and/or the attribute table.
                                                // Has to be called before any changes are made to the row.
                                                context.Invalidate(row);

                                                row["symbol"] = paddedSymbol;

                                                // After all the changes are done, persist it.
                                                row.Store();

                                                // Has to be called after the store too.
                                                context.Invalidate(row);
                                            }
                                        }
                                    }
                                }
                            }
                        }, table);

                        bool result = editOperation.Execute();
                    }
                }
            });

            return count;
        }
    }
}