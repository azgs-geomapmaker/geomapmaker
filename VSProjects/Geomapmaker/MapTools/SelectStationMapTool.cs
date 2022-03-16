using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Mapping;
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

        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            return base.OnSketchCompleteAsync(geometry);
        }
    }
}
