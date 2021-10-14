using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.RibbonElements;

namespace Geomapmaker
{
    internal /*public*/ class GeomapmakerModule : Module
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
        public static GeomapmakerModule Current
        {
            get
            {
                HideDockPanes();
                return _this ?? (_this = (GeomapmakerModule)FrameworkApplication.FindModule("Geomapmaker_Module"));
            }
        }

        private static void HideDockPanes()
        {
            // When the app starts up, there will be no user logged in. Clean up dockpanes to reflect this
            List<string> DockPaneIds = new List<string>
            {
                // Old dockpane id => can eventually remove this one
                "Geomapmaker_AddEditMapUnitsDockPane",

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

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //TODO - add your business logic
            //return false to ~cancel~ Application close

            HideDockPanes();

            return true;
        }

        #endregion Overrides
    }
}
