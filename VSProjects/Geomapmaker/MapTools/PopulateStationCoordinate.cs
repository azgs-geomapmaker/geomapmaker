using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.ViewModels.Stations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.MapTools
{
    internal class PopulateStationCoordinate : MapTool
    {
        public PopulateStationCoordinate()
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
            IEnumerable<StationsViewModel> stationWindowVMs = System.Windows.Application.Current.Windows.OfType<StationsViewModel>(); ;

            // Get the most recent window. GC takes some time to clean up the closed prowindows.
            StationsViewModel stationViewModel = stationWindowVMs.LastOrDefault();

            if (stationViewModel?.SelectedTabIndex == 0)
            {
                stationViewModel.Create.PopulateCoordinates((MapPoint)geometry);
            }
            else if (stationViewModel?.SelectedTabIndex == 1)
            {
                stationViewModel.Edit.PopulateCoordinates((MapPoint)geometry);
            }

            // Overriding an async method. Does nothing.
            await QueuedTask.Run(() => { });

            // Return if the sketch complete event was handled.
            return true;
        }
    }
}
