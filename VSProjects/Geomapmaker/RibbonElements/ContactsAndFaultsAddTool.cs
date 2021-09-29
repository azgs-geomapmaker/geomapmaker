using System;
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
using Geomapmaker.Models;

namespace Geomapmaker {
	internal class ContactsAndFaultsAddTool : MapTool {
		public ContactsAndFaultsAddTool() {
			GeomapmakerModule.ContactsAndFaultsAddTool = this;
			IsSketchTool = true;
			SketchType = SketchGeometryType.Line;
			SketchOutputMode = SketchOutputMode.Map;
			ContextToolbarID = "";
			UseSnapping = true;
		}

		public void Clear() {
			ClearSketchAsync();
		}

		public void SetPopulate() {
			SketchType = SketchGeometryType.Point;
		}

		protected override Task OnToolActivateAsync(bool active) {
			AddEditContactsAndFaultsDockPaneViewModel.Show();
			GeomapmakerModule.ContactsAndFaultsVM.Heading = "Add Contacts and Faults";
			return base.OnToolActivateAsync(active);
		}

		protected override Task OnToolDeactivateAsync(bool hasMapViewChanged) {
			AddEditContactsAndFaultsDockPaneViewModel.Hide();
			GeomapmakerModule.ContactsAndFaultsVM.Reset();
			return base.OnToolDeactivateAsync(hasMapViewChanged);
		}


		protected async override Task<bool> OnSketchCompleteAsync(Geometry geometry) {
			if (geometry == null) {
				return false; //Task.FromResult(false);
			}

			if (SketchType == SketchGeometryType.Point) { //Populate from existing 
				await QueuedTask.Run(() => {
					var mv = MapView.Active;
					// Get the features that intersect the sketch geometry.
					var features = mv.GetFeatures(geometry);
					//Only interested in ContactsAndFaults
					var cfFeatures = features.Where(x => x.Key.Name == "ContactsAndFaults");
					if (cfFeatures.Count() > 0) {
						if (cfFeatures.First().Value.Count() > 0) {
							var cfID = cfFeatures.First().Value.First();
							using (Geodatabase geodatabase = new Geodatabase(DataHelper.connectionProperties)) {
								using (Table cfTable = geodatabase.OpenDataset<Table>("ContactsAndFaults")) {
									QueryFilter queryFilter = new QueryFilter {
										WhereClause = "objectid in (" + cfID + ")"
									};
									using (RowCursor rowCursor = cfTable.Search(queryFilter, false)) {
										if (rowCursor.MoveNext()) {
											using (Row row = rowCursor.Current) {
												//populate a CF from fields
												var cf = new CF();
												cf.symbol = DataHelper.CFSymbols.Where(cfs => cfs.key == row["symbol"].ToString()).First();
												cf.IdentityConfidence = row["identityconfidence"].ToString();
												cf.ExistenceConfidence = row["existenceconfidence"].ToString();
												cf.LocationConfidenceMeters = row["locationconfidencemeters"].ToString();
												cf.IsConcealed = row["isconcealed"].ToString() == "Y";
												cf.Notes = row["notes"] == null ? "" : row["notes"].ToString();
												cf.Shape = GeomapmakerModule.ContactsAndFaultsVM.Shape; //in case user had already drawn a line
												//TODO: Datasource, etc.

												//pass it to a method in the viewmodel called Populate, which will do just that
												GeomapmakerModule.ContactsAndFaultsVM.SelectedCF = cf;
												GeomapmakerModule.ContactsAndFaultsVM.SelectedCFSymbol = cf.symbol; 

												//unset populate mode
												SketchType = SketchGeometryType.Line;
												GeomapmakerModule.ContactsAndFaultsVM.Prepopulate = false;
												SetCurrentSketchAsync(cf.Shape); //redraw existing shape if exists
											}
										}
									}
								}
							}
						}
					}
				});
			} else {  //creating line for new CF

				//GeomapmakerModule.ContactsAndFaultsVM.SelectedCF.Shape = geometry;
				GeomapmakerModule.ContactsAndFaultsVM.Shape = geometry;

				//This is a little janky, but it's the only way I have found to persist the poly and keep it editable
				//until the whole map unit is saved.
				//TODO: Is there a better way?
				//StartSketchAsync(); //Looks like setting current is enough
				await SetCurrentSketchAsync(geometry);
			}

			return false; // Task.FromResult(false);

		}
	}
}
