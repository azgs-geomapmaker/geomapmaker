﻿using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
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

            if (await FeatureLayers.FeatureLayerExistsAsync("OrientationPoints") == false)
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
                int tableCount = FeatureLayers.FeatureLayerCount("OrientationPoints");
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
                List<string> missingFields = await FeatureLayers.FeatureLayerGetMissingFieldsAsync("OrientationPoints", opRequiredFields);
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
                List<string> fieldsWithMissingValues = await FeatureLayers.FeatureLayerGetRequiredFieldIsNullAsync("OrientationPoints", opNotNull);
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
                List<string> duplicateIds = await FeatureLayers.FeatureLayerGetDuplicateValuesInFieldAsync("OrientationPoints", "orientationpoints_id");
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
        public static async Task<List<UndefinedTerm>> GetTermsUndefinedInGlossaryAsync(List<string> definedTerms)
        {
            List<UndefinedTerm> undefinedTerms = new List<UndefinedTerm>();

            List<string> TypeTerms = await FeatureLayers.FeatureLayerGetDistinctValuesForFieldAsync("OrientationPoints", "Type");

            IEnumerable<string> undefinedType = TypeTerms.Except(definedTerms);

            foreach (string term in undefinedType)
            {
                undefinedTerms.Add(new UndefinedTerm()
                {
                    DatasetName = "OrientationPoints",
                    FieldName = "Type",
                    Term = term
                });
            }

            List<string> IdentityConfidenceTerms = await FeatureLayers.FeatureLayerGetDistinctValuesForFieldAsync("OrientationPoints", "IdentityConfidence");

            IEnumerable<string> undefinedIdentityConfidence = IdentityConfidenceTerms.Except(definedTerms);

            foreach (string term in undefinedIdentityConfidence)
            {
                undefinedTerms.Add(new UndefinedTerm()
                {
                    DatasetName = "OrientationPoints",
                    FieldName = "IdentityConfidence",
                    Term = term
                });
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

            // Check if the symbol list has been populated 
            if (Symbology.OrientationPointSymbols == null)
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

                                    GemsSymbol Symbol = Symbology.OrientationPointSymbols.FirstOrDefault(a => a.Key == cfSymbolKey);

                                    if (Symbol != null)
                                    {
                                        // Add symbology for existing CF polylines
                                        AddSymbolToRenderer(Symbol.Key, Symbol.SymbolJson);
                                    }
                                }
                            }
                        }

                        OperationManager opManager = MapView.Active.Map.OperationManager;

                        List<Operation> mapUnitPolyLayerUndos = opManager.FindUndoOperations(a => a.Name == "Update layer renderer: OrientationPoints");
                        foreach (Operation undoOp in mapUnitPolyLayerUndos)
                        {
                            opManager.RemoveUndoOperation(undoOp);
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
            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "OrientationPoints");

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

                layer.SetRenderer(updatedRenderer);
            });
        }
    }
}
