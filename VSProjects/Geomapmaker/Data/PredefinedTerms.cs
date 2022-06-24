using Geomapmaker.Models;
using System.Collections.Generic;
using System.Linq;

namespace Geomapmaker.Data
{
    public class PredefinedTerms
    {
        public static GlossaryTerm GetPrepopulatedDefinition(string datasetName, string fieldName, string term)
        {
            GlossaryTerm prepop = PrepopulateTermDefinitions.FirstOrDefault(a => a.DatasetName == datasetName && a.FieldName == fieldName && a.Term == term);

            if (prepop == null)
            {
                return new GlossaryTerm { DatasetName = datasetName, FieldName = fieldName, Term = term };
            }

            return prepop;
        }

        public static List<GlossaryTerm> PrepopulateTermDefinitions => new List<GlossaryTerm>()
        {
            new GlossaryTerm { DatasetName = "DescriptionOfMapUnits", FieldName = "ParagraphStyle", Term = "Heading2", Definition = "Prepop ParagraphStyle test" },
            new GlossaryTerm { DatasetName = "DescriptionOfMapUnits", FieldName = "GeoMaterialConfidence", Term = "Low", Definition = "Prepop GeoMaterialConfidence test" },
        };
    }
}
