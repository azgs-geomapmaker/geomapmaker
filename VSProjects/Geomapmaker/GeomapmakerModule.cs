using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace Geomapmaker
{
    internal class GeomapmakerModule : Module
    {
        private static GeomapmakerModule _this = null;

        public static string DataSourceId { get; set; }

        public const string CF_SketchTemplateName = "Sketch";

        public const string MUP_UnassignedTemplateName = "Unassigned";

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static GeomapmakerModule Current => _this ?? (_this = (GeomapmakerModule)FrameworkApplication.FindModule("Geomapmaker_Module"));
    }
}
