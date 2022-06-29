namespace Geomapmaker.Models
{
    public class GlossaryTerm
    {
        public long ObjectId { get; set; }

        public string DatasetName { get; set; }

        public string FieldName { get; set; }

        public string Term { get; set; }

        public string Definition { get; set; } = "";

        public string DefinitionSourceID { get; set; }
    }
}
