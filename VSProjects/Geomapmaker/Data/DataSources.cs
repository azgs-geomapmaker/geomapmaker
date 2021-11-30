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

                using (RowCursor rowCursor = enterpriseTable.Search())
                {
                    while (rowCursor.MoveNext())
                    {
                        using (Row row = rowCursor.Current)
                        {
                            DataSource dS = new DataSource
                            {
                                ObjecttId = long.Parse(row["objectid"].ToString()),
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

        public static List<DataSource> DataSourcesList { get; set; } = new List<DataSource>();

        public static async Task RefreshAsync()
        {
            List<DataSource> DataSourcesList = new List<DataSource>();

            if (DbConnectionProperties.GetProperties() == null)
            {
                return;
            }

            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                using (Geodatabase geodatabase = new Geodatabase(DbConnectionProperties.GetProperties()))
                {

                    QueryDef dsQDef = new QueryDef
                    {
                        Tables = "DataSources",
                        PostfixClause = "order by source"
                    };

                    using (RowCursor rowCursor = geodatabase.Evaluate(dsQDef, false))
                    {
                        while (rowCursor.MoveNext())
                        {
                            using (Row row = rowCursor.Current)
                            {
                                DataSource dS = new DataSource
                                {
                                    ObjecttId = long.Parse(row["objectid"].ToString()),
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

                }
            });
        }
    }
}
