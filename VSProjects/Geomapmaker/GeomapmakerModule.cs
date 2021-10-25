using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.RibbonElements;

namespace Geomapmaker
{
    internal class GeomapmakerModule : Module
    {
        private static GeomapmakerModule _this = null;

        //public DataHelper helper = new DataHelper();
        internal static AddEditContactsAndFaultsDockPaneViewModel ContactsAndFaultsVM { get; set; }
        internal static ContactsAndFaultsAddTool ContactsAndFaultsAddTool { get; set; }
        internal static ContactsAndFaultsEditTool ContactsAndFaultsEditTool { get; set; }

        internal static AddEditMapUnitPolysDockPaneViewModel MapUnitPolysVM { get; set; }
        internal static MapUnitPolyAddTool AddMapUnitPolyTool { get; set; }
        internal static MapUnitPolyEditTool EditMapUnitPolyTool { get; set; }

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static GeomapmakerModule Current => _this ?? (_this = (GeomapmakerModule)FrameworkApplication.FindModule("Geomapmaker_Module"));

        #region Overrides

        protected override bool Initialize()
        {
            HideDockPanes();
            RemoveLayersTables();

            return true;
        }

        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            HideDockPanes();
            RemoveLayersTables();

            return true;
        }

        // Hide geomapmaker dockpanes when loading/unloading
        private void HideDockPanes()
        {
            // When the app starts up, there will be no user logged in. Clean up dockpanes to reflect this
            List<string> DockPaneIds = new List<string>
            {
                "Geomapmaker_Headings",
                "Geomapmaker_DescriptionOfMapUnits",
                "Geomapmaker_Hierarchy"
            };

            foreach (string dockId in DockPaneIds)
            {
                DockPane pane = FrameworkApplication.DockPaneManager.Find(dockId);
                pane?.Hide();
            }
        }

        // Remove geomapmaker layers and tables when loading/unloading
        private void RemoveLayersTables()
        {
            ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                var map = MapView.Active?.Map;

                if (map != null)
                {
                    // Remove all layers except basemaps
                    map.RemoveLayers(map.Layers.Where(a => a.MapLayerType != MapLayerType.BasemapBackground && a.MapLayerType != MapLayerType.BasemapTopReference));
                    // Remove all standalone tables
                    map.RemoveStandaloneTables(map.StandaloneTables);
                }

            });
        }

        #endregion Overrides
    }
}
