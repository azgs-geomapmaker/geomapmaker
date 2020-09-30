﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace Geomapmaker {
	internal class MapUnitPolyEditTool : MapTool {
		public MapUnitPolyEditTool() {
			IsSketchTool = true;
			SketchType = SketchGeometryType.Rectangle;
			SketchOutputMode = SketchOutputMode.Map;
		}

		protected override Task OnToolActivateAsync(bool active) {
			AddEditMapUnitPolysDockPaneViewModel.Show();
			return base.OnToolActivateAsync(active);
		}
		protected override Task OnToolDeactivateAsync(bool hasMapViewChanged) {
			AddEditMapUnitPolysDockPaneViewModel.Hide();
			return base.OnToolDeactivateAsync(hasMapViewChanged);
		}

		protected override Task<bool> OnSketchCompleteAsync(Geometry geometry) {
			return base.OnSketchCompleteAsync(geometry);
		}
	}
}
