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

		private SketchGeometryType selectionSketchType = SketchGeometryType.Point;
		//private SketchGeometryType modifySketchType = SketchGeometryType.Polygon;

		public MapUnitPolyAddTool() {
			GeomapmakerModule.AddMapUnitPolyTool = this;
			IsSketchTool = true;
			//UseSnapping = true;
			// Select the type of construction tool you wish to implement.  
			// Make sure that the tool is correctly registered with the correct component category type in the daml 
			SketchType = selectionSketchType;
			// SketchType = SketchGeometryType.Line;
			//SketchType = SketchGeometryType.Polygon;
			SketchOutputMode = SketchOutputMode.Map;
			//Gets or sets whether the sketch is for creating a feature and should use the CurrentTemplate.
			//UsesCurrentTemplate = true;
			//ContextToolbarID = "";
			UseSelection = false;
			//UseSelection = true;
			//UseSnapping = true;
			//CompleteSketchOnMouseUp = true;
		}

		private List<long> cfIDs = new List<long>();

		public void Clear() {
			overlay.Dispose();
			ClearSketchAsync();
			cfIDs.Clear();
			UseSelection = false;
			//SketchType = SketchGeometryType.Point;
			SketchType = selectionSketchType;
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

		private IDisposable overlay;

		/// <summary>
		/// Called when the sketch finishes. This is where we will create the sketch operation and then execute it.
		/// </summary>
		/// <param name="geometry">The geometry created by the sketch.</param>
		/// <returns>A Task returning a Boolean indicating if the sketch complete event was successfully handled.</returns>
		protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry) {
			var mv = MapView.Active;
			var identifyResult = await QueuedTask.Run(() => {
	
				/*
				 * This is unused, but I'm keeping it for now to remind me how to do it.
				var lineStroke = SymbolFactory.Instance.ConstructStroke(ColorFactory.Instance.RedRGB, 4.0);
				var marker = SymbolFactory.Instance.ConstructMarker(ColorFactory.Instance.RedRGB, 12, SimpleMarkerStyle.Circle);
				marker.MarkerPlacement = new CIMMarkerPlacementOnVertices() {
					AngleToLine = true,
					PlaceOnEndPoints = true,
					Offset = 0
				};
				var lineSymbol = new CIMLineSymbol() {
					SymbolLayers = new CIMSymbolLayer[2] { marker, lineStroke }
				};
				*/

				//set up poly symbol for use in previewing poly that will result from CF selection
				var fill = SymbolFactory.Instance.ConstructSolidFill(ColorFactory.Instance.CreateRGBColor(55,55,55,20));
				var polySymbol = new CIMPolygonSymbol() {
					SymbolLayers = new CIMSymbolLayer[1] { fill }
				};

				//TODO: The intent here was to allow the user to tweak the poly after selecting CF's. But I'm no longer doing that,
				//so this control flow logic can probably go away.
				if (SketchType == selectionSketchType) {

					// Get the features that intersect the sketch geometry.
					var features = mv.GetFeatures(geometry);
					//Only interested in ContactsAndFaults
					var cfFeatures = features.Where(x => x.Key.Name == "ContactsAndFaults");
					if (cfFeatures.Count() > 0) {
						//Get cf objectid
						//TODO: I am only pulling the first from the list. Might need to present some sort of selector to the user. 
						if (cfFeatures.First().Value.Count() > 0) {
							var cfID = cfFeatures.First().Value.First();
							if (!cfIDs.Contains(cfID)) {
								cfIDs.Add(cfID);
							}

							//Build string for use in query where clause
							String cfIDstring = "";
							cfIDs.ForEach(cf => {
								cfIDstring += cfIDstring == "" ? "" + cf : ", " + cf;
							});

							//Using the objectids, get the cf records from the database, and build a poly out of them
							using (Geodatabase geodatabase = new Geodatabase(DataHelper.connectionProperties)) {
								using (Table cfTable = geodatabase.OpenDataset<Table>("ContactsAndFaults")) {
									QueryFilter queryFilter = new QueryFilter {
										WhereClause = "objectid in (" + cfIDstring + ")"
									};
									using (RowCursor rowCursor = cfTable.Search(queryFilter, false)) {
										PolylineBuilder polylineBuilder = new PolylineBuilder(); //TODO: spatial ref?
										while (rowCursor.MoveNext()) {
											using (var lineFeature = rowCursor.Current as Feature) {
												// add the coordinate collection of the current geometry into our overall list of collections
												var polylineGeometry = lineFeature.GetShape() as Polyline;
												polylineBuilder.AddParts(polylineGeometry.Parts);
											}
										}
										Polyline polyline = GeometryEngine.Instance.SimplifyPolyline(polylineBuilder.ToGeometry(), SimplifyType.Network, true);
										GeomapmakerModule.MapUnitPolysVM.Shape = GeometryEngine.Instance.SimplifyAsFeature(PolygonBuilder.CreatePolygon(polyline), true);

									}

									//SketchType = modifySketchType;
									//SetCurrentSketchAsync(GeomapmakerModule.MapUnitPolysVM.Shape);
									if (overlay != null) overlay.Dispose();
									overlay = this.AddOverlay(GeomapmakerModule.MapUnitPolysVM.Shape, polySymbol.MakeSymbolReference());
								}
							}
						}
					}
					return true;
				} else { //TODO: the rest of the control logic that can probably go away (we never reach this)
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
