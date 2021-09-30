using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace Geomapmaker
{
    internal class MapUnitsViewModel : DockPane
    {
        private const string _dockPaneID = "Geomapmaker_MapUnits";

        protected MapUnitsViewModel() 
        {

        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            pane?.Activate();
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class MapUnits_ShowButton : Button
    {
        protected override void OnClick()
        {
            MapUnitsViewModel.Show();
        }
    }
}
