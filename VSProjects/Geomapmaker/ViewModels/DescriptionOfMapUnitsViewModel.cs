using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace Geomapmaker
{
    internal class DescriptionOfMapUnitsViewModel : DockPane
    {
        private const string _dockPaneID = "Geomapmaker_DescriptionOfMapUnits";

        protected DescriptionOfMapUnitsViewModel() { }

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
    internal class DescriptionOfMapUnits_ShowButton : Button
    {
        protected override void OnClick()
        {
            DescriptionOfMapUnitsViewModel.Show();
        }
    }
}
