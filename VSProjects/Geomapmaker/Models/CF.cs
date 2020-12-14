using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ArcGIS.Core.Geometry;

namespace Geomapmaker.Models {
	public class CF {
		public Int64? ID { get; set; }

		/*
		public string key { get; set; }
		public string description { get; set; }
		public string symbol { get; set; }
		public ImageSource preview { get; set; }
		*/
		public CFSymbol symbol { get; set; }
		public string IdentityConfidence { get; set; }
		public string ExistenceConfidence { get; set; }
		public string LocationConfidenceMeters { get; set; }
		public string IsConcealed { get; set; }
		public string Notes { get; set; }
		public string DataSource { get; set; }
		public ArcGIS.Core.Geometry.Geometry Shape { get; set; }

	}
}
