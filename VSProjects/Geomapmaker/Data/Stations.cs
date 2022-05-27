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
