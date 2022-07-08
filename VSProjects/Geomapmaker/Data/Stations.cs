using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker._helpers;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public class Stations
    {
        /// <summary>
        /// Get validation report for layer
        /// </summary>
        /// <returns>List of Validation results</returns>
        public static async Task<List<ValidationRule>> GetValidationResultsAsync()
        {
            List<ValidationRule> results = new List<ValidationRule>
            {
                new ValidationRule{ Description="Layer exists."},
                new ValidationRule{ Description="No duplicate layers."},
                new ValidationRule{ Description="No missing fields."},
                new ValidationRule{ Description="No empty/null values in required fields."},
                new ValidationRule{ Description="No duplicate Stations_ID values."},
            };

            if (await AnyFeatureLayer.DoesLayerExistsAsync("Stations") == false)
            {
                results[0].Status = ValidationStatus.Skipped;
                results[0].Errors.Add("Layer not found");
                return results;
            }
            else // Table was found
            {
                results[0].Status = ValidationStatus.Passed;

                //
                // Check for duplicate layers
                //
                int tableCount = AnyFeatureLayer.GetLayerCount("Stations");
                if (tableCount > 1)
                {
                    results[1].Status = ValidationStatus.Failed;
                    results[1].Errors.Add($"{tableCount} layers found");
                }
                else
                {
                    results[1].Status = ValidationStatus.Passed;
                }

                //
                // Check table for any missing fields 
                //

                // List of fields to check 
                List<string> stationRequiredFields = new List<string>() { "fieldid", "locationconfidencemeters", "observedmapunit", "mapunit", "symbol", "label", "plotatscale",
                "datasourceid", "notes", "locationmethod", "timedate", "observer", "significantdimensionmeters", "gpsx", "gpsy", "pdop", "mapx", "mapy", "stations_id" };

                // Get list of missing fields
                List<string> missingFields = await AnyFeatureLayer.GetMissingFieldsAsync("Stations", stationRequiredFields);
                if (missingFields.Count == 0)
                {
                    results[2].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[2].Status = ValidationStatus.Failed;
                    foreach (string field in missingFields)
                    {
                        results[2].Errors.Add($"Field not found: {field}");
                    }
                }

                //
                // Check for empty/null values in required fields
                //

                // List of fields to check for null values
                List<string> stationsNotNull = new List<string>() { "locationconfidencemeters", "mapunit", "plotatscale", "datasourceid", "stations_id" };

                // Get requied fields with a null value
                List<string> fieldsWithMissingValues = await AnyFeatureLayer.GetRequiredFieldIsNullAsync("Stations", stationsNotNull);
                if (fieldsWithMissingValues.Count == 0)
                {
                    results[3].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[3].Status = ValidationStatus.Failed;
                    foreach (string field in fieldsWithMissingValues)
                    {
                        results[3].Errors.Add($"Null value found in field: {field}");
                    }
                }

                //
                // Check for duplicate Stations_ID values
                //
                List<string> duplicateIds = await AnyFeatureLayer.GetDuplicateValuesInFieldAsync("Stations", "Stations_IDs");
                if (duplicateIds.Count == 0)
                {
                    results[4].Status = ValidationStatus.Passed;
                }
                else
                {
                    results[4].Status = ValidationStatus.Failed;
                    foreach (string id in duplicateIds)
                    {
                        results[4].Errors.Add($"Duplicate Stations_ID value: {id}");
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Get all stations
        /// </summary>
        /// <returns>Returns a list of stations</returns>
        public static async Task<List<Station>> GetStationsAsync()
        {
            List<Station> StationsList = new List<Station>();

            FeatureLayer stationsLayer = MapView.Active?.Map.FindLayers("Stations").FirstOrDefault() as FeatureLayer;

            if (stationsLayer == null)
            {
                return StationsList;
            }

            await QueuedTask.Run(() =>
            {
                using (Table table = stationsLayer.GetTable())
                {
                    if (table != null)
                    {
                        using (RowCursor rowCursor = table.Search())
                        {
                            while (rowCursor.MoveNext())
                            {
                                using (Row row = rowCursor.Current)
                                {
                                    MapPoint Shape = (MapPoint)row["SHAPE"];

                                    Station newStation = new Station
                                    {
                                        ObjectID = Helpers.RowValueToLong(row["ObjectID"]),
                                        FieldID = Helpers.RowValueToString(row["FieldID"]),
                                        TimeDate = row["TimeDate"]?.ToString(),
                                        Observer = Helpers.RowValueToString(row["Observer"]),
                                        LocationMethod = Helpers.RowValueToString(row["LocationMethod"]),
                                        LocationConfidenceMeters = Helpers.RowValueToString(row["LocationConfidenceMeters"]),
                                        PlotAtScale = Helpers.RowValueToString(row["PlotAtScale"]),
                                        Notes = Helpers.RowValueToString(row["Notes"]),
                                        DataSourceId = Helpers.RowValueToString(row["DataSourceId"]),

                                        SpatialReferenceWkid = Shape?.SpatialReference?.Wkid.ToString(),
                                        XCoordinate = Shape?.X.ToString(),
                                        YCoordinate = Shape?.Y.ToString(),
                                    };

                                    StationsList.Add(newStation);
                                }
                            }
                        }
                    }
                }
            });

            return StationsList;
        }

        /// <summary>
        /// Find Stations that don't have a mapunit value. Find intersection with MapUnitPolys and update mapunit value.
        /// </summary>
        /// <returns>Count of rows updated</returns>
        public static async Task<int> UpdateStationsWithMapUnitIntersectionAsync()
        {
            int count = 0;

            FeatureLayer stationsLayer = (FeatureLayer)(MapView.Active?.Map.Layers.FirstOrDefault(a => a.Name == "Stations"));

            FeatureLayer mupLayer = (FeatureLayer)(MapView.Active?.Map.Layers.FirstOrDefault(a => a.Name == "MapUnitPolys"));

            if (stationsLayer == null || mupLayer == null)
            {
                return 0;
            }

            await QueuedTask.Run(() =>
            {
                using (Table stationsTable = stationsLayer.GetTable())
                using (Table mupTable = mupLayer.GetTable())
                {
                    if (stationsTable != null && mupTable != null)
                    {
                        QueryFilter queryFilter = new QueryFilter
                        {
                            SubFields = "shape, mapunit",
                            //WhereClause = "mapunit IS NULL"
                        };

                        EditOperation editOperation = new EditOperation()
                        {
                            Name = "Update MapUnit value for Stations",
                            ProgressMessage = "Intersecting Stations with MapUnitPolys",
                            ShowProgressor = true
                        };

                        editOperation.Callback(context =>
                        {
                            using (RowCursor rowCursor = stationsTable.Search(queryFilter, false))
                            {
                                while (rowCursor.MoveNext())
                                {
                                    using (Row row = rowCursor.Current)
                                    {
                                        // Get the geometry for the station
                                        MapPoint point = (MapPoint)row["SHAPE"];

                                        // Get all of map features that intersect the station
                                        Dictionary<BasicFeatureLayer, List<long>> features = MapView.Active.GetFeatures(point);

                                        // Filter out only MapUnitPolys and then grab the first Object ID in the list
                                        long? mupObjectId = features.Where(x => x.Key.Name == "MapUnitPolys").Select(a => a.Value).FirstOrDefault()?.FirstOrDefault();

                                        if (mupObjectId != null)
                                        {
                                            string mapunit = "";

                                            QueryFilter innerQueryFilter = new QueryFilter
                                            {
                                                SubFields = "mapunit",
                                                ObjectIDs = new List<long> { (long)mupObjectId }
                                            };

                                            using (RowCursor innerRowCursor = mupTable.Search(innerQueryFilter))
                                            {
                                                while (innerRowCursor.MoveNext())
                                                {
                                                    using (Row innerRow = innerRowCursor.Current)
                                                    {
                                                        // Grab the mapunit value
                                                        mapunit = innerRow["mapunit"]?.ToString();
                                                    }
                                                }
                                            }

                                            if (!string.IsNullOrEmpty(mapunit))
                                            {
                                                count++;

                                                // In order to update the Map and/or the attribute table.
                                                // Has to be called before any changes are made to the row.
                                                context.Invalidate(row);

                                                row["mapunit"] = mapunit;

                                                // After all the changes are done, persist it.
                                                row.Store();

                                                // Has to be called after the store too.
                                                context.Invalidate(row);
                                            }
                                        }
                                    }
                                }
                            }
                        }, stationsTable);

                        bool result = editOperation.Execute();

                    }
                }
            });

            return count;
        }

    }
}
