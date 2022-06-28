using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public class Glossary
    {
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
                new ValidationRule{ Description="No empty/null values in required fields."},
                new ValidationRule{ Description="No duplicate ids."},
                new ValidationRule{ Description="No duplicate terms."},
                new ValidationRule{ Description="No missing glossary terms in DescriptionOfMapUnits."},
                new ValidationRule{ Description="No missing glossary terms in ContactsAndFaults."},
                new ValidationRule{ Description="No missing glossary terms in MapUnitPolys."},
                new ValidationRule{ Description="No missing glossary terms in OrientationPoints."}
            };

            if (await AnyStandaloneTable.DoesTableExistsAsync("Glossary") == false)
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
                int tableCount = AnyStandaloneTable.GetTableCount("Glossary");
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
                // Check table for any missing fields 
                //

                // List of fields to check for
                List<string> glossaryRequiredFields = new List<string>() { "term", "definition", "definitionsourceid", "glossary_id" };

                // Get list of missing fields
                List<string> missingFields = await AnyStandaloneTable.GetMissingFieldsAsync("Glossary", glossaryRequiredFields);
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

                // List of fields that can't have nulls
                List<string> glossaryNotNUll = new List<string>() { "term", "definition", "definitionsourceid", "glossary_id" };

                // Get the required fields with a null
                List<string> fieldsWithMissingValues = await AnyStandaloneTable.GetRequiredFieldIsNullAsync("Glossary", glossaryNotNUll);
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
                // Check for any duplicate ids
                //
                List<string> duplicateIds = await AnyStandaloneTable.GetDuplicateValuesInFieldAsync("Glossary", "Glossary_ID");
                if (duplicateIds.Count == 0)
                {
                    results[4].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[4].Status = ValidationStatus.Failed;
                    foreach (string id in duplicateIds)
                    {
                        results[4].Errors.Add($"Duplicate glossary_id: {id}");
                    }
                }

                //
                // Check for any duplicate terms
                //
                List<string> duplicateTerms = await AnyStandaloneTable.GetDuplicateValuesInFieldAsync("Glossary", "term");
                if (duplicateTerms.Count == 0)
                {
                    results[5].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[5].Status = ValidationStatus.Failed;
                    foreach (string term in duplicateTerms)
                    {
                        results[5].Errors.Add($"Duplicate term: {term}");
                    }
                }

                // Find Undefined terms
                Tuple<List<string>, List<GlossaryTerm>> tuple = await GetUndefinedGlossaryTerms();

                // Get the list of undefined terms from the tuple
                List<GlossaryTerm> undefinedTerms = tuple.Item2;

                //
                // Check DescriptionOfMapUnits for missing undefined glossary terms
                //
                List<GlossaryTerm> dmuTerms = undefinedTerms.Where(a => a.DatasetName == "DescriptionOfMapUnits").ToList();
                if (dmuTerms.Count == 0)
                {
                    results[6].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[6].Status = ValidationStatus.Failed;
                    foreach (GlossaryTerm undefinedTerm in dmuTerms)
                    {
                        results[6].Errors.Add($"Missing {undefinedTerm.DatasetName} glossary term in {undefinedTerm.FieldName} field: {undefinedTerm.Term}");
                    }
                }

                //
                // Check ContactsAndFaults for missing undefined glossary terms
                //
                List<GlossaryTerm> cfTerms = undefinedTerms.Where(a => a.DatasetName == "ContactsAndFaults").ToList();
                if (cfTerms.Count == 0)
                {
                    results[7].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[7].Status = ValidationStatus.Failed;
                    foreach (GlossaryTerm undefinedTerm in cfTerms)
                    {
                        results[7].Errors.Add($"Missing {undefinedTerm.DatasetName} glossary term in {undefinedTerm.FieldName} field: {undefinedTerm.Term}");
                    }
                }

                //
                // Check MapUnitPolys for missing undefined glossary terms
                //
                List<GlossaryTerm> mupTerms = undefinedTerms.Where(a => a.DatasetName == "MapUnitPolys").ToList();
                if (mupTerms.Count == 0)
                {
                    results[8].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[8].Status = ValidationStatus.Failed;
                    foreach (GlossaryTerm undefinedTerm in mupTerms)
                    {
                        results[8].Errors.Add($"Missing {undefinedTerm.DatasetName} glossary term in {undefinedTerm.FieldName} field: {undefinedTerm.Term}");
                    }
                }

                //
                // Check OrientationPoints for missing undefined glossary terms
                //
                List<GlossaryTerm> opTerms = undefinedTerms.Where(a => a.DatasetName == "OrientationPoints").ToList();
                if (opTerms.Count == 0)
                {
                    results[9].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[9].Status = ValidationStatus.Failed;
                    foreach (GlossaryTerm undefinedTerm in opTerms)
                    {
                        results[9].Errors.Add($"Missing {undefinedTerm.DatasetName} glossary term in {undefinedTerm.FieldName} field: {undefinedTerm.Term}");
                    }
                }
            }

            return results;
        }

        public static async Task<Tuple<List<string>, List<GlossaryTerm>>> GetUndefinedGlossaryTerms()
        {
            List<GlossaryTerm> undefinedTerms = new List<GlossaryTerm>();

            List<string> glossaryTerms = await GetGlossaryTermsAsync();

            // DescriptionOfMapUnits
            undefinedTerms.AddRange(await DescriptionOfMapUnits.GetTermsUndefinedInGlossaryAsync(glossaryTerms));

            // ContactsAndFaults
            undefinedTerms.AddRange(await ContactsAndFaults.GetTermsUndefinedInGlossaryAsync(glossaryTerms));

            // MapUnitPolys
            undefinedTerms.AddRange(await MapUnitPolys.GetTermsUndefinedInGlossaryAsync(glossaryTerms));

            // OrientationPoints
            undefinedTerms.AddRange(await OrientationPoints.GetTermsUndefinedInGlossaryAsync(glossaryTerms));

            return Tuple.Create(glossaryTerms, undefinedTerms);
        }

        public static async Task<List<string>> GetGlossaryTermsAsync()
        {
            List<string> terms = new List<string>();

            StandaloneTable standalone = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "Glossary");

            if (standalone == null)
            {
                return terms;
            }

            await QueuedTask.Run(() =>
            {
                using (Table table = standalone.GetTable())
                {
                    if (table == null)
                    {
                        return;
                    }

                    QueryFilter queryFilter = new QueryFilter
                    {
                        SubFields = "Term"
                    };

                    using (RowCursor rowCursor = table.Search(queryFilter))
                    {
                        while (rowCursor.MoveNext())
                        {
                            using (Row row = rowCursor.Current)
                            {
                                terms.Add(row["Term"]?.ToString());
                            }
                        }
                    }
                }
            });

            return terms;
        }
    }
}
