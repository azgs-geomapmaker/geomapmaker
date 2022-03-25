﻿using ArcGIS.Core.Data;
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
                        Station newStation = new Station
                        {
                            FieldID = Helpers.RowValueToString(row["FieldID"]),
                            TimeDate = Helpers.RowValueToString(row["TimeDate"]),
                            Observer = Helpers.RowValueToString(row["Observer"]),
                            LocationMethod = Helpers.RowValueToString(row["LocationMethod"]),
                            LocationConfidenceMeters = Helpers.RowValueToString(row["LocationConfidenceMeters"]),
                            PlotAtScale = Helpers.RowValueToString(row["PlotAtScale"]),
                            Notes = Helpers.RowValueToString(row["Notes"]),
                            DataSourceId = Helpers.RowValueToString(row["DataSourceId"])
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
