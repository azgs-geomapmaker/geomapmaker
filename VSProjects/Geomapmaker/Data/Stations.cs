using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using System.Collections.Generic;
using System.Linq;

namespace Geomapmaker.Data
{
    public class Stations
    {
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
