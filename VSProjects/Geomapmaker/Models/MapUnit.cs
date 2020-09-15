using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geomapmaker.Models {
	public class MapUnit {
		//TODO: Some of these will need more fancy processing. Here or in view model?
		public int ID { get; set; }

		//public string MU;
		//private string mu;
		public string MU { get; set; }

		//private string name;
		public string Name { get; set; }

		//public string fullName;
		public string FullName { get; set; }

		public string Age { get; set; }

		public string RelativeAge { get; set; }

		//private string description;
		public string Description { get; set; }

		public string HierarchyKey { get; set; }

		public List<string> ParagraphStyle { get; set; }

		public string Label { get; set; }

		public string Symbol { get; set; }

		public string AreaFillRGB { get; set; }

		public string hexcolor { get; set; }

		public string DescriptionSourceID { get; set; }

		public string GeoMaterial { get; set; }

		public string GeoMaterialConfidence { get; set; }

		public MapUnit() {
		}
	}
}
