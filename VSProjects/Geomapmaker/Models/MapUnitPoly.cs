using ArcGIS.Core.Geometry;

namespace Geomapmaker.Models
{
    public class MapUnitPoly
    {
        public long? ID { get; set; }

        public MapUnit MapUnit { get; set; }

        public string IdentityConfidence { get; set; }

        public string Label { get; set; }

        public string Symbol { get; set; }

        public string Notes { get; set; }

        public string DataSource { get; set; }

        public Geometry Shape { get; set; }
    }
}
