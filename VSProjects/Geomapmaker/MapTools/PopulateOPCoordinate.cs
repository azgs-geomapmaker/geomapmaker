using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System.Threading.Tasks;

namespace Geomapmaker.MapTools
{
    internal class PopulateOPCoordinate : MapTool
    {
        public PopulateOPCoordinate()
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
            await QueuedTask.Run(() =>
            {
   
            });

            // Return if the sketch complete event was handled.
            return true;
        }
    }
}
