using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using Geomapmaker.Models;
using System.Collections.Generic;

namespace Geomapmaker
{
    internal class GeomapmakerModule : Module
    {
        private static GeomapmakerModule _this = null;

        public static string DataSourceId { get; set; }

        public static List<GemsSymbol> ContactsAndFaultsSymbols { get; set; }

        public static List<GemsSymbol> OrientationPointSymbols { get; set; }

        public const string CF_SketchTemplateName = "Sketch";

        public const string MUP_UnassignedTemplateName = "Unassigned";

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static GeomapmakerModule Current => _this ?? (_this = (GeomapmakerModule)FrameworkApplication.FindModule("Geomapmaker_Module"));
    }
}
