using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
	internal class MapUnitPolyAddTool : MapTool {
		public MapUnitPolyAddTool() {
			GeomapmakerModule.AddMapUnitPolyTool = this;
			IsSketchTool = true;
			UseSnapping = true;
			// Select the type of construction tool you wish to implement.  
			// Make sure that the tool is correctly registered with the correct component category type in the daml 
			//SketchType = SketchGeometryType.Point;
			// SketchType = SketchGeometryType.Line;
			SketchType = SketchGeometryType.Polygon;
			//SketchOutputMode = SketchOutputMode.Screen;
			//Gets or sets whether the sketch is for creating a feature and should use the CurrentTemplate.
			//UsesCurrentTemplate = true;
			ContextToolbarID = "";
		}

		public void Clear() {
			ClearSketchAsync();
		}

		protected override Task OnToolActivateAsync(bool active) {
			AddEditMapUnitPolysDockPaneViewModel.Show();
			GeomapmakerModule.MapUnitPolysVM.Heading = "Add Map Unit Poly";
			return base.OnToolActivateAsync(active);
		}

		protected override Task OnToolDeactivateAsync(bool hasMapViewChanged) {
			AddEditMapUnitPolysDockPaneViewModel.Hide();
			GeomapmakerModule.MapUnitPolysVM.Reset();
			return base.OnToolDeactivateAsync(hasMapViewChanged);
		}

		/// <summary>
		/// Called when the sketch finishes. This is where we will create the sketch operation and then execute it.
		/// </summary>
		/// <param name="geometry">The geometry created by the sketch.</param>
		/// <returns>A Task returning a Boolean indicating if the sketch complete event was successfully handled.</returns>
		protected override Task<bool> OnSketchCompleteAsync(Geometry geometry) {
			if (geometry == null) {
				return Task.FromResult(false);
			}

			//TODO: This sets the geom. Need to implement save. Then refresh map
			//GeomapmakerModule.MapUnitPolysVM.SelectedMapUnitPoly.Shape = geometry;
			GeomapmakerModule.MapUnitPolysVM.Shape = geometry;

			//This is a little janky, but it's the only way I have found to persist the poly and keep it editable
			//until the whole map unit is saved.
			//TODO: Is there a better way?
			//StartSketchAsync(); //Looks like setting current is enough
			SetCurrentSketchAsync(geometry);

			return Task.FromResult(false);

			/*
			if (CurrentTemplate == null || geometry == null)
				return Task.FromResult(false);

			// Create an edit operation
			var createOperation = new EditOperation();
			createOperation.Name = string.Format("Create {0}", CurrentTemplate.Layer.Name);
			createOperation.SelectNewFeatures = true;

			// Queue feature creation
			createOperation.Create(CurrentTemplate, geometry);

			// Execute the operation
			return createOperation.ExecuteAsync();
			*/
		}
	}
}
