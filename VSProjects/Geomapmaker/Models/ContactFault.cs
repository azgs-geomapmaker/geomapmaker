namespace Geomapmaker.Models
{
    public class ContactFault
    {
        public long? ID { get; set; }

        public string Symbol { get; set; }

        public string Type { get; set; }

        public string IdentityConfidence { get; set; }

        public string ExistenceConfidence { get; set; }

        public string LocationConfidenceMeters { get; set; }

        public bool IsConcealed { get; set; }

        public string Notes { get; set; }

        public string DataSource { get; set; }

        public ArcGIS.Core.Geometry.Geometry Shape { get; set; }
    }
}
