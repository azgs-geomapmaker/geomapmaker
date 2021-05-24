using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geomapmaker.Models {
	public class DataSource {
		public Int64? ID { get; set; }
		public string DataSource_ID { get; set; }
		public string Source { get; set; }
		public string Url { get; set; }
		public string Notes { get; set; }
	}
}
