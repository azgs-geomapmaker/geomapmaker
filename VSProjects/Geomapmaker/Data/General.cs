using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public class General
    {
        public static async Task<List<string>> GetUniqueValuesForFieldAsync(string fieldName, string tableName)
        {
            List<string> uniqueValues = new List<string>();

            FeatureLayer layer = MapView.Active?.Map.FindLayers(tableName).FirstOrDefault() as FeatureLayer;

            if (layer != null)
            {
                await QueuedTask.Run(() =>
                {
                    using (Table table = layer.GetTable())
                    {
                        QueryFilter queryFilter = new QueryFilter
                        {
                            SubFields = fieldName
                        };

                        using (RowCursor rowCursor = table.Search(queryFilter))
                        {
                            while (rowCursor.MoveNext())
                            {
                                using (Row row = rowCursor.Current)
                                {
                                    if (row[fieldName] != null)
                                    {
                                        string fieldValue = row[fieldName]?.ToString();

                                        if (!string.IsNullOrEmpty(fieldValue) && !uniqueValues.Contains(fieldValue))
                                        {
                                            uniqueValues.Add(fieldValue);
                                        }
                                    }
                                }
                            }
                        }
                    }
                });
            }

            return uniqueValues;
        }
    }
}
