using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
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
            IEnumerable<OrientationPointsViewModel> orientationPointsVMs = System.Windows.Application.Current.Windows.OfType<OrientationPointsViewModel>(); ;

            // Get the most recent window. GC takes some time to clean up the closed prowindows.
            OrientationPointsViewModel opVM = orientationPointsVMs.LastOrDefault();

            if (opVM == null)
            {
                return false;
            }

            await QueuedTask.Run(() =>
            {
                // Get features that intersect the point
                Dictionary<BasicFeatureLayer, List<long>> selection = MapView.Active.GetFeatures(geometry);

                // Filter anything not stations
                FeatureLayer stationsLayer = selection.Where(f => f.Key.Name == "Stations").FirstOrDefault().Key as FeatureLayer;

                // Select the oids
                List<long> oids = selection[stationsLayer];

                if (oids.Count > 0)
                {
                    opVM.Create.SetStation(oids.First());
                }
            });

            return true;
        }
    }
}
