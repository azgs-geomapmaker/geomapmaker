using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
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
        /// Check if the Stations layer exists in Active Map
        /// </summary>
        /// <returns>Returns true if layer exists</returns>
        public static async Task<bool> FeatureLayerExistsAsync()
        {
            return await General.FeatureLayerExistsAsync("Stations");
        }

        /// <summary>
        /// Check the layer for any missing fieldss
        /// </summary>
        /// <returns>Returns a list of fieldnames missing from the layer</returns>
        public static async Task<List<string>> GetMissingFieldsAsync()
        {
            // List of fields to check for
            List<string> requiredFields = new List<string>() { "fieldid", "locationconfidencemeters", "observedmapunit", "mapunit", "symbol", "label", "plotatscale",
                "datasourceid", "notes", "locationmethod", "timedate", "observer", "significantdimensionmeters", "gpsx", "gpsy", "pdop", "mapx", "mapy", "stations_id" };

            return await General.FeatureLayerGetMissingFieldsAsync("Stations", requiredFields);
        }

        /// <summary>
        /// Get a list of unique, non-null values for the field DataSourceId in the Stations layer
        /// </summary>
        /// <returns>List of DataSourceID values</returns>
        public static async Task<List<string>> GetDistinctDataSourceIdValuesAsync()
        {
            return await General.FeatureLayerGetDistinctValuesForFieldAsync("Stations", "datasourceid");
        }

        /// <summary>
        /// Get a list of unique, non-null values for the field FieldId in the Stations layer
        /// </summary>
        /// <returns>List of FieldId values</returns>
        public static async Task<List<string>> GetStationFieldIdsAsync()
        {
            return await General.FeatureLayerGetDistinctValuesForFieldAsync("Stations", "fieldid");
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

    }
}
