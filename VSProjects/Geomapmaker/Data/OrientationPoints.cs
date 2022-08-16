using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
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
                new ValidationRule{ Description="No duplicate OrientationPoints_ID values."},
            };

            if (await AnyFeatureLayer.DoesLayerExistsAsync("OrientationPoints") == false)
            {
                results[0].Status = ValidationStatus.Skipped;
                results[0].Errors.Add("Layer not found");
                return results;
            }
            else // Table was found
            {
                results[0].Status = ValidationStatus.Passed;

                //
                // Check for duplicate layers
                //
                int tableCount = AnyFeatureLayer.GetLayerCount("OrientationPoints");
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
                // Check table for any missing fields 
                //

                // List of fields to check for
                List<string> opRequiredFields = new List<string>() { "type", "azimuth", "inclination", "symbol", "label", "locationconfidencemeters",
                "identityconfidence", "orientationconfidencedegrees", "plotatscale", "stationsid", "mapunit", "locationsourceid",
                "orientationsourceid", "notes", "orientationpoints_id" };

                // Get list of missing fields
                List<string> missingFields = await AnyFeatureLayer.GetMissingFieldsAsync("OrientationPoints", opRequiredFields);
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

                // List of fields to check for null values
                List<string> opNotNull = new List<string>() { "type", "azimuth", "inclination", "locationconfidencemeters", "identityconfidence", "orientationconfidencedegrees",
                    "plotatscale", "mapunit", "locationsourceid", "orientationsourceid", "orientationpoints_id" };

                // Get list of required fields with a null
                List<string> fieldsWithMissingValues = await AnyFeatureLayer.GetRequiredFieldIsNullAsync("OrientationPoints", opNotNull);
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
                // Check for duplicate OrientationPoints_ID values
                //
                List<string> duplicateIds = await AnyFeatureLayer.GetDuplicateValuesInFieldAsync("OrientationPoints", "orientationpoints_id");
                if (duplicateIds.Count == 0)
                {
                    results[4].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[4].Status = ValidationStatus.Failed;
                    foreach (string id in duplicateIds)
                    {
                        results[4].Errors.Add($"Duplicate OrientationPoints_ID value: {id}");
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
                string symbolDef = await AnyStandaloneTable.GetValueFromWhereClauseAsync("Symbology", $"Type = 'Point' AND Key = '{TypeSymbolDict[key]}'", "Description");

                undefinedTerms.Add(new GlossaryTerm()
                {
                    DatasetName = "OrientationPoints",
                    FieldName = "Type",
                    Term = key,
                    Definition = symbolDef
                });
            }

            List<string> IdentityConfidenceTerms = await AnyFeatureLayer.GetDistinctValuesForFieldAsync("OrientationPoints", "IdentityConfidence");

            IEnumerable<string> undefinedIdentityConfidence = IdentityConfidenceTerms.Except(definedTerms);

            foreach (string term in undefinedIdentityConfidence)
            {
                undefinedTerms.Add(await PredefinedTerms.GetPrepopulatedDefinitionAsync("OrientationPoints", "IdentityConfidence", term));
            }

            return undefinedTerms;
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

            // Check if the symbol list has already been populated 
            if (GeomapmakerModule.OrientationPointSymbols == null)
            {
                await Symbology.RefreshOPSymbolOptionsAsync();
            }

            ProgressorSource ps = new ProgressorSource("Rebuilding Orientation Points Symbology...");

            await QueuedTask.Run(() =>
            {
                using (Table opTable = opLayer.GetTable())
                {
                    if (opTable != null)
                    {
                        // Remove all existing symbols
                        opLayer.SetRenderer(opLayer.CreateRenderer(new SimpleRendererDefinition()));

                        QueryFilter queryFilter = new QueryFilter
                        {
                            WhereClause = "symbol IS NOT NULL",
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

                                    GemsSymbol Symbol = GeomapmakerModule.OrientationPointSymbols.FirstOrDefault(a => a.Key == cfSymbolKey);

                                    if (Symbol != null)
                                    {
                                        // Add symbology for existing CF polylines
                                        AddSymbolToRenderer(Symbol.Key, Symbol.SymbolJson);
                                    }
                                }
                            }
                        }

                        OperationManager opManager = MapView.Active?.Map?.OperationManager;

                        if (opManager != null)
                        {
                            List<Operation> mapUnitPolyLayerUndos = opManager?.FindUndoOperations(a => a.Name == "Update layer renderer: OrientationPoints");
                            foreach (Operation undoOp in mapUnitPolyLayerUndos)
                            {
                                opManager.RemoveUndoOperation(undoOp);
                            }
                        }
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
            FeatureLayer layer = MapView.Active?.Map?.GetLayersAsFlattenedList()?.OfType<FeatureLayer>()?.FirstOrDefault(l => l.Name == "OrientationPoints");

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

                // Rotate the symbol based on the Azimuth value
                var cimExpressionInfoZ = new CIMExpressionInfo { Expression = "$feature.Azimuth" };

                var cimVisualVariableInfoZ = new CIMVisualVariableInfo { VisualVariableInfoType = VisualVariableInfoType.Expression, ValueExpressionInfo = cimExpressionInfoZ };

                var listCIMVisualVariables = new List<CIMVisualVariable>
                {
                    new CIMRotationVisualVariable {
                        VisualVariableInfoZ = cimVisualVariableInfoZ
                    }
                };

                //Apply the visual variables to the renderer's VisualVariables property
                updatedRenderer.VisualVariables = listCIMVisualVariables.ToArray();

                layer.SetRenderer(updatedRenderer);
            });
        }

        /// <summary>
        /// Get a dictionary with all of the OrientationPoints Type and Symbol pairs.
        /// </summary>
        /// <returns>Returns a Dictionary<string, string> of Type and Symbol</returns>
        public static async Task<Dictionary<string, string>> GetTypeAndSymbolsAsync()
        {
            Dictionary<string, string> typeDictionary = new Dictionary<string, string>();

            FeatureLayer layer = MapView.Active?.Map.FindLayers("OrientationPoints").FirstOrDefault() as FeatureLayer;

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
        /// Find OrientationPoints that don't have a mapunit value. Find intersection with MapUnitPolys and update mapunit value.
        /// </summary>
        /// <returns>Count of rows updated</returns>
        public static async Task<int> UpdateOrientationPointsWithMapUnitIntersectionAsync()
        {
            int count = 0;

            FeatureLayer opLayer = (FeatureLayer)(MapView.Active?.Map.Layers.FirstOrDefault(a => a.Name == "OrientationPoints"));

            FeatureLayer mupLayer = (FeatureLayer)(MapView.Active?.Map.Layers.FirstOrDefault(a => a.Name == "MapUnitPolys"));

            if (opLayer == null || mupLayer == null)
            {
                return 0;
            }

            await QueuedTask.Run(() =>
            {
                using (Table opTable = opLayer.GetTable())
                using (Table mupTable = mupLayer.GetTable())
                {
                    if (opTable != null && mupTable != null)
                    {
                        QueryFilter queryFilter = new QueryFilter
                        {
                            SubFields = "shape, mapunit",
                            //WhereClause = "mapunit IS NULL"
                        };

                        EditOperation editOperation = new EditOperation()
                        {
                            Name = "Update MapUnit value for OrientationPoints",
                            ProgressMessage = "Intersecting Orientation Points with MapUnitPolys",
                            ShowProgressor = true
                        };

                        editOperation.Callback(context =>
                        {
                            using (RowCursor rowCursor = opTable.Search(queryFilter, false))
                            {
                                while (rowCursor.MoveNext())
                                {
                                    using (Row row = rowCursor.Current)
                                    {
                                        // Get the geometry for the station
                                        MapPoint point = (MapPoint)row["SHAPE"];

                                        // Get all of map features that intersect the station
                                        Dictionary<BasicFeatureLayer, List<long>> features = MapView.Active.GetFeatures(point);

                                        // Filter out only MapUnitPolys and then grab the first Object ID in the list
                                        long? mupObjectId = features.Where(x => x.Key.Name == "MapUnitPolys").Select(a => a.Value).FirstOrDefault()?.FirstOrDefault();

                                        if (mupObjectId != null)
                                        {
                                            string mapunit = "";

                                            QueryFilter innerQueryFilter = new QueryFilter
                                            {
                                                SubFields = "mapunit",
                                                ObjectIDs = new List<long> { (long)mupObjectId }
                                            };

                                            using (RowCursor innerRowCursor = mupTable.Search(innerQueryFilter))
                                            {
                                                while (innerRowCursor.MoveNext())
                                                {
                                                    using (Row innerRow = innerRowCursor.Current)
                                                    {
                                                        // Grab the mapunit value
                                                        mapunit = innerRow["mapunit"]?.ToString();
                                                    }
                                                }
                                            }

                                            if (!string.IsNullOrEmpty(mapunit))
                                            {
                                                count++;

                                                // In order to update the Map and/or the attribute table.
                                                // Has to be called before any changes are made to the row.
                                                context.Invalidate(row);

                                                row["mapunit"] = mapunit;

                                                // After all the changes are done, persist it.
                                                row.Store();

                                                // Has to be called after the store too.
                                                context.Invalidate(row);
                                            }
                                        }
                                    }
                                }
                            }
                        }, opTable);

                        bool result = editOperation.Execute();
                    }
                }
            });

            return count;
        }

        /// <summary>
        /// Update the symbol values to be zero-padded. Example, 1.1.1 => 001.001.001
        /// </summary>
        /// <returns>Number of rows updated</returns>
        public static async Task<int> ZeroPadSymbolValues()
        {
            int count = 0;

            FeatureLayer opLayer = (FeatureLayer)(MapView.Active?.Map.Layers.FirstOrDefault(a => a.Name == "OrientationPoints"));

            if (opLayer == null)
            {
                return 0;
            }

            await QueuedTask.Run(() =>
            {
                using (Table opTable = opLayer.GetTable())
                {
                    if (opTable != null)
                    {
                        QueryFilter queryFilter = new QueryFilter
                        {
                            SubFields = "symbol",
                            WhereClause = "symbol <> ''"
                        };

                        EditOperation editOperation = new EditOperation()
                        {
                            Name = "Zero-Pad OrientationPoints Symbols",
                            ProgressMessage = "Updating OrientationPoints Symbols",
                            ShowProgressor = true
                        };

                        editOperation.Callback(context =>
                        {
                            using (RowCursor rowCursor = opTable.Search(queryFilter, false))
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
                        }, opTable);

                        bool result = editOperation.Execute();
                    }
                }
            });

            return count;
        }
    }
}
