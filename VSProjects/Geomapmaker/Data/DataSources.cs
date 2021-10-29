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
        public static List<DataSource> DataSourcesList { get; set; } = new List<DataSource>();

        public static async Task RefreshAsync()
        {
            List<DataSource> DataSourcesList = new List<DataSource>();

            IEnumerable<GDBProjectItem> gdbProjectItems = Project.Current.GetItems<GDBProjectItem>();

            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {

                foreach (GDBProjectItem gdbProjectItem in gdbProjectItems)
                {
                    using (Datastore datastore = gdbProjectItem.GetDatastore())
                    {
                        //Unsupported datastores (non File GDB and non Enterprise GDB) will be of type UnknownDatastore
                        if (datastore is UnknownDatastore)
                            continue;

                        Geodatabase geodatabase = datastore as Geodatabase;

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
                                        Source = row["source"].ToString(),
                                        DataSource_ID = row["datasources_id"].ToString(),
                                        Url = row["url"]?.ToString(),
                                        Notes = row["notes"]?.ToString()
                                    };

                                    DataSourcesList.Add(dS);
                                }
                            }
                        }
                    }

                }






                //using (Geodatabase geodatabase = new Geodatabase())
                //{
                //    QueryDef dsQDef = new QueryDef
                //    {
                //        Tables = "DataSources",
                //        PostfixClause = "order by source"
                //    };

                //    using (RowCursor rowCursor = geodatabase.Evaluate(dsQDef, false))
                //    {
                //        while (rowCursor.MoveNext())
                //        {
                //            using (Row row = rowCursor.Current)
                //            {
                //                DataSource dS = new DataSource
                //                {
                //                    ID = long.Parse(row["objectid"].ToString()),
                //                    Source = row["source"].ToString(),
                //                    DataSource_ID = row["datasources_id"].ToString(),
                //                    Url = row["url"]?.ToString(),
                //                    Notes = row["notes"]?.ToString()
                //                };

                //                DataSourcesList.Add(dS);
                //            }
                //        }
                //    }
                //}
            });
        }
    }
}
