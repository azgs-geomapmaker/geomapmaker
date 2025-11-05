using ArcGIS.Desktop.Editing.Templates;

namespace Geomapmaker.Models
{
    public class ContactFaultTemplate
    {
        public long? ID { get; set; }

        public string Symbol { get; set; }

        public string Type { get; set; }

        public string Label { get; set; }

        public string IdentityConfidence { get; set; }

        public string ExistenceConfidence { get; set; }

        public string LocationConfidenceMeters { get; set; }

        public bool IsConcealed { get; set; }

        public string Notes { get; set; }

        public string DataSource { get; set; }

        public EditingTemplate Template { get; set; }

        // Add this property to hold the full GemsSymbol object
        public GemsSymbol SymbolObject { get; set; }
    }
}
