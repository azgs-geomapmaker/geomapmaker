namespace Geomapmaker.Models
{
    public class Interval
    {
        public string Name { get; set; }

        public double Early_Age { get; set; }

        public double Late_Age { get; set; }

        public string Type { get; set; }

        public string Range => $"{Early_Age}-{Late_Age}ma";

        public string Tooltip => $"<b>Name:</b> {Name} <br /> <b>Range:</b> {Early_Age}-{Late_Age}ma <br /> <b>Type:</b> {Type}";
    }
}
