using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geomapmaker.Models {
	public class Interval {
		public Interval() { }
		public Interval(string name, string range) {
			this.name = name;
			this.range = range;
		}

		public string name { get; set; }
		public string range { get; set; }
	}
}
