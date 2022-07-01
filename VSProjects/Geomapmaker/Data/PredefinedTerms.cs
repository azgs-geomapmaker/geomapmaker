using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public class PredefinedTerms
    {
        /// <summary>
        /// Get predefined terms from the standalone table.
        /// </summary>
        /// <param name="datasetName"></param>
        /// <param name="fieldName"></param>
        /// <param name="term"></param>
        /// <returns>GlossarTerm with the predefined definition or an empty string if it is not found.</returns>
        public static async Task<GlossaryTerm> GetPrepopulatedDefinitionAsync(string datasetName, string fieldName, string term)
        {   
            // If term is empty don't bother looking it up
            if (string.IsNullOrEmpty(term))
            {
                return new GlossaryTerm();
            }

            StringBuilder query = new StringBuilder();

            if (!string.IsNullOrEmpty(datasetName))
            {
                query.Append($"DatasetName = '{datasetName}' AND ");
            }

            if (!string.IsNullOrEmpty(fieldName))
            {
                query.Append($"FieldName = '{fieldName}' AND ");
            }

            query.Append($"Term = '{term}'");

            string definition = await AnyStandaloneTable.GetValueFromWhereClauseAsync("PredefinedTerms", query.ToString(), "Definition");

            return new GlossaryTerm { DatasetName = datasetName, FieldName = fieldName, Term = term, Definition = definition };
        }

        public static async Task<List<GlossaryTerm>> GetPredefinedDictionaryAsync()
        {
            List<GlossaryTerm> terms = new List<GlossaryTerm>();

            StandaloneTable standalone = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "PredefinedTerms");

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

                    using (RowCursor rowCursor = table.Search())
                    {
                        while (rowCursor.MoveNext())
                        {
                            using (Row row = rowCursor.Current)
                            {
                                GlossaryTerm term = new GlossaryTerm
                                {
                                    DatasetName = row["DatasetName"]?.ToString(),
                                    FieldName = row["FieldName"]?.ToString(),
                                    Term = row["Term"]?.ToString(),
                                    Definition = row["Definition"]?.ToString(),
                                };

                                terms.Add(term);
                            }
                        }
                    }
                }
            });

            return terms;
        }
    }
}
