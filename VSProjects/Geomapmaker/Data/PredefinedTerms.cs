using Geomapmaker.Models;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public class PredefinedTerms
    {
        public static async Task<GlossaryTerm> GetPrepopulatedDefinitionAsync(string datasetName, string fieldName, string term)
        {
            string definition = await AnyStandaloneTable.GetValueFromWhereClauseAsync("PredefinedTerms", $"DatasetName = '{datasetName}' AND FieldName = '{fieldName}' AND Term = '{term}'", "Definition");

            return new GlossaryTerm { DatasetName = datasetName, FieldName = fieldName, Term = term, Definition = definition };
        }
    }
}
