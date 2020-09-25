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

		//TODO: This age logic, and the older/younger properties that drive it, should probably be moved into the 
		//view model. Addendum: I tried this and got it working. But it felt messy to me. I think Imma leave it here afterall.
		public string Age { 
			get {
				var older = OlderInterval == null ? "" : OlderInterval;
				var younger = YoungerInterval == null ? "" : YoungerInterval;
				return older != "" || younger != "" ? older + "-" + younger : null;
			} 
			set {
				if (value != null) {
					var intervals = value.Split('-');
					OlderInterval = intervals.Length > 0 ? intervals[0] : null;
					YoungerInterval = intervals.Length > 1 ? intervals[1] : null;
				}
			}
		}

		public string OlderInterval { get; set; }

		public string YoungerInterval { get; set; }

		public string RelativeAge { get; set; }

		//private string description;
		public string Description { get; set; }

		public string HierarchyKey { get; set; }

		public List<string> ParagraphStyle { get; set; }

		public string Label { get; set; }

		public string Symbol { get; set; }

		public string AreaFillRGB {
			//hexcolor is the primary color holder, and is bound to the view. AreaFilleRGB just reformats that into xxx;xxx;xxx.
			get {
				if (hexcolor != null) {
					int r = Convert.ToInt32(hexcolor.Substring(1, 2), 16);
					int g = Convert.ToInt32(hexcolor.Substring(3, 2), 16);
					int b = Convert.ToInt32(hexcolor.Substring(5, 2), 16);
					return r + ";" + g + ";" + b;
				} else {
					return null;
				}
			}
		}

		private string _hexcolor;
		public string hexcolor { 
			//Depending on whether hexcolor is assigned from the database or from the view, it may have an alpha channel.
			//We don't want that. So, if it exists, we get rid of it in the get.
			get {
				return _hexcolor == null ? null :
					_hexcolor.Length == 9 ? "#" + _hexcolor.Substring(3, 6) :
											"#" + _hexcolor.Substring(1, 6);
			}
			set {
				_hexcolor = value;
			}
		}
		
		public string DescriptionSourceID { get; set; }

		public string GeoMaterial { get; set; }

		public string GeoMaterialConfidence { get; set; }

		public MapUnit() {
		}
	}
}
