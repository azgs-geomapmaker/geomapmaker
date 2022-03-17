using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using Geomapmaker.ViewModels.OrientationPoints;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.MapTools
{
    internal class SelectStationMapTool : MapTool
    {
        public SelectStationMapTool()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Map;
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            return base.OnToolActivateAsync(active);
        }

        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            FeatureLayer stationsLayer = MapView.Active?.Map.FindLayers("Stations").FirstOrDefault() as FeatureLayer;

            IEnumerable<OrientationPointsViewModel> orientationPointsVMs = System.Windows.Application.Current.Windows.OfType<OrientationPointsViewModel>(); ;

            // Get the most recent window. GC takes some time to clean up the closed prowindows.
            OrientationPointsViewModel opVM = orientationPointsVMs.LastOrDefault();

            if (stationsLayer == null || opVM == null)
            {
                return false;
            }

            await QueuedTask.Run(() =>
            {
                // Get features that intersect the point
                Dictionary<BasicFeatureLayer, List<long>> selection = MapView.Active.GetFeatures(geometry);

                // Select the oids
                List<long> oids = selection[stationsLayer];

                if (oids.Count > 0)
                {
                    long stationOid = oids.First();

                    Table enterpriseTable = stationsLayer.GetTable();

                    QueryFilter queryFilter = new QueryFilter
                    {
                        ObjectIDs = new List<long> { stationOid }
                    }
;
                    using (RowCursor rowCursor = enterpriseTable.Search(queryFilter, false))
                    {
                        if (rowCursor.MoveNext())
                        {
                            using (Row row = rowCursor.Current)
                            {
                                // create a station from fields
                                Station station = new Station
                                {
                                    FieldID = row["fieldid"]?.ToString(),
                                    TimeDate = row["timedate"]?.ToString(),
                                    Observer = row["observer"]?.ToString(),
                                    LocationMethod = row["locationmethod"]?.ToString(),
                                    LocationConfidenceMeters = row["locationconfidencemeters"]?.ToString(),
                                    PlotAtScale = row["plotatscale"]?.ToString(),
                                    Notes = row["notes"]?.ToString(),
                                    DataSourceId = row["datasourceid"]?.ToString(),
                               };

                                // Pass values back to the ViewModel to prepop
                                opVM.Create.SetStation(station);
                            }
                        }
                    }
                }
            });

            return true;
        }
    }
}
