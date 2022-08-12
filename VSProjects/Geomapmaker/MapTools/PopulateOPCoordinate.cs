using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.ViewModels.OrientationPoints;
using System.Collections.Generic;
using System.Linq;
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

            IEnumerable<OrientationPointsViewModel> opWindowVMs = System.Windows.Application.Current.Windows.OfType<OrientationPointsViewModel>(); ;

            // Get the most recent window. GC takes some time to clean up the closed prowindows.
            OrientationPointsViewModel opViewModel = opWindowVMs.LastOrDefault();

            opViewModel.Create.PopulateCoordinates((MapPoint)geometry);

            // Overriding an async method. Does nothing.
            await QueuedTask.Run(() =>{});

            // Return if the sketch complete event was handled.
            return true;
        }
    }
}
