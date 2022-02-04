using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public static class DataSources
    {
        public static async Task<List<DataSource>> GetDataSourcesAsync()
        {
            List<DataSource> DataSourcesList = new List<DataSource>();

            StandaloneTable dataSourcesTable = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "DataSources");

            if (dataSourcesTable == null)
            {
                return DataSourcesList;
            }

            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                Table enterpriseTable = dataSourcesTable.GetTable();

                if (enterpriseTable == null)
                {
                    return;
                }

                QueryFilter queryFilter = new QueryFilter
                {
                    PostfixClause = "ORDER BY datasources_id"
                };

                using (RowCursor rowCursor = enterpriseTable.Search(queryFilter))
                {
                    while (rowCursor.MoveNext())
                    {
                        using (Row row = rowCursor.Current)
                        {
                            DataSource dS = new DataSource
                            {
                                ObjectId = long.Parse(row["objectid"].ToString()),
                                Source = row["source"]?.ToString(),
                                DataSource_ID = row["datasources_id"]?.ToString(),
                                Url = row["url"]?.ToString(),
                                Notes = row["notes"]?.ToString()
                            };

                            //add it to our list
                            DataSourcesList.Add(dS);
                        }
                    }
                }
            });

            return DataSourcesList;
        }
    }
}
