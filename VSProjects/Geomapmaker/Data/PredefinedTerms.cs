using Geomapmaker.Models;
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
            string definition = await AnyStandaloneTable.GetValueFromWhereClauseAsync("PredefinedTerms", $"DatasetName = '{datasetName}' AND FieldName = '{fieldName}' AND Term = '{term}'", "Definition");

            return new GlossaryTerm { DatasetName = datasetName, FieldName = fieldName, Term = term, Definition = definition };
        }
    }
}
