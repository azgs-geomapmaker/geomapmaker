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
        public static async Task<List<string>> FeatureLayerGetDistinctValuesForFieldAsync(string fieldName, string layerName)
        {
            List<string> uniqueValues = new List<string>();

            FeatureLayer layer = MapView.Active?.Map.FindLayers(layerName).FirstOrDefault() as FeatureLayer;

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
                                PrefixClause = "DISTINCT",
                                // Where not null or empty
                                WhereClause = $"{fieldName} <> ''",
                                SubFields = fieldName
                            };

                            using (RowCursor rowCursor = table.Search(queryFilter))
                            {
                                while (rowCursor.MoveNext())
                                {
                                    using (Row row = rowCursor.Current)
                                    {
                                        string fieldValue = row[fieldName]?.ToString();

                                        uniqueValues.Add(fieldValue);
                                    }
                                }
                            }
                        }
                    }
                });
            }

            return uniqueValues;
        }

        public static async Task<List<string>> StandaloneTableGetDistinctValuesForFieldAsync(string fieldName, string tableName)
        {
            List<string> uniqueValues = new List<string>();

            StandaloneTable standaloneTable = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == tableName);

            if (standaloneTable != null)
            {
                await QueuedTask.Run(() =>
                {
                    using (Table table = standaloneTable.GetTable())
                    {
                        if (table != null)
                        {
                            QueryFilter queryFilter = new QueryFilter
                            {
                                PrefixClause = "DISTINCT",
                                // Where not null or empty
                                WhereClause = $"{fieldName} <> ''",
                                SubFields = fieldName
                            };

                            using (RowCursor rowCursor = table.Search(queryFilter))
                            {
                                while (rowCursor.MoveNext())
                                {
                                    using (Row row = rowCursor.Current)
                                    {
                                        string fieldValue = row[fieldName]?.ToString();

                                        uniqueValues.Add(fieldValue);
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
