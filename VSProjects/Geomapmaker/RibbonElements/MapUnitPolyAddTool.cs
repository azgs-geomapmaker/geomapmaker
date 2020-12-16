﻿using System;
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
			//UseSnapping = true;
			// Select the type of construction tool you wish to implement.  
			// Make sure that the tool is correctly registered with the correct component category type in the daml 
			SketchType = SketchGeometryType.Point;
			// SketchType = SketchGeometryType.Line;
			//SketchType = SketchGeometryType.Polygon;
			SketchOutputMode = SketchOutputMode.Map;
			//Gets or sets whether the sketch is for creating a feature and should use the CurrentTemplate.
			//UsesCurrentTemplate = true;
			//ContextToolbarID = "";
			UseSelection = false;
		}

		public void Clear() {
			ClearSketchAsync();
			UseSelection = false;
			SketchType = SketchGeometryType.Point;
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
		protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry) {
			var mv = MapView.Active;
			var identifyResult = await QueuedTask.Run(() => {

				//mv.SelectFeatures(geometry);

				if (UseSelection == false) {
					//User just selected a cf to convert. Copy geometry to view model.

					// Get the features that intersect the sketch geometry.
					var features = mv.GetFeatures(geometry);
					//Only interested in MapUnitPolys
					var cfFeatures = features.Where(x => x.Key.Name == "ContactsAndFaults");
					if (cfFeatures.Count() > 0) {
						//Get cf objectid
						//TODO: I am only pulling the first from the list. Might need to present some sort of selector to the user. 
						if (cfFeatures.First().Value.Count() > 0) {
							var cfID = cfFeatures.First().Value.First();

							//Using the objectid, get the cf record from the database
							using (Geodatabase geodatabase = new Geodatabase(DataHelper.connectionProperties)) {
								using (Table cfTable = geodatabase.OpenDataset<Table>("ContactsAndFaults")) {
									QueryFilter queryFilter = new QueryFilter {
										WhereClause = "objectid = " + cfID
									};
									using (RowCursor rowCursor = cfTable.Search(queryFilter, false)) {
										if (rowCursor.MoveNext()) {
											using (Row row = rowCursor.Current) {
												GeomapmakerModule.MapUnitPolysVM.Shape = PolygonBuilder.CreatePolygon((Polyline)row["shape"]);
											}
										}

									}

									UseSelection = true;
									SketchType = SketchGeometryType.Polygon; 
									ContextToolbarID = "";
									SetCurrentSketchAsync(GeomapmakerModule.MapUnitPolysVM.Shape);

								}
							}
						}
					}
					return true;
				} else {
					//User has just modified the geometery, stick it on the form
					GeomapmakerModule.MapUnitPolysVM.Shape = geometry;
					SetCurrentSketchAsync(geometry);
					//UseSelection = false;
					//SketchType = SketchGeometryType.Point;
					//TODO: more stuff?
					return true;
				}
			});
			//MessageBox.Show(identifyResult);
			return true;
		}
	}
}
