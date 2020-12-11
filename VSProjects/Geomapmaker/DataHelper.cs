﻿using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Geomapmaker {
	public static class DataHelper {
		public static int userID;
		public static String userName;

		public static ArcGIS.Core.Data.DatabaseConnectionProperties connectionProperties;
		public static void setConnectionProperties(JObject props) {
			connectionProperties = new DatabaseConnectionProperties(EnterpriseDatabaseType.PostgreSQL) {
				AuthenticationMode = AuthenticationMode.DBMS,
				Instance = props["instance"].ToString(),
				Database = props["database"].ToString(),
				User = props["user"].ToString(),
				Password = props["password"].ToString(),
			};
		}

		public static List<FeatureLayer> currentLayers = new List<FeatureLayer>();
		public static List<StandaloneTable> currentTables = new List<StandaloneTable>();

		public static CIMUniqueValueRenderer mapUnitRenderer;
		public static CIMUniqueValueRenderer cfRenderer;

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

		private static ObservableCollection<MapUnit> mapUnits = new ObservableCollection<MapUnit>();
		public static ObservableCollection<MapUnit> MapUnits {
			get => mapUnits;
			set {
				mapUnits = value;
				MapUnitsChanged?.Invoke(null, EventArgs.Empty);
			}
		}

		public static event EventHandler CFChanged;
		private static ObservableCollection<CF> cfs = new ObservableCollection<CF>();
		public static ObservableCollection<CF> CFs {
			get => cfs;
			set {
				cfs = value;
				CFChanged?.Invoke(null, EventArgs.Empty);
			}
		}

		//Populate our map units list from the database and create unique value renderer for mapunitpolys featureclass 
		//using colors from the descriptionofmapunits table. This uses a variation of the technique presented here: 
		//https://community.esri.com/message/960006-how-to-render-color-of-individual-features-based-on-field-in-another-table
		//and here: https://community.esri.com/thread/217968-unique-value-renderer-specifying-values
		public static async Task populateMapUnits() {
			Debug.WriteLine("populateMapUnits enter");

			//var mapUnits = new ObservableCollection<ComboBoxItem>();
			var mapUnits = new ObservableCollection<MapUnit>();

			if (DataHelper.connectionProperties == null) {
				return;
			}

			await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() => {

				using (Geodatabase geodatabase = new Geodatabase(DataHelper.connectionProperties)) {

					//Get list of map units currently in feature class
					List<string> muInFeatureClass = new List<string>();
					QueryDef cfQDef = new QueryDef {
						Tables = "MapUnitPolys"
					};
					using (RowCursor rowCursor = geodatabase.Evaluate(cfQDef, false)) {
						while (rowCursor.MoveNext()) {
							using (Row row = rowCursor.Current) {
							muInFeatureClass.Add(row["MapUnit"].ToString());
							}
						}
					}

					//Table t = geodatabase.OpenDataset<Table>("DescriptionOfMapUnits");
					QueryDef mapUnitsQDef = new QueryDef {
						Tables = "DescriptionOfMapUnits",
						PostfixClause = "order by objectid"
					};

					using (RowCursor rowCursor = geodatabase.Evaluate(mapUnitsQDef, false)) {
						List<CIMUniqueValueClass> listUniqueValueClasses = new List<CIMUniqueValueClass>();
						while (rowCursor.MoveNext()) {
							using (Row row = rowCursor.Current) {
								Debug.WriteLine(row["Name"].ToString());

								//create and load map unit
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

								//add it to our list
								mapUnits.Add(mapUnit);

								//Only add to renderer if map unit is in the feature class
								if (muInFeatureClass.Contains(row["MapUnit"].ToString())) {
									//Create a "CIMUniqueValueClass" for the map unit and add it to the list of unique values.
									//This is what creates the mapping from map unit to color
									List<CIMUniqueValue> listUniqueValues = new List<CIMUniqueValue> {
											new CIMUniqueValue {
												FieldValues = new string[] { mapUnit.MU }
											}
										};
									string colorString = mapUnit.AreaFillRGB;
									string[] strVals = colorString.Split(';');
									var cF = ColorFactory.Instance;
									CIMUniqueValueClass uniqueValueClass = new CIMUniqueValueClass {
										Editable = true,
										Label = mapUnit.MU,
										//Patch = PatchShape.Default,
										Patch = PatchShape.AreaPolygon,
										Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(cF.CreateRGBColor(Double.Parse(strVals[0]), Double.Parse(strVals[1]), Double.Parse(strVals[2]))).MakeSymbolReference(),
										Visible = true,
										Values = listUniqueValues.ToArray()
									};
									listUniqueValueClasses.Add(uniqueValueClass);
								}

							}
						}

						//Create a list of CIMUniqueValueGroup
						CIMUniqueValueGroup uvg = new CIMUniqueValueGroup {
							Classes = listUniqueValueClasses.ToArray(),
						};
						List<CIMUniqueValueGroup> listUniqueValueGroups = new List<CIMUniqueValueGroup> { uvg };

						//Use the list to create the CIMUniqueValueRenderer
						DataHelper.mapUnitRenderer = new CIMUniqueValueRenderer {
							UseDefaultSymbol = false,
							//DefaultLabel = "all other values",
							//DefaultSymbolPatch = PatchShape.AreaPolygon,
							//DefaultSymbol = SymbolFactory.Instance.ConstructPolygonSymbol(ColorFactory.Instance.GreyRGB).MakeSymbolReference(),
							Groups = listUniqueValueGroups.ToArray(),
							Fields = new string[] { "mapunit" }
						};

						//Set renderer in mapunit. The try/catch is there because the first time through, this is called 
						//before the layer has been added to the map. We just ignore the error in that case.
						try {
							var muLayer = MapView.Active.Map.GetLayersAsFlattenedList().First((l) => l.Name == "MapUnitPolys") as FeatureLayer;
							muLayer.SetRenderer(DataHelper.mapUnitRenderer);
						} catch { }

					}

				}
			});
			DataHelper.MapUnits = mapUnits;
		}


		public static async Task populateContactsAndFaults() {
			Debug.WriteLine("populateContactsAndFaults enter");

			var cfs = new ObservableCollection<CF>();

			if (DataHelper.connectionProperties == null) {
				return;
			}

			await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() => {

				using (Geodatabase geodatabase = new Geodatabase(DataHelper.connectionProperties)) {

					List<string> cfInFeatureClass = new List<string>();
					QueryDef cfQDef = new QueryDef {
						Tables = "ContactsAndFaults"
					};
					using (RowCursor rowCursor = geodatabase.Evaluate(cfQDef, false)) {
						while (rowCursor.MoveNext()) {
							using (Row row = rowCursor.Current) {
								//cfInFeatureClass.Add(row["symbol"] + "." + row["label"]);
								cfInFeatureClass.Add(row["symbol"].ToString());
							}
						}
					}

					QueryDef cfSymbolQDef = new QueryDef {
						Tables = "CFSymbology",
						PostfixClause = "order by key"
					};

					using (RowCursor rowCursor = geodatabase.Evaluate(cfSymbolQDef, false)) {
						List<CIMUniqueValueClass> listUniqueValueClasses = new List<CIMUniqueValueClass>();
						while (rowCursor.MoveNext()) {
							using (Row row = rowCursor.Current) {
								Debug.WriteLine(row["key"].ToString());

								//create and load map unit
								var cf = new CF();
								cf.key = row["key"].ToString();
								cf.symbol = row["symbol"].ToString();
								//Wrap the symbol JSON in CIMSymbolReference, so we can use that class to deserialize it.
								cf.symbol = cf.symbol.Insert(0, "{\"type\": \"CIMSymbolReference\", \"symbol\": ");
								cf.symbol = cf.symbol.Insert(cf.symbol.Length, "}");

								//Create the preview image used in the ComboBox
								SymbolStyleItem sSI = new SymbolStyleItem() {
									Symbol = CIMSymbolReference.FromJson(cf.symbol).Symbol,
									PatchWidth = 50,
									PatchHeight = 25
								};
								cf.preview = sSI.PreviewImage;

								//add it to our list
								cfs.Add(cf);

								//Only add to renderer if present in the feature class
								if (cfInFeatureClass.Contains(cf.key)) {
									//Create a "CIMUniqueValueClass" for the cf and add it to the list of unique values.
									//This is what creates the mapping from cf derived attribute to symbol
									List<CIMUniqueValue> listUniqueValues = new List<CIMUniqueValue> {
										new CIMUniqueValue {
											FieldValues = new string[] { cf.key }
										}
									};

									CIMUniqueValueClass uniqueValueClass = new CIMUniqueValueClass {
										Editable = true,
										Label = cf.key,
										//Patch = PatchShape.Default,
										Patch = PatchShape.AreaPolygon,
										Symbol = CIMSymbolReference.FromJson(cf.symbol, null),
										Visible = true,
										Values = listUniqueValues.ToArray()
									};
									listUniqueValueClasses.Add(uniqueValueClass);
								}
							}
						}

						//Create a list of CIMUniqueValueGroup
						CIMUniqueValueGroup uvg = new CIMUniqueValueGroup {
							Classes = listUniqueValueClasses.ToArray(),
						};
						List<CIMUniqueValueGroup> listUniqueValueGroups = new List<CIMUniqueValueGroup> { uvg };

						//This specifies the derived attribute. 
						//TODO: Currently using the wrong attributes here simply because they were convenient.
						//Final version will use Type, Subtype, Symbol.
						/*
						CIMExpressionInfo cEI = new CIMExpressionInfo() {
							Expression = "Concatenate([$feature.Symbol, $feature.Label], '.')"
						};
						*/

						//Use the list to create the CIMUniqueValueRenderer
						DataHelper.cfRenderer = new CIMUniqueValueRenderer {
							UseDefaultSymbol = false,
							Groups = listUniqueValueGroups.ToArray(),
							Fields = new string[] { "symbol" }
							//ValueExpressionInfo = cEI //fields used for testing
						};

						//Set renderer in mapunit. The try/catch is there because the first time through, this is called 
						//before the layer has been added to the map. We just ignore the error in that case.
						try {
							var cfLayer = MapView.Active.Map.GetLayersAsFlattenedList().First((l) => l.Name == "ContactsAndFaults") as FeatureLayer;
							cfLayer.SetRenderer(DataHelper.cfRenderer);
						} catch { }

					}

				}
			});
			DataHelper.CFs = cfs;
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

		private static ObservableCollection<string> identityConfidences = new ObservableCollection<string>() {
			"High",
			"Medium",
			"Low"
		};
		public static ObservableCollection<string> IdentityConfidences {
			get {
				return identityConfidences;
			}
		}

		private static ObservableCollection<string> existenceConfidences = new ObservableCollection<string>() {
			"High",
			"Medium",
			"Low"
		};
		public static ObservableCollection<string> ExistenceConfidences {
			get {
				return existenceConfidences;
			}
		}

		private static ObservableCollection<string> locationConfidenceMeters = new ObservableCollection<string>() {
			"10",
			"25",
			"50",
			"100",
			"250"
		};
		public static ObservableCollection<string> LocationConfidenceMeters {
			get {
				return locationConfidenceMeters;
			}
		}

		private static ObservableCollection<string> concealedYN = new ObservableCollection<string>() {
			"Y",
			"N"
		};
		public static ObservableCollection<string> ConcealedYN {
			get {
				return concealedYN;
			}
		}

	}
}
