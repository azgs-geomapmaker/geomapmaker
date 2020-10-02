using ArcGIS.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geomapmaker.Models {
	class MapUnitPoly {
		public int ID { get; set; }
		public MapUnit MapUnit { get; set; }
		public string IdentityConfidence { get; set; }
		public string Label { get; set; }
		public string Symbol { get; set; }
		public string Notes { get; set; }
		public string DataSource { get; set; }
		public Geometry Shape { get; set; }

	}
}
