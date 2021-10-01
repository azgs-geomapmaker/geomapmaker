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

        public double Early_Age { get; set; }

        public double Late_Age { get; set; }

        public string Type { get; set; }

        public string Tooltip => $"Range: {Early_Age} - {Late_Age}ma" + Environment.NewLine + $"Type: {Type}";
    }
}
