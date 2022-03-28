using ArcGIS.Core.Geometry;

namespace Geomapmaker.Models
{
    public class Station
    {
        public string ObjectID { get; set; }

        // Station number assigned by person who originally located this station. Commonly, key to field sheet and (or) field notebook
        public string FieldID { get; set; }

        public string TimeDate { get; set; }

        public string Observer { get; set; }

        public string LocationMethod { get; set; }

        public string LocationConfidenceMeters { get; set; }

        public string PlotAtScale { get; set; }

        // Optional field
        public string Notes { get; set; }

        public string DataSourceId { get; set; }

        public string SpatialReferenceWkid { get; set; }

        public string XCoordinate { get; set; }

        public string YCoordinate { get; set; }
    }
}
