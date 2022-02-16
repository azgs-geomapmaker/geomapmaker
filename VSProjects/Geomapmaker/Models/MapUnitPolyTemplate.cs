using ArcGIS.Desktop.Editing.Templates;

namespace Geomapmaker.Models
{
    public class MapUnitPolyTemplate
    {
        public string MapUnit { get; set; }

        public string DataSourceID { get; set; }

        public string HexColor { get; set; }

        public string Tooltip { get; set; }

        public EditingTemplate Template { get; set; }
    }
}
