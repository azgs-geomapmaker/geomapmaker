namespace Geomapmaker.Models
{
    public class GlossaryTerm
    {
        public string DatasetName { get; set; }

        public string FieldName { get; set; }

        public string Term { get; set; }

        public string Definition { get; set; } = "";
    }
}
