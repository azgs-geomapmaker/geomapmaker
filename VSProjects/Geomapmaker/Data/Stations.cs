using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
using Geomapmaker._helpers;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Linq;

namespace Geomapmaker.Data
{
    public class Stations
    {
        public static List<Station> GetStations()
        {
            List<Station> StationsList = new List<Station>();

            FeatureLayer stationsLayer = MapView.Active?.Map.FindLayers("Stations").FirstOrDefault() as FeatureLayer;

            if (stationsLayer == null)
            {
                return StationsList;
            }

            Table enterpriseTable = stationsLayer.GetTable();

            using (RowCursor rowCursor = enterpriseTable.Search())
            {
                while (rowCursor.MoveNext())
                {
                    using (Row row = rowCursor.Current)
                    {
                        MapPoint Shape = (ArcGIS.Core.Geometry.MapPoint)row["SHAPE"];

                        Station newStation = new Station
                        {
                            ObjectID = row["ObjectID"]?.ToString(),
                            FieldID = row["FieldID"]?.ToString(),
                            TimeDate = row["TimeDate"]?.ToString(),
                            Observer = row["Observer"]?.ToString(),
                            LocationMethod = row["LocationMethod"]?.ToString(),
                            LocationConfidenceMeters = row["LocationConfidenceMeters"]?.ToString(),
                            PlotAtScale = row["PlotAtScale"]?.ToString(),
                            Notes = row["Notes"]?.ToString(),
                            DataSourceId = row["DataSourceId"]?.ToString(),
                            SpatialReferenceWkid = Shape?.SpatialReference?.Wkid.ToString(),
                            XCoordinate = Shape?.X.ToString(),
                            YCoordinate = Shape?.Y.ToString(),
                        };

                        StationsList.Add(newStation);
                    }
                }
            }

            return StationsList;
        }

        public static List<string> GetStationFieldIds()
        {
            List<string> FieldIds = new List<string>();

            FeatureLayer stationsLayer = MapView.Active?.Map.FindLayers("Stations").FirstOrDefault() as FeatureLayer;

            if (stationsLayer == null)
            {
                return FieldIds;
            }

            Table enterpriseTable = stationsLayer.GetTable();

            using (RowCursor rowCursor = enterpriseTable.Search())
            {
                while (rowCursor.MoveNext())
                {
                    using (Row row = rowCursor.Current)
                    {
                        string rowFieldId = row["FieldID"]?.ToString();

                        // Add it to temp list
                        FieldIds.Add(rowFieldId);
                    }
                }
            }

            return FieldIds;
        }
    }
}
