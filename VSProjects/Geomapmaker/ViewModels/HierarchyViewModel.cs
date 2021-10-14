using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Geomapmaker.ViewModels
{
    internal class HierarchyViewModel : DockPane
    {
        private const string _dockPaneID = "Geomapmaker_Hierarchy";

        protected HierarchyViewModel() { }

        public ObservableCollection<MapUnit> TreeData => new ObservableCollection<MapUnit>(Data.DescriptionOfMapUnits.Headings);

        public ObservableCollection<MapUnit> Orphans => new ObservableCollection<MapUnit>(Data.DescriptionOfMapUnits.MapUnits);

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
    internal class Hierarchy_ShowButton : Button
    {
        protected override void OnClick()
        {
            HierarchyViewModel.Show();
        }
    }
}
