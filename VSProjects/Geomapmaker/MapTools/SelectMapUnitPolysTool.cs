using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.ViewModels.MapUnitPolysEdit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.MapTools
{
    internal class SelectMapUnitPolysTool : MapTool
    {
        public SelectMapUnitPolysTool()
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
            FeatureLayer layer = MapView.Active?.Map?.GetLayersAsFlattenedList()?.OfType<FeatureLayer>()?.FirstOrDefault(l => l.Name == "MapUnitPolys");

            IEnumerable<MapUnitPolysEditVM> viewModels = System.Windows.Application.Current.Windows.OfType<MapUnitPolysEditVM>(); ;

            // Get the most recent window. GC takes some time to clean up the closed prowindows.
            MapUnitPolysEditVM mapUnitPolysEditVM = viewModels.LastOrDefault();

            if (mapUnitPolysEditVM == null)
            {
                return false;
            }

            await QueuedTask.Run(() =>
            {
                // Get features that intersect the point

                Dictionary<BasicFeatureLayer, List<long>> selection = MapView.Active.GetFeatures(geometry);

                // Filter anything not CF
                FeatureLayer mupLayer = selection.Where(f => f.Key.Name == "MapUnitPolys").FirstOrDefault().Key as FeatureLayer;

                // Select the oids
                List<long> oidsMUPs = selection[mupLayer];

                if (oidsMUPs.Count > 0)
                {
                    mapUnitPolysEditVM.Set_MUP_Oids(oidsMUPs);
                }
            });

            return true;
        }
    }
}

