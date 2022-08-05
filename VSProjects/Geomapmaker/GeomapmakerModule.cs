using ArcGIS.Desktop.Core.Events;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using Geomapmaker.Models;
using Geomapmaker.RibbonElements;
using System.Collections.Generic;

namespace Geomapmaker
{
    internal class GeomapmakerModule : Module
    {
        private static GeomapmakerModule _this = null;

        // User-selected data source (Ribbon Combobox)
        public static string DataSourceId { get; set; }

        public static DataSourceComboBox DataSourceComboBox { get; set; }

        // Line symbol options
        public static List<GemsSymbol> ContactsAndFaultsSymbols { get; set; }

        // Point symbol options
        public static List<GemsSymbol> OrientationPointSymbols { get; set; }

        // Name of the CF Sketch Template
        public const string CF_SketchTemplateName = "Sketch";

        // Name of the MUP Unassigned Template
        public const string MUP_UnassignedTemplateName = "Unassigned";

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static GeomapmakerModule Current => _this ?? (_this = (GeomapmakerModule)FrameworkApplication.FindModule("Geomapmaker_Module"));

        protected override bool Initialize()
        {
            // Subscribe to project closing
            ProjectClosedEvent.Subscribe(OnProjectClosed);

            return true;
        }

        private void OnProjectClosed(ProjectEventArgs args)
        {
            // Clear the Datasource combo
            DataSourceComboBox?.ClearSelection();
        }
    }
}
