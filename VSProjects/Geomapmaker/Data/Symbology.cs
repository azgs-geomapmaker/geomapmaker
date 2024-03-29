﻿using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker._helpers;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public class Symbology
    {
        // List of OP symbols
        //public static List<GemsSymbol> OrientationPointSymbols;

        /// <summary>
        /// Get validation report for table
        /// </summary>
        /// <returns>List of Validation results</returns>
        public static async Task<List<ValidationRule>> GetValidationResultsAsync()
        {
            List<ValidationRule> results = new List<ValidationRule>
            {
                new ValidationRule{ Description="Table exists."},
                new ValidationRule{ Description="No duplicate tables."},
                new ValidationRule{ Description="No missing fields."},
                new ValidationRule{ Description="No missing ContactsAndFaults symbols"},
                new ValidationRule{ Description="No duplicate ContactsAndFaults symbols"},
                new ValidationRule{ Description="No missing OrientationPoints symbols"},
                new ValidationRule{ Description="No duplicate OrientationPoints symbols"}
            };

            if (await AnyStandaloneTable.DoesTableExistsAsync("Symbology") == false)
            {
                results[0].Status = ValidationStatus.Failed;
                results[0].Errors.Add("Table not found");
                return results;
            }
            else // Table was found
            {
                results[0].Status = ValidationStatus.Passed;

                //
                // Check for duplicate tables
                //
                int tableCount = AnyStandaloneTable.GetTableCount("Symbology");
                if (tableCount > 1)
                {
                    results[1].Status = ValidationStatus.Failed;
                    results[1].Errors.Add($"{tableCount} tables found");
                }
                else
                {
                    results[1].Status = ValidationStatus.Passed;
                }

                //
                // Check for any missing fields
                //

                // List of required fields
                List<string> symbologyRequiredFields = new List<string>() { "symbology_id", "symboltype", "key", "description", "symbol" };

                // Get missing fields
                List<string> missingFields = await AnyStandaloneTable.GetMissingFieldsAsync("Symbology", symbologyRequiredFields);
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
                // Check for any missing CF symbols
                //
                List<string> missingCFsymbols = await GetMissingContactsAndFaultsSymbologyAsync();
                if (missingCFsymbols.Count == 0)
                {
                    results[3].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[3].Status = ValidationStatus.Failed;
                    foreach (string key in missingCFsymbols)
                    {
                        results[3].Errors.Add($"Missing line symbology: {key}");
                    }
                }

                //
                // Check for any duplicate CF symbols
                //
                List<string> duplicateCFsymbols = await GetDuplicateContactsAndFaultsSymbologyAsync();
                if (duplicateCFsymbols.Count == 0)
                {
                    results[4].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[4].Status = ValidationStatus.Failed;
                    foreach (string key in duplicateCFsymbols)
                    {
                        results[4].Errors.Add($"Duplicate line symbology: {key}");
                    }
                }

                //
                // Check for any missing OP symbols
                //
                List<string> missingOPsymbols = await GetMissingOrientationPointsSymbologyAsync();
                if (missingOPsymbols.Count == 0)
                {
                    results[5].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[5].Status = ValidationStatus.Failed;
                    foreach (string key in missingOPsymbols)
                    {
                        results[5].Errors.Add($"Missing point symbology: {key}");
                    }
                }

                //
                // Check for any duplicate OP symbols
                //
                List<string> duplicateOPsymbols = await GetDuplicateOrientationPointsSymbologyAsync();
                if (duplicateOPsymbols.Count == 0)
                {
                    results[6].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[6].Status = ValidationStatus.Failed;
                    foreach (string key in duplicateOPsymbols)
                    {
                        results[6].Errors.Add($"Duplicate point symbology: {key}");
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Rebuild the CF symbols from the symbology table
        /// </summary>
        /// <returns>Returns async task</returns>
        public static async Task RefreshCFSymbolOptionsAsync()
        {
            List<GemsSymbol> cfSymbols = new List<GemsSymbol>();

            StandaloneTable SymbologyTable = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "Symbology");

            // Return an empty list if the cfsymbology table isn null
            if (SymbologyTable == null)
            {
                GeomapmakerModule.ContactsAndFaultsSymbols = cfSymbols;
                return;
            }

            ProgressorSource ps = new ProgressorSource("Rebuilding Line Symbol Options...");

            // Process the cfsymbology table
            await QueuedTask.Run(() =>
            {
                using (Table enterpriseTable = SymbologyTable.GetTable())
                {
                    if (enterpriseTable != null)
                    {
                        QueryFilter queryFilter = new QueryFilter
                        {
                            WhereClause = "SymbolType = 'Line'",
                            PostfixClause = "ORDER BY key"
                        };

                        try
                        {
                            using (RowCursor rowCursor = enterpriseTable.Search(queryFilter))
                            {
                                while (rowCursor.MoveNext())
                                {
                                    using (Row row = rowCursor.Current)
                                    {
                                        GemsSymbol cfS = new GemsSymbol
                                        {
                                            Key = Helpers.RowValueToString(row["key"]),
                                            Description = Helpers.RowValueToString(row["description"]),
                                            SymbolJson = Helpers.RowValueToString(row["symbol"])
                                        };

                                        try
                                        {
                                            // Create the preview image used in the ComboBox
                                            SymbolStyleItem sSI = new SymbolStyleItem()
                                            {
                                                Symbol = CIMSymbolReference.FromJson(cfS.SymbolJson).Symbol,
                                                PatchWidth = 250,
                                                PatchHeight = 25,
                                                SymbolPatchType = SymbolPatchType.HorizontalLine
                                            };
                                            cfS.Preview = sSI.PreviewImage;

                                            // Add to list
                                            cfSymbols.Add(cfS);
                                        }
                                        catch
                                        {
                                            // Invalid CIM Symbol JSON
                                            Debug.WriteLine("Error prrocessing CIM Symbol JSON");
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                }
            }, ps.Progressor);

            GeomapmakerModule.ContactsAndFaultsSymbols = cfSymbols;
        }

        /// <summary>
        /// Rebuild the OP symbols from the symbology table
        /// </summary>
        /// <returns>Returns async task</returns>
        public static async Task RefreshOPSymbolOptionsAsync()
        {
            List<GemsSymbol> orientationSymbols = new List<GemsSymbol>();

            StandaloneTable orientationSymbologyTable = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "Symbology");

            // Check if the table exists
            if (orientationSymbologyTable == null)
            {
                GeomapmakerModule.OrientationPointSymbols = orientationSymbols;
                return;
            }

            ProgressorSource ps = new ProgressorSource("Rebuilding Point Symbol Options...");

            // Process the cfsymbology table
            await QueuedTask.Run(() =>
            {
                using (Table enterpriseTable = orientationSymbologyTable.GetTable())
                {
                    if (enterpriseTable != null)
                    {
                        QueryFilter queryFilter = new QueryFilter
                        {
                            WhereClause = "SymbolType = 'Point'",
                            PostfixClause = "ORDER BY key"
                        };

                        try
                        {
                            using (RowCursor rowCursor = enterpriseTable.Search(queryFilter))
                            {
                                while (rowCursor.MoveNext())
                                {
                                    using (Row row = rowCursor.Current)
                                    {
                                        GemsSymbol symbol = new GemsSymbol
                                        {
                                            Key = Helpers.RowValueToString(row["key"]),
                                            Description = Helpers.RowValueToString(row["description"]),
                                            SymbolJson = Helpers.RowValueToString(row["symbol"])
                                        };

                                        try
                                        {
                                            // Create the preview image used in the ComboBox
                                            SymbolStyleItem sSI = new SymbolStyleItem()
                                            {
                                                Symbol = CIMSymbolReference.FromJson(symbol.SymbolJson).Symbol,
                                                PatchWidth = 25,
                                                PatchHeight = 25,
                                                SymbolPatchType = SymbolPatchType.Default
                                            };
                                            symbol.Preview = sSI.PreviewImage;

                                            // Add to list
                                            orientationSymbols.Add(symbol);
                                        }
                                        catch
                                        {
                                            // Invalid CIM Symbol JSON
                                            Debug.WriteLine("Error prrocessing CIM Symbol JSON");
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                }
            }, ps.Progressor);

            GeomapmakerModule.OrientationPointSymbols = orientationSymbols;
        }

        /// <summary>
        /// Check for any missing line symbols in the symbology table
        /// </summary>
        /// <returns>List of missing symbology keys</returns>
        public static async Task<List<string>> GetMissingContactsAndFaultsSymbologyAsync()
        {
            List<string> missingSymbol = new List<string>();

            // Check if the ContactsAndFaultsSymbols have been processed
            if (GeomapmakerModule.ContactsAndFaultsSymbols == null)
            {
                await RefreshCFSymbolOptionsAsync();
            }

            // Get the symbol values from the CF layer
            List<string> cfSymbolValues = await AnyFeatureLayer.GetDistinctValuesForFieldAsync("ContactsAndFaults", "symbol");

            // Loop over the CF symbols
            foreach (string symbol in cfSymbolValues)
            {
                // Check if symbology exists
                if (!GeomapmakerModule.ContactsAndFaultsSymbols.Any(a => a.Key == symbol))
                {
                    missingSymbol.Add(symbol);
                }
            }

            return missingSymbol;
        }

        /// <summary>
        /// Check for any duplicate line symbols in the symbology table
        /// </summary>
        /// <returns>List of duplicate symbology keys</returns>
        public static async Task<List<string>> GetDuplicateContactsAndFaultsSymbologyAsync()
        {
            List<string> duplicateSymbol = new List<string>();

            // Check if the ContactsAndFaultsSymbols have been processed
            if (GeomapmakerModule.ContactsAndFaultsSymbols == null)
            {
                await RefreshCFSymbolOptionsAsync();
            }

            return GeomapmakerModule.ContactsAndFaultsSymbols.Select(a => a.Key).GroupBy(a => a).Where(b => b.Count() > 1).Select(c => c.Key).ToList();
        }

        /// <summary>
        /// Check for any missing point symbols in the symbology table
        /// </summary>
        /// <returns>List of missing symbology keys</returns>
        public static async Task<List<string>> GetMissingOrientationPointsSymbologyAsync()
        {
            List<string> missingSymbol = new List<string>();

            // Check if the OrientationPoints have been processed
            if (GeomapmakerModule.OrientationPointSymbols == null)
            {
                await RefreshOPSymbolOptionsAsync();
            }

            // Get the symbol values from the OP
            List<string> opSymbolValues = await AnyFeatureLayer.GetDistinctValuesForFieldAsync("OrientationPoints", "symbol");

            // Loop over the symbols
            foreach (string symbol in opSymbolValues)
            {
                // Check if symbology exists
                if (!GeomapmakerModule.OrientationPointSymbols.Any(a => a.Key == symbol))
                {
                    missingSymbol.Add(symbol);
                }
            }

            return missingSymbol;
        }

        /// <summary>
        /// Check for any duplicate point symbols in the symbology table
        /// </summary>
        /// <returns>List of duplicate symbology keys</returns>
        public static async Task<List<string>> GetDuplicateOrientationPointsSymbologyAsync()
        {
            List<string> duplicateSymbol = new List<string>();

            // Check if the OrientationPoints have been processed
            if (GeomapmakerModule.OrientationPointSymbols == null)
            {
                await RefreshOPSymbolOptionsAsync();
            }

            return GeomapmakerModule.OrientationPointSymbols.Select(a => a.Key).GroupBy(a => a).Where(b => b.Count() > 1).Select(c => c.Key).ToList();
        }
    }
}
