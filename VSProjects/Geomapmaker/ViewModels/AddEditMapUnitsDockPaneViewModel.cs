using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ArcGIS.Core;
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
	internal class AddEditMapUnitsDockPaneViewModel : DockPane {
		private const string _dockPaneID = "Geomapmaker_AddEditMapUnitsDockPane";

		protected  AddEditMapUnitsDockPaneViewModel() { 
			Debug.WriteLine("AddEditMapUnitsDockPaneViewModel constructor enter");
		}


		/// <summary>
		/// Show the DockPane.
		/// </summary>
		internal static void Show() {
			DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
			if (pane == null)
				return;
			Debug.WriteLine("VM.Show, mapUnitName = " + DataHelper.MapUnitName);
			pane.Activate(); 
		}

		internal static void Hide() {
			DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
			if (pane == null)
				return;
			pane.Hide();
		}

		public Boolean IsValid {
		//TODO: This is not raising property changed event, so the button never enables. Hard to believe I'll have to raise an event from 
		//each of the properties used below. There must be a better way.
			get {
				//return true;
				return
					SelectedMapUnit != null &&
					SelectedMapUnit.MU != null && SelectedMapUnit.MU.Trim() != "" &&
					SelectedMapUnit.Name != null && SelectedMapUnit.Name.Trim() != "";// &&

					//SelectedMapUnit.Age != null && SelectedMapUnit.Age.Trim() != "";//&&
					//SelectedMapUnit.HierarchyKey != null && SelectedMapUnit.HierarchyKey.Trim() != "" &&
					//SelectedMapUnit.ParagraphStyle != null && SelectedMapUnit.ParagraphStyle.Trim() != "" &&
					//SelectedMapUnit.Symbol != null && SelectedMapUnit.Symbol.Trim() != "" &&
					//SelectedMapUnit.DescriptionSourceID != null && SelectedMapUnit.DescriptionSourceID.Trim() != "" &&
				
			}
		}


		/// <summary>
		/// Text shown near the top of the DockPane.
		/// </summary>
		private const string GENERIC_HEADING = "Add/Edit Map Units";
		private const string ADD_HEADING = "Add Map Unit";
		public const string EDIT_HEADING = "Edit Map Unit";
		private string _heading = GENERIC_HEADING;
		public string Heading {
			get { return _heading; }
			set {
				SetProperty(ref _heading, value, () => Heading);
			}
		}
		private string subHeading = GENERIC_HEADING;
		public string SubHeading {
			get { return subHeading; }
			set {
				SetProperty(ref subHeading, value, () => SubHeading);
			}
		}

		private MapUnit selectedMapUnit;
		public MapUnit SelectedMapUnit {
			get => selectedMapUnit;
			set {
				if (value == null) {
					SetProperty(ref selectedMapUnit, value, () => SelectedMapUnit);
					SubHeading = GENERIC_HEADING;
					return;
				}
				//selectedMapUnit = value;
				SetProperty(ref selectedMapUnit, value, () => SelectedMapUnit); //Have to do this to trigger stuff, I guess.

				//Set heading and populate form, depending on whether this is a new map unit or not 
				//if (!DataHelper.MapUnits.Any(p => p.Text == value)) {
				if (!DataHelper.MapUnits.Any(mu => mu.MU == value.MU)) {
					SubHeading = ADD_HEADING;
					//populate form with defaults
				} else {
					SubHeading = EDIT_HEADING;
					// populate form from db

				}
			}
		}

		public string SelectedOlderInterval { get; set; }
		public string SelectedYoungerInterval { get; set; }

		//This is to capture the user entering a new map unit (not in the list)
		private string userEnteredMapUnit;
		public string UserEnteredMapUnit {
			get {
				return userEnteredMapUnit;
			}
			set {
				//if (userEnteredMapUnit != value) {
				userEnteredMapUnit = value;
				if (value == null) {
					SelectedMapUnit = null;
				} else if (!DataHelper.MapUnits.Any(mu => mu.MU == value)) {
					//userEnteredMapUnit = value;
					var mapUnit = new MapUnit();
					mapUnit.MU = userEnteredMapUnit;
					SelectedMapUnit = mapUnit;
				}
			}
		}


		public  async Task saveMapUnit(MapUnit mapUnit) {
			Debug.WriteLine("saveMapUnit enter");

			//var mapUnits = new ObservableCollection<ComboBoxItem>();
			var mapUnits = new ObservableCollection<MapUnit>();

			if (DataHelper.connectionProperties == null) {
				return;
			}

			await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() => {
				string message = String.Empty;
				bool result = false;
				EditOperation editOperation = new EditOperation();

				using (Geodatabase geodatabase = new Geodatabase(DataHelper.connectionProperties)) {
					using (Table enterpriseTable = geodatabase.OpenDataset<Table>("DescriptionOfMapUnits")) {

						if (DataHelper.MapUnits.Any(mu => mu.MU == mapUnit.MU)) {
							Debug.WriteLine("editting");

							editOperation.Callback(context => {
								QueryFilter filter = new QueryFilter { WhereClause = "objectid = " + mapUnit.ID };

								using (RowCursor rowCursor = enterpriseTable.Search(filter, false)) {
									TableDefinition tableDefinition = enterpriseTable.GetDefinition();
									int subtypeFieldIndex = tableDefinition.FindField(tableDefinition.GetSubtypeField());

									while (rowCursor.MoveNext()) { //TODO: Anything? Should be only one
										using (Row row = rowCursor.Current) {
											// In order to update the Map and/or the attribute table.
											// Has to be called before any changes are made to the row.
											context.Invalidate(row);

											row["MapUnit"] = mapUnit.MU;
											row["Name"] = mapUnit.Name;
											row["FullName"] = mapUnit.FullName;
											row["Age"] = mapUnit.Age;
											//row["RelativeAge"] = mapUnit.RelativeAge;
											row["Description"] = mapUnit.Description;
											row["HierarchyKey"] = 5; // mapUnit.HierarchyKey;
											row["ParagraphStyle"] = 5; // mapUnit.ParagraphStyle;
											row["Label"] = mapUnit.Label;
											row["Symbol"] = mapUnit.Symbol;
											row["AreaFillRGB"] = mapUnit.AreaFillRGB;
											row["hexcolor"] = mapUnit.hexcolor;
											row["DescriptionSourceID"] = 5; // mapUnit.DescriptionSourceID;
											row["GeoMaterial"] = mapUnit.GeoMaterial;
											row["GeoMaterialConfidence"] = mapUnit.GeoMaterialConfidence;

											//After all the changes are done, persist it.
											row.Store();

											// Has to be called after the store too.
											context.Invalidate(row);
										}
									}
								}
							}, enterpriseTable);

						} else {
							Debug.WriteLine("adding");

							editOperation.Callback(context => {
								TableDefinition tableDefinition = enterpriseTable.GetDefinition();
								using (RowBuffer rowBuffer = enterpriseTable.CreateRowBuffer()) {
									// Either the field index or the field name can be used in the indexer.
									rowBuffer["MapUnit"] = mapUnit.MU;
									rowBuffer["Name"] = mapUnit.Name;
									rowBuffer["FullName"] = mapUnit.FullName;
									rowBuffer["Age"] = mapUnit.Age;
									//rowBuffer["RelativeAge"] = mapUnit.RelativeAge;
									rowBuffer["Description"] = mapUnit.Description;
									rowBuffer["HierarchyKey"] = 5; // mapUnit.HierarchyKey;
									rowBuffer["ParagraphStyle"] = 5; // mapUnit.ParagraphStyle;
									rowBuffer["Label"] = mapUnit.Label;
									rowBuffer["Symbol"] = mapUnit.Symbol;
									rowBuffer["AreaFillRGB"] = mapUnit.AreaFillRGB;
									rowBuffer["hexcolor"] = mapUnit.hexcolor;
									rowBuffer["DescriptionSourceID"] = 5; // mapUnit.DescriptionSourceID;
									rowBuffer["GeoMaterial"] = mapUnit.GeoMaterial;
									rowBuffer["GeoMaterialConfidence"] = mapUnit.GeoMaterialConfidence;

									using (Row row = enterpriseTable.CreateRow(rowBuffer)) {
										// To Indicate that the attribute table has to be updated.
										context.Invalidate(row);
									}
								}
							}, enterpriseTable);

						}

						try {
							result = editOperation.Execute();
							if (!result) message = editOperation.ErrorMessage;
						} catch (GeodatabaseException exObj) {
							message = exObj.Message;
						}
					}
				}

				if (!string.IsNullOrEmpty(message))
					MessageBox.Show(message);
				else {
					UserEnteredMapUnit = null;
				}
			});

			//await DataHelper.populateMapUnits();

		}

	}

	/*
	/// <summary>
	/// Button implementation to show the DockPane.
	/// </summary>
	internal class AddEditMapUnitsDockPane_ShowButton : Button {
		protected override void OnClick() {
			AddEditMapUnitsDockPaneViewModel.Show();
		}
	}
	*/
}
