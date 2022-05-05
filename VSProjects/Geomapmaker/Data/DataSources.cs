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
        /// <summary>
        /// Checks the data sources for any unused data sources
        /// </summary>
        /// <returns>List of unused data sources</returns>
        public static async Task<List<string>> GetUnnecessaryDataSources()
        {
            // List of all foreign keys
            List<string> foreignKeys = new List<string>();

            // Add DMU data sources
            foreignKeys.AddRange(await General.StandaloneTableGetDistinctValuesForFieldAsync("DescriptionOfMapUnits", "DescriptionSourceID"));

            // Add CF data sources
            foreignKeys.AddRange(await General.FeatureLayerGetDistinctValuesForFieldAsync("ContactsAndFaults", "DataSourceID"));

            // Add MUP data sources
            foreignKeys.AddRange(await General.FeatureLayerGetDistinctValuesForFieldAsync("MapUnitPolys", "datasourceid"));

            // Add Stations data sources
            foreignKeys.AddRange(await Stations.GetDistinctDataSourceIdValuesAsync());

            // Add OP data sources (location)
            foreignKeys.AddRange(await General.FeatureLayerGetDistinctValuesForFieldAsync("OrientationPoints", "locationsourceid"));

            // Add OP data sources (orientation)
            foreignKeys.AddRange(await General.FeatureLayerGetDistinctValuesForFieldAsync("OrientationPoints", "orientationsourceid"));

            // Get all datasources from Data Source table
            List<string> dataSources = await General.StandaloneTableGetDistinctValuesForFieldAsync("DataSources", "datasources_id");

            // Find unused data sources
            List<string> unusedDataSources = dataSources.Except(foreignKeys.Distinct()).ToList();

            return unusedDataSources;
        }

        /// <summary>
        /// Checks the data source table for any missing data sources
        /// </summary>
        /// <returns>List of missing data sources</returns>
        public static async Task<List<string>> GetMissingDataSources()
        {
            // List of all foreign keys
            List<string> foreignKeys = new List<string>();

            // Add DMU data sources
            foreignKeys.AddRange(await General.StandaloneTableGetDistinctValuesForFieldAsync("DescriptionOfMapUnits", "DescriptionSourceID"));

            // Add CF data sources
            foreignKeys.AddRange(await General.FeatureLayerGetDistinctValuesForFieldAsync("ContactsAndFaults", "DataSourceID"));

            // Add MUP data sources
            foreignKeys.AddRange(await General.FeatureLayerGetDistinctValuesForFieldAsync("MapUnitPolys", "datasourceid"));

            // Add Stations data sources
            foreignKeys.AddRange(await Stations.GetDistinctDataSourceIdValuesAsync());

            // Add OP data sources (location)
            foreignKeys.AddRange(await General.FeatureLayerGetDistinctValuesForFieldAsync("OrientationPoints", "locationsourceid"));

            // Add OP data sources (orientation)
            foreignKeys.AddRange(await General.FeatureLayerGetDistinctValuesForFieldAsync("OrientationPoints", "orientationsourceid"));

            // Get all datasources from Data Source table
            List<string> dataSources = await General.StandaloneTableGetDistinctValuesForFieldAsync("DataSources", "datasources_id");

            // Find missing data sources
            List<string> missingDataSources = foreignKeys.Except(dataSources).ToList();

            return missingDataSources;
        }

        /// <summary>
        /// Get list of DataSources
        /// </summary>
        /// <returns>List of DataSources</returns>
        public static async Task<List<DataSource>> GetDataSourcesAsync()
        {
            List<DataSource> DataSourcesList = new List<DataSource>();

            StandaloneTable dataSourcesTable = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "DataSources");

            if (dataSourcesTable == null)
            {
                return DataSourcesList;
            }

            await QueuedTask.Run(() =>
            {
                using (Table table = dataSourcesTable.GetTable())
                {
                    if (table == null)
                    {
                        return;
                    }

                    QueryFilter queryFilter = new QueryFilter
                    {
                        PostfixClause = "ORDER BY datasources_id"
                    };

                    using (RowCursor rowCursor = table.Search(queryFilter))
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
                }
            });

            return DataSourcesList;
        }

        /// <summary>
        /// Updates related tables when datasourceID changes 
        /// </summary>
        /// <param name="originalDataSourceID">old id value</param>
        /// <param name="newDataSourceID">new id value</param>
        public static void UpdateDataSourceForeignKeys(string originalDataSourceID, string newDataSourceID)
        {
            // Update DescriptionOfMapUnits
            QueuedTask.Run(() =>
            {
                StandaloneTable dmu = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "DescriptionOfMapUnits");

                if (dmu != null)
                {
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
                }
            });

            // Update MapUnitPolys
            QueuedTask.Run(() =>
            {
                FeatureLayer mup = MapView.Active?.Map.FindLayers("MapUnitPolys").FirstOrDefault() as FeatureLayer;

                if (mup != null)
                {
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
                }
            });

            // Update ContactsAndFaults
            QueuedTask.Run(() =>
            {
                FeatureLayer cf = MapView.Active?.Map.FindLayers("ContactsAndFaults").FirstOrDefault() as FeatureLayer;

                if (cf != null)
                {
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
                }
            });

            // Update ContactsAndFaults Templates
            QueuedTask.Run(() =>
            {
                FeatureLayer cf = MapView.Active?.Map.FindLayers("ContactsAndFaults").FirstOrDefault() as FeatureLayer;

                if (cf != null)
                {
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
                }
            });

            // Update Stations
            QueuedTask.Run(() =>
            {
                FeatureLayer stationsLayer = MapView.Active?.Map.FindLayers("Stations").FirstOrDefault() as FeatureLayer;

                if (stationsLayer != null)
                {
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
                }
            });

            // Update OrientationPoints (LocationSourceID)
            QueuedTask.Run(() =>
            {
                FeatureLayer opLayer = MapView.Active?.Map.FindLayers("OrientationPoints").FirstOrDefault() as FeatureLayer;

                if (opLayer != null)
                {
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
                }
            });

            // Update OrientationPoints (OrientationSourceID)
            QueuedTask.Run(() =>
            {
                FeatureLayer opLayer = MapView.Active?.Map.FindLayers("OrientationPoints").FirstOrDefault() as FeatureLayer;

                if (opLayer != null)
                {
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
                }
            });
        }
    }
}
