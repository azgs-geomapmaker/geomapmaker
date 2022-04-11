using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Editing.Templates;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public class DataSources
    {
        public static async Task<List<string>> GetDataSourceIdsAsync()
        {
            List<string> DataSourcesIds = new List<string>();

            StandaloneTable dataSourcesTable = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "DataSources");

            if (dataSourcesTable == null)
            {
                return DataSourcesIds;
            }

            await QueuedTask.Run(() =>
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
                            DataSourcesIds.Add(row["datasources_id"]?.ToString());
                        }
                    }
                }
            });

            return DataSourcesIds;
        }

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

        public static void UpdateDataSourceForeignKeys(string originalDataSourceID, string newDataSourceID)
        {
            // Update DescriptionOfMapUnits
            QueuedTask.Run(() =>
            {
                StandaloneTable dmu = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "DescriptionOfMapUnits");

                QueryFilter queryFilter = new QueryFilter
                {
                    // Where DataSourceID is set to the original value
                    WhereClause = $"DescriptionSourceID = '{originalDataSourceID}'"
                };

                // Create list of oids to update
                List<long> oidSet = new List<long>();

                using (RowCursor rc = dmu.Search(queryFilter))
                {
                    while (rc.MoveNext())
                    {
                        using (Row record = rc.Current)
                        {
                            oidSet.Add(record.GetObjectID());
                        }
                    }
                }

                // Create the edit operation
                EditOperation modifyFeatures = new EditOperation
                {
                    Name = "Update DescriptionSourceID for DescriptionOfMapUnits",
                    ShowProgressor = true
                };

                Inspector multipleFeaturesInsp = new Inspector();

                multipleFeaturesInsp.Load(dmu, oidSet);

                multipleFeaturesInsp["DescriptionSourceID"] = newDataSourceID;

                modifyFeatures.Modify(multipleFeaturesInsp);

                modifyFeatures.Execute();
            });

            // Update MapUnitPolys
            QueuedTask.Run(() =>
            {
                FeatureLayer mup = MapView.Active?.Map.FindLayers("MapUnitPolys").FirstOrDefault() as FeatureLayer;

                QueryFilter queryFilter = new QueryFilter
                {
                    // Where DataSourceID is set to the original value
                    WhereClause = $"DataSourceID = '{originalDataSourceID}'"
                };

                // Create list of oids to update
                List<long> oidSet = new List<long>();

                using (RowCursor rc = mup.Search(queryFilter))
                {
                    while (rc.MoveNext())
                    {
                        using (Row record = rc.Current)
                        {
                            oidSet.Add(record.GetObjectID());
                        }
                    }
                }

                // Create the edit operation
                EditOperation modifyFeatures = new EditOperation
                {
                    Name = "Update DataSourceID for MapUnitPolys",
                    ShowProgressor = true
                };

                Inspector multipleFeaturesInsp = new Inspector();

                multipleFeaturesInsp.Load(mup, oidSet);

                multipleFeaturesInsp["DataSourceID"] = newDataSourceID;

                modifyFeatures.Modify(multipleFeaturesInsp);

                modifyFeatures.Execute();
            });

            // Update ContactsAndFaults
            QueuedTask.Run(() =>
            {
                FeatureLayer cf = MapView.Active?.Map.FindLayers("ContactsAndFaults").FirstOrDefault() as FeatureLayer;

                QueryFilter queryFilter = new QueryFilter
                {
                    // Where DataSourceID is set to the original value
                    WhereClause = $"DataSourceID = '{originalDataSourceID}'"
                };

                // Create list of oids to update
                List<long> oidSet = new List<long>();

                using (RowCursor rc = cf.Search(queryFilter))
                {
                    while (rc.MoveNext())
                    {
                        using (Row record = rc.Current)
                        {
                            oidSet.Add(record.GetObjectID());
                        }
                    }
                }

                // Create the edit operation
                EditOperation modifyFeatures = new EditOperation
                {
                    Name = "Update DataSourceID for ContactsAndFaults",
                    ShowProgressor = true
                };

                Inspector multipleFeaturesInsp = new Inspector();

                multipleFeaturesInsp.Load(cf, oidSet);

                multipleFeaturesInsp["DataSourceID"] = newDataSourceID;

                modifyFeatures.Modify(multipleFeaturesInsp);

                modifyFeatures.Execute();
            });

            QueuedTask.Run(() =>
            {
                FeatureLayer cf = MapView.Active?.Map.FindLayers("ContactsAndFaults").FirstOrDefault() as FeatureLayer;

                IEnumerable<EditingTemplate> cfTemplates = cf.GetTemplates();

                foreach (var template in cfTemplates)
                {
                    var templateDef = template.GetDefinition() as CIMFeatureTemplate;

                    if (templateDef.DefaultValues["datasourceid"]?.ToString() == originalDataSourceID)
                    {
                        // Update datasourceid
                        templateDef.DefaultValues["datasourceid"] = newDataSourceID;

                        template.SetDefinition(templateDef);
                    }
                }
            });

            // Update Stations
            QueuedTask.Run(() =>
            {
                FeatureLayer stationsLayer = MapView.Active?.Map.FindLayers("Stations").FirstOrDefault() as FeatureLayer;

                QueryFilter queryFilter = new QueryFilter
                {
                    // Where DataSourceID is set to the original value
                    WhereClause = $"DataSourceID = '{originalDataSourceID}'"
                };

                // Create list of oids to update
                List<long> oidSet = new List<long>();

                using (RowCursor rc = stationsLayer.Search(queryFilter))
                {
                    while (rc.MoveNext())
                    {
                        using (Row record = rc.Current)
                        {
                            oidSet.Add(record.GetObjectID());
                        }
                    }
                }

                // Create the edit operation
                EditOperation modifyFeatures = new EditOperation
                {
                    Name = "Update DataSourceID for Stations",
                    ShowProgressor = true
                };

                Inspector multipleFeaturesInsp = new Inspector();

                multipleFeaturesInsp.Load(stationsLayer, oidSet);

                multipleFeaturesInsp["DataSourceID"] = newDataSourceID;

                modifyFeatures.Modify(multipleFeaturesInsp);

                modifyFeatures.Execute();
            });

            // Update OrientationPoints (LocationSourceID)
            QueuedTask.Run(() =>
            {
                FeatureLayer opLayer = MapView.Active?.Map.FindLayers("OrientationPoints").FirstOrDefault() as FeatureLayer;

                QueryFilter queryFilter = new QueryFilter
                {
                    // Where DataSourceID is set to the original value
                    WhereClause = $"LocationSourceID = '{originalDataSourceID}'"
                };

                // Create list of oids to update
                List<long> oidSet = new List<long>();

                using (RowCursor rc = opLayer.Search(queryFilter))
                {
                    while (rc.MoveNext())
                    {
                        using (Row record = rc.Current)
                        {
                            oidSet.Add(record.GetObjectID());
                        }
                    }
                }

                // Create the edit operation
                EditOperation modifyFeatures = new EditOperation
                {
                    Name = "Update LocationSourceID for Orientation Points",
                    ShowProgressor = true
                };

                Inspector multipleFeaturesInsp = new Inspector();

                multipleFeaturesInsp.Load(opLayer, oidSet);

                multipleFeaturesInsp["LocationSourceID"] = newDataSourceID;

                modifyFeatures.Modify(multipleFeaturesInsp);

                modifyFeatures.Execute();
            });

            // Update OrientationPoints (OrientationSourceID)
            QueuedTask.Run(() =>
            {
                FeatureLayer opLayer = MapView.Active?.Map.FindLayers("OrientationPoints").FirstOrDefault() as FeatureLayer;

                QueryFilter queryFilter = new QueryFilter
                {
                    // Where DataSourceID is set to the original value
                    WhereClause = $"OrientationSourceID = '{originalDataSourceID}'"
                };

                // Create list of oids to update
                List<long> oidSet = new List<long>();

                using (RowCursor rc = opLayer.Search(queryFilter))
                {
                    while (rc.MoveNext())
                    {
                        using (Row record = rc.Current)
                        {
                            oidSet.Add(record.GetObjectID());
                        }
                    }
                }

                // Create the edit operation
                EditOperation modifyFeatures = new EditOperation
                {
                    Name = "Update LocationSourceID for Orientation Points",
                    ShowProgressor = true
                };

                Inspector multipleFeaturesInsp = new Inspector();

                multipleFeaturesInsp.Load(opLayer, oidSet);

                multipleFeaturesInsp["OrientationSourceID"] = newDataSourceID;

                modifyFeatures.Modify(multipleFeaturesInsp);

                modifyFeatures.Execute();
            });
        }
    }
}
