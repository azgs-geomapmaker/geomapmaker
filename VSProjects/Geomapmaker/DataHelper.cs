using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geomapmaker {
	public static class DataHelper {
		public static int userID;
		public static String userName;

		public static string connectionString;
		public static ArcGIS.Core.Data.DatabaseConnectionProperties connectionProperties;

		public static List<FeatureLayer> currentLayers = new List<FeatureLayer>();
		public static List<StandaloneTable> currentTables = new List<StandaloneTable>();

		//public static String mapUnitName { get; set; } = "Play A";
		public static event EventHandler MapUnitNameChanged;
		private static string mapUnitName;
		public static string MapUnitName {
			get => mapUnitName; 
			set {
				mapUnitName = value;
				MapUnitNameChanged?.Invoke(null, EventArgs.Empty);
			}
		}

		public static event EventHandler MapUnitsChanged;
		//private static ObservableCollection<ComboBoxItem> mapUnits = new ObservableCollection<ComboBoxItem>();
		private static ObservableCollection<MapUnit> mapUnits = new ObservableCollection<MapUnit>();
		public static ObservableCollection<MapUnit> MapUnits { 
			get => mapUnits;
			set {
				mapUnits = value;
				MapUnitsChanged?.Invoke(null, EventArgs.Empty);
			}
		}

		public static async Task populateMapUnits() {
			Debug.WriteLine("populateMapUnits enter");

			//var mapUnits = new ObservableCollection<ComboBoxItem>();
			var mapUnits = new ObservableCollection<MapUnit>();

			if (DataHelper.connectionProperties == null) {
				return;
			}

			await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() => {

				using (Geodatabase geodatabase = new Geodatabase(DataHelper.connectionProperties)) {

					//Table t = geodatabase.OpenDataset<Table>("DescriptionOfMapUnits");
					QueryDef mapUnitsQDef = new QueryDef {
						Tables = "DescriptionOfMapUnits",
						//WhereClause = "ADACOMPLY = 'Yes'",
					};

					using (RowCursor rowCursor = geodatabase.Evaluate(mapUnitsQDef, false)) {
						while (rowCursor.MoveNext()) {
							using (Row row = rowCursor.Current) {
								Debug.WriteLine(row["Name"].ToString());
								//mapUnits.Add(new ComboBoxItem(row["Name"].ToString()));

								var mapUnit = new MapUnit();
								
								//mapUnit.ID = int.Parse(row["DescriptionOfMapUnits_ID"].ToString());
								mapUnit.MU = row["MapUnit"].ToString();
								mapUnit.Name = row["Name"].ToString();
								mapUnit.FullName = row["FullName"].ToString();
								mapUnit.Age = row["Age"].ToString(); //TODO: more formatting here
								//mapUnit.RelativeAge = row["RelativeAge"].ToString(); //TODO: this is column missing in the table right now
								mapUnit.Description = row["Description"].ToString();
								mapUnit.HierarchyKey = row["HierarchyKey"].ToString();
								mapUnit.ParagraphStyle = JsonConvert.DeserializeObject<List<string>>(row["ParagraphStyle"].ToString());
								//mapUnit.Label = row["Label"].ToString();
								//mapUnit.Symbol = row["Symbol"].ToString();
								//mapUnit.AreaFillRGB = row["AreaFillRGB"].ToString(); //TODO: more formatting here
								//mapUnit.hexcolor = row["hexcolor"].ToString();
								//mapUnit.DescriptionSourceID = row["DescriptionSourceID"].ToString();
								mapUnit.GeoMaterial = row["GeoMaterial"].ToString();
								mapUnit.GeoMaterialConfidence = row["GeoMaterialConfidence"].ToString();

								mapUnits.Add(mapUnit);
							}
						}
					}

				}
			});
			DataHelper.MapUnits = mapUnits;
		}

		/*
		public static event EventHandler SelectedMapUnitChanged;
		private static string selectedMapUnit;
		public static string SelectedMapUnit {
			get  => selectedMapUnit; 
			set {
				// populate form
				selectedMapUnit = value;
				SelectedMapUnitChanged?.Invoke(null, EventArgs.Empty);
			}
		}
		*/

		public delegate void UserLoginDelegate();
		public static event UserLoginDelegate UserLoginHandler;
		public static void UserLogin(int uID, String uName) {
			userID = uID;
			userName = uName;
			UserLoginHandler?.Invoke();
		}

		public delegate void ProjectSelectedDelegate();
		public static event ProjectSelectedDelegate ProjectSelectedHandler;
		public static void ProjectSelected() {
			ProjectSelectedHandler?.Invoke();
		}

	}
}
