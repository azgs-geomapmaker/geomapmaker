using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geomapmaker.Models
{
    public class Interval
    {
        public string Name { get; set; }

        // I don't think we still need this property, but I carried it over from orignal version
        public string Range => $"{Early_Age} - {Late_Age}ma";
     
        public string Abbrev { get; set; }

        public double Early_Age { get; set; }

        public double Late_Age { get; set; }

        public string Type { get; set; }

        public string Color { get; set; }
    }
}
