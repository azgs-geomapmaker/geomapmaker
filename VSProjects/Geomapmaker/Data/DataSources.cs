using ArcGIS.Core.Data;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public static class DataSources
    {
        // User's DS for the geomapmaker tool. Hiding this here for now.
        public static string DataSourceId { get; set; }

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
                                    ID = long.Parse(row["objectid"].ToString()),
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
