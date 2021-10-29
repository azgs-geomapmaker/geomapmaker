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

        //protected override Task OnReadSettingsAsync(ModuleSettingsReader settings)
        //{
        //    //the project is being opened
        //    object value = settings.Get("MyModule.Setting1");

        //    if (value != null)
        //    {
        //        //Do something with the read property
        //    }
        //}

        //protected override Task OnWriteSettingsAsync(ModuleSettingsWriter settings)
        //{
        //    //the project is being saved
        //    settings.Add("MyModule.Setting1", _customValue);
        //}

        protected override bool Initialize()
        {
            HideDockPanes();

            return true;
        }

        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            HideDockPanes();

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

        #endregion Overrides
    }
}
