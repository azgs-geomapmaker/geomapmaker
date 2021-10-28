using ArcGIS.Core.Data;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public class DataSources
    {
        public async Task<List<DataSource>> GetDatasourcesAsync()
        {
            if (DbConnectionProperties.GetProperties() == null)
            {
                return null;
            }

            List<DataSource> DataSources = new List<DataSource>();

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
                                //create and load map unit
                                DataSource dS = new DataSource
                                {
                                    ID = long.Parse(row["objectid"].ToString()),
                                    Source = row["source"].ToString(),
                                    DataSource_ID = row["datasources_id"].ToString(),
                                    Url = row["url"]?.ToString(),
                                    Notes = row["notes"]?.ToString()
                                };

                                DataSources.Add(dS);
                            }
                        }
                    }
                }
            });

            return DataSources;
        }
    }
}
