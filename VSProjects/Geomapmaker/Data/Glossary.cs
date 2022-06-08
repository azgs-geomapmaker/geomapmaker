using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public class Glossary
    {
        public static async Task<List<UndefinedTerm>> GetUndefinedGlossaryTerms()
        {
            List<UndefinedTerm> undefinedTerms = new List<UndefinedTerm>();

            List<string> glossaryTerms = await GetGlossaryTermsAsync();

            // DescriptionOfMapUnits
            undefinedTerms.AddRange(await DescriptionOfMapUnits.GetTermsUndefinedInGlossaryAsync(glossaryTerms));

            // ContactsAndFaults
            undefinedTerms.AddRange(await ContactsAndFaults.GetTermsUndefinedInGlossaryAsync(glossaryTerms));

            // MapUnitPolys
            undefinedTerms.AddRange(await MapUnitPolys.GetTermsUndefinedInGlossaryAsync(glossaryTerms));

            // OrientationPoints
            undefinedTerms.AddRange(await OrientationPoints.GetTermsUndefinedInGlossaryAsync(glossaryTerms));

            return undefinedTerms;
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
