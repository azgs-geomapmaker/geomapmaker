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
						PostfixClause = "order by objectid"
					};

					using (RowCursor rowCursor = geodatabase.Evaluate(mapUnitsQDef, false)) {
						while (rowCursor.MoveNext()) {
							using (Row row = rowCursor.Current) {
								Debug.WriteLine(row["Name"].ToString());
								//mapUnits.Add(new ComboBoxItem(row["Name"].ToString()));

								var mapUnit = new MapUnit();

								mapUnit.ID = int.Parse(row["ObjectID"].ToString());
								mapUnit.MU = row["MapUnit"].ToString();
								mapUnit.Name = row["Name"].ToString();
								mapUnit.FullName = row["FullName"] == null ? null : row["FullName"].ToString();
								mapUnit.Age = row["Age"] == null ? null : row["Age"].ToString(); //TODO: more formatting here
																								 //mapUnit.RelativeAge = row["RelativeAge"].ToString(); //TODO: this is column missing in the table right now
								mapUnit.Description = row["Description"] == null ? null : row["Description"].ToString();
								mapUnit.HierarchyKey = row["HierarchyKey"].ToString();
								//mapUnit.ParagraphStyle = JsonConvert.DeserializeObject<List<string>>(row["ParagraphStyle"].ToString());
								//mapUnit.Label = row["Label"].ToString();
								//mapUnit.Symbol = row["Symbol"].ToString();
								//mapUnit.AreaFillRGB = row["AreaFillRGB"].ToString(); //TODO: more formatting here
								mapUnit.hexcolor = row["hexcolor"] == null ? null : row["hexcolor"].ToString();
								//mapUnit.Color = row["hexcolor"];
								//mapUnit.DescriptionSourceID = row["DescriptionSourceID"].ToString();
								mapUnit.GeoMaterial = row["GeoMaterial"] == null ? null : row["GeoMaterial"].ToString();
								mapUnit.GeoMaterialConfidence = row["GeoMaterialConfidence"] == null ? null : row["GeoMaterialConfidence"].ToString();

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

		//TODO: Might be nice to load this from a table in geomapmaker.public
		private static ObservableCollection<string> ages = new ObservableCollection<string>() {
			"Cenozoic",
			"Holocene",
			"Quaternary",
			"Pleistocene",
			"Late Pleistocene",
			"Middle Pleistocene",
			"Calabrian",
			"Gelasian",
			"Neogene",
			"Pliocene",
			"Piacenzian",
			"Zanclean",
			"Miocene",
			"Messinian",
			"Tortonian",
			"Serravallian",
			"Langhian",
			"Burdigalian",
			"Aquitanian",
			"Paleogene",
			"Oligocene",
			"Chattian",
			"Rupelian",
			"Eocene",
			"Priabonian",
			"Bartonian",
			"Lutetian",
			"Ypresian",
			"Paleocene",
			"Thanetian",
			"Selandian",
			"Danian",
			"Mesozoic",
			"Cretaceous",
			"Late Cretaceous",
			"Maastrichtian",
			"Campanian",
			"Santonian",
			"Coniacian",
			"Turonian",
			"Cenomanian",
			"Early Cretaceous",
			"Albian",
			"Aptian",
			"Barremian",
			"Hauterivian",
			"Valanginian",
			"Berriasian",
			"Jurassic",
			"Late Jurassic",
			"Tithonian",
			"Kimmeridgian",
			"Oxfordian",
			"Middle Jurassic",
			"Callovian",
			"Bathonian",
			"Bajocian",
			"Aalenian",
			"Early Jurassic",
			"Toarcian",
			"Pliensbachian",
			"Sinemurian",
			"Hettangian",
			"Triassic",
			"Late Triassic",
			"Rhaetian",
			"Norian",
			"Carnian",
			"Middle Triassic",
			"Ladinian",
			"Anisian",
			"Olenekian",
			"Early Triassic",
			"Induan",
			"Paleozoic",
			"Permian",
			"Changhsingian",
			"Lopingian",
			"Wuchiapingian",
			"Guadalupian",
			"Capitanian",
			"Wordian",
			"Roadian",
			"Kungurian",
			"Cisuralian",
			"Artinskian",
			"Sakmarian",
			"Asselian",
			"Carboniferous",
			"Pennsylvanian",
			"Gzhelian",
			"Kasimovian",
			"Moscovian",
			"Bashkirian",
			"Mississippian",
			"Serpukhovian",
			"Visean",
			"Tournaisian",
			"Devonian",
			"Late Devonian",
			"Famennian",
			"Frasnian",
			"Middle Devonian",
			"Givetian",
			"Eifelian",
			"Early Devonian",
			"Emsian",
			"Pragian",
			"Lochkovian",
			"Silurian",
			"Pridoli",
			"Ludlow",
			"Ludfordian",
			"Gorstian",
			"Wenlock",
			"Homerian",
			"Sheinwoodian",
			"Llandovery",
			"Telychian",
			"Aeronian",
			"Rhuddanian",
			"Ordovician",
			"Late Ordovician",
			"Hirnantian",
			"Katian",
			"Sandbian",
			"Middle Ordovician",
			"Darriwilian",
			"Dapingian",
			"Early Ordovician",
			"Floian",
			"Tremadocian",
			"Cambrian",
			"Furongian",
			"Stage 10",
			"Jiangshanian",
			"Paibian",
			"Guzhangian",
			"Miaolingian",
			"Drumian",
			"Wuliuan",
			"Stage 4",
			"Series 2",
			"Stage 3",
			"Terreneuvian",
			"Stage 2",
			"Fortunian",
			"Neoproterozoic",
			"Ediacaran",
			"Cryogenian",
			"Tonian",
			"Mesoproterozoic",
			"Stenian",
			"Ectasian",
			"Calymmian",
			"Paleoproterozoic",
			"Statherian",
			"Orosirian",
			"Rhyacian",
			"Siderian",
			"Neoarchean",
			"Mesoarchean",
			"Paleoarchean",
			"Eoarchean",
			"Rancholabrean",
			"Irvingtonian",
			"Blancan",
			"Hemphillian",
			"Clarendonian",
			"Barstovian",
			"Hemingfordian",
			"Arikareean",
			"Whitneyan",
			"Orellan",
			"Chadronian",
			"Duchesnean",
			"Uintan",
			"Bridgerian",
			"Wasatchian",
			"Clarkforkian",
			"Tiffanian",
			"Torrejonian",
			"Puercan",
			"Maastrichtian",
			"Campanian",
			"Santonian",
			"Coniacian",
			"Turonian",
			"Cenomanian",
			"Albian",
			"Aptian",
			"Barremian",
			"Hauterivian",
			"Valanginian",
			"Berriasian",
			"Tithonian",
			"Kimmeridgian",
			"Oxfordian",
			"Callovian",
			"Bathonian",
			"Bajocian",
			"Aalenian",
			"Toarcian",
			"Pliensbachian",
			"Sinemurian",
			"Hettangian",
			"Rhaetian",
			"Sevatian",
			"Alaunian",
			"Lacian",
			"Tuvalian",
			"Julian",
			"Ladinian",
			"Anisian",
			"Spathian",
			"Smithian",
			"Dienerian",
			"Griesbachian",
			"Ochoan",
			"Capitanian",
			"Wordian",
			"Roadian",
			"Leonardian",
			"Wolfcampian",
			"Virgilian",
			"Missourian",
			"Desmoinesian",
			"Atokan",
			"Morrowan",
			"Chesterian",
			"Meramecian",
			"Osagean",
			"Kinderhookian",
			"Chatauquan",
			"Senecan",
			"Erian",
			"Ulsterian",
			"Cayugan",
			"Niagaran",
			"Alexandrian",
			"Gamachian",
			"Richmondian",
			"Maysvillian",
			"Edenian",
			"Mohawkian",
			"Whiterock",
			"Blackhillsian",
			"Tulean",
			"Stairsian",
			"Skullrockian",
			"Sunwaptan",
			"Steptoean",
			"Marjuman",
			"Topazan",
			"Delamaran",
			"Dyeran",
			"Montezuman",
			"Begadean",
			"Hadrynian"
		 };
		public static ObservableCollection<string> Ages {
			get {
				return ages;
			}
		}

		private static ObservableCollection<string> geoMaterials = new ObservableCollection<string>() {
			"Sedimentary material",
			"Sediment",
			"Clastic sediment",
			"Sand and gravel of unspecified origin",
			"Silt and clay of unspecified origin",
			"Alluvial sediment",
			"Alluvial sediment, mostly coarse-grained",
			"Alluvial sediment, mostly fine-grained",
			"Glacial till",
			"Glacial till, mostly sandy",
			"Glacial till, mostly silty",
			"Glacial till, mostly clayey",
			"Ice-contact and ice-marginal sediment",
			"Ice-contact and ice-marginal sediment, mostly coarse-grained",
			"Ice-contact and ice-marginal sediment, mostly fine-grained",
			"Eolian sediment",
			"Dune sand",
			"Loess",
			"Lacustrine sediment",
			"Lacustrine sediment, mostly coarse-grained",
			"Lacustrine sediment, mostly fine-grained",
			"Playa sediment",
			"Coastal zone sediment",
			"Coastal zone sediment, mostly coarse-grained",
			"Coastal zone sediment, mostly fine-grained",
			"Marine sediment",
			"Marine sediment, mostly coarse-grained",
			"Marine sediment, mostly fine-grained",
			"Mass movement sediment",
			"Colluvium and other widespread mass-movement sediment",
			"Debris flows, landslides, and other localized mass-movement sediment",
			"Residual material",
			"Carbonate sediment",
			"Peat and muck",
			"Sedimentary rock",
			"Clastic sedimentary rock",
			"Conglomerate",
			"Sandstone",
			"Mostly sandstone",
			"Sandstone and mudstone",
			"Mudstone",
			"Mostly mudstone",
			"Carbonate rock",
			"Limestone",
			"Dolomite",
			"Mostly carbonate rock",
			"Chert",
			"Evaporitic rock",
			"Iron-rich sedimentary rock",
			"Coal and lignite",
			"Sedimentary and extrusive igneous material",
			"Igneous rock",
			"Extrusive igneous material",
			"Volcaniclastic (fragmental) material",
			"Pyroclastic flows",
			"Felsic-composition pyroclastic flows",
			"Intermediate-composition pyroclastic flows",
			"Mafic-composition pyroclastic flows",
			"Air-fall tephra",
			"Felsic-composition air-fall tephra",
			"Intermediate-composition air-fall tephra",
			"Mafic-composition air-fall tephra",
			"Lava flows",
			"Felsic-composition lava flows",
			"Intermediate-composition lava flows",
			"Mafic-composition lava flows",
			"Volcanic mass flow",
			"Intrusive igneous rock",
			"Coarse-grained intrusive igneous rock",
			"Coarse-grained, felsic-composition intrusive igneous rock",
			"Coarse-grained, intermediate-composition intrusive igneous rock",
			"Coarse-grained, mafic-composition intrusive igneous rock",
			"Ultramafic intrusive igneous rock",
			"Fine-grained intrusive igneous rock",
			"Fine-grained, felsic-composition intrusive igneous rock",
			"Fine-grained, intermediate-composition intrusive igneous rock",
			"Fine-grained, mafic-composition intrusive igneous rock",
			"Exotic-composition intrusive igneous rock",
			"Igneous and metamorphic rock",
			"Metamorphic rock",
			"Regional metamorphic rock, of unspecified origin",
			"Lower-grade metamorphic rock, of unspecified origin",
			"Medium and high-grade regional metamorphic rock, of unspecified origin",
			"Contact-metamorphic rock",
			"Deformation-related metamorphic rock",
			"Metasedimentary rock",
			"Slate and phyllite, of sedimentary-rock origin",
			"Schist and gneiss, of sedimentary-rock origin",
			"Meta-carbonate rock",
			"Quartzite",
			"Metaigneous rock",
			"Meta-ultramafic rock",
			"Meta-mafic rock",
			"Meta-felsic and intermediate rock",
			"Meta-volcaniclastic rock",
			"Other materials",
			"Rock and sediment",
			"Rock",
			"“Made” or human-engineered land",
			"Water or ice",
			"Unmapped area"
		};
		public static ObservableCollection<string> GeoMaterials {
			get {
				return geoMaterials;
			}
		}

		private static ObservableCollection<string> geoMaterialConfidences = new ObservableCollection<string>() {
			"High",
			"Medium",
			"Low"
		};
		public static ObservableCollection<string> GeoMaterialConfidences {
			get {
				return geoMaterialConfidences;
			}
		}

	}
}
