namespace Geomapmaker.Models
{
    public class Interval
    {
        public string Name { get; set; }

        public double Early_Age { get; set; }

        public double Late_Age { get; set; }

        public string Type { get; set; }

        public string Range => $"{Early_Age}-{Late_Age}ma";

        public string Tooltip => $"Name: {Name} <br /> Range: {Early_Age}-{Late_Age}ma <br /> Type: {Type}";
    }
}
