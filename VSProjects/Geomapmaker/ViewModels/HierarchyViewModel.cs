using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace Geomapmaker.ViewModels
{
    internal class HierarchyViewModel : DockPane
    {
        private const string _dockPaneID = "Geomapmaker_Hierarchy";

        protected HierarchyViewModel() { }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            pane?.Activate();
        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Testing123";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class Hierarchy_ShowButton : Button
    {
        protected override void OnClick()
        {
            HierarchyViewModel.Show();
        }
    }
}
