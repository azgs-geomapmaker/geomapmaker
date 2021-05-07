using ArcGIS.Core.CIM;
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
		private static ObservableCollection<CFSymbol> cfSymbols = new ObservableCollection<CFSymbol>();
		public static ObservableCollection<CFSymbol> CFSymbols {
			get => cfSymbols;
			set {
				cfSymbols = value;
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

			var cfSymbols = new ObservableCollection<CFSymbol>();

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
						SubFields = "key,description,symbol", //TODO: I'm not sure why I have to explicitly list these to get description, but it seems I do
						PostfixClause = "order by key"
					};

					using (RowCursor rowCursor = geodatabase.Evaluate(cfSymbolQDef, false)) {
						List<CIMUniqueValueClass> listUniqueValueClasses = new List<CIMUniqueValueClass>();
						while (rowCursor.MoveNext()) {
							using (Row row = rowCursor.Current) {
								Debug.WriteLine(row["key"].ToString());

								//create and load map unit
								CFSymbol cfS = new CFSymbol();
								cfS.key = row["key"].ToString();
								cfS.description = row["description"] == null ? "" : row["description"].ToString();
								cfS.symbol = row["symbol"].ToString();
								//Wrap the symbol JSON in CIMSymbolReference, so we can use that class to deserialize it.
								cfS.symbol = cfS.symbol.Insert(0, "{\"type\": \"CIMSymbolReference\", \"symbol\": ");
								cfS.symbol = cfS.symbol.Insert(cfS.symbol.Length, "}");

								//Create the preview image used in the ComboBox
								SymbolStyleItem sSI = new SymbolStyleItem() {
									Symbol = CIMSymbolReference.FromJson(cfS.symbol).Symbol,
									PatchWidth = 50,
									PatchHeight = 25
								};
								cfS.preview = sSI.PreviewImage;

								//add it to our list
								cfSymbols.Add(cfS);

								//Only add to renderer if present in the feature class
								if (cfInFeatureClass.Contains(cfS.key)) {
									//Create a "CIMUniqueValueClass" for the cf and add it to the list of unique values.
									//This is what creates the mapping from cf derived attribute to symbol
									List<CIMUniqueValue> listUniqueValues = new List<CIMUniqueValue> {
										new CIMUniqueValue {
											FieldValues = new string[] { cfS.key }
										}
									};

									CIMUniqueValueClass uniqueValueClass = new CIMUniqueValueClass {
										Editable = true,
										Label = cfS.key,
										//Patch = PatchShape.Default,
										Patch = PatchShape.AreaPolygon,
										Symbol = CIMSymbolReference.FromJson(cfS.symbol, null),
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

						//Set renderer in cf. The try/catch is there because the first time through, this is called 
						//before the layer has been added to the map. We just ignore the error in that case.
						try {
							var cfLayer = MapView.Active.Map.GetLayersAsFlattenedList().First((l) => l.Name == "ContactsAndFaults") as FeatureLayer;
							cfLayer.SetRenderer(DataHelper.cfRenderer);
						} catch { }

					}

				}
			});
			DataHelper.CFSymbols = cfSymbols;
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


		//These intervals and their ranges are, with a little post-processing, from:
		//	https://macrostrat.org/api/v1/defs/intervals?timescale_id=11
		//	https://macrostrat.org/api/v1/defs/intervals?timescale_id=17
		//TODO: Might be nice to load this from a table in geomapmaker.public
		private static ObservableCollection<Interval> intervals = new ObservableCollection<Interval>() {
			new Interval {name ="Cenozoic", range="66mya-0mya"},
			new Interval {name ="Holocene", range="0.0117mya-0mya"},
			new Interval {name ="Quaternary", range="2.588mya-0mya"},
			new Interval {name ="Pleistocene", range="2.588mya-0.0117mya"},
			new Interval {name ="Late Pleistocene", range="0.126mya-0.0117mya"},
			new Interval {name ="Middle Pleistocene", range="0.781mya-0.126mya"},
			new Interval {name ="Calabrian", range="1.806mya-0.781mya"},
			new Interval {name ="Gelasian", range="2.588mya-1.806mya"},
			new Interval {name ="Neogene", range="23.03mya-2.588mya"},
			new Interval {name ="Pliocene", range="5.333mya-2.588mya"},
			new Interval {name ="Piacenzian", range="3.6mya-2.588mya"},
			new Interval {name ="Zanclean", range="5.333mya-3.6mya"},
			new Interval {name ="Miocene", range="23.03mya-5.333mya"},
			new Interval {name ="Messinian", range="7.246mya-5.333mya"},
			new Interval {name ="Tortonian", range="11.62mya-7.246mya"},
			new Interval {name ="Serravallian", range="13.82mya-11.62mya"},
			new Interval {name ="Langhian", range="15.97mya-13.82mya"},
			new Interval {name ="Burdigalian", range="20.44mya-15.97mya"},
			new Interval {name ="Aquitanian", range="23.03mya-20.44mya"},
			new Interval {name ="Paleogene", range="66mya-23.03mya"},
			new Interval {name ="Oligocene", range="33.9mya-23.03mya"},
			new Interval {name ="Chattian", range="28.1mya-23.03mya"},
			new Interval {name ="Rupelian", range="33.9mya-28.1mya"},
			new Interval {name ="Eocene", range="56mya-33.9mya"},
			new Interval {name ="Priabonian", range="37.8mya-33.9mya"},
			new Interval {name ="Bartonian", range="41.3mya-37.8mya"},
			new Interval {name ="Lutetian", range="47.8mya-41.3mya"},
			new Interval {name ="Ypresian", range="56mya-47.8mya"},
			new Interval {name ="Paleocene", range="66mya-56mya"},
			new Interval {name ="Thanetian", range="59.2mya-56mya"},
			new Interval {name ="Selandian", range="61.6mya-59.2mya"},
			new Interval {name ="Danian", range="66mya-61.6mya"},
			new Interval {name ="Mesozoic", range="251.902mya-66mya"},
			new Interval {name ="Cretaceous", range="145mya-66mya"},
			new Interval {name ="Late Cretaceous", range="100.5mya-66mya"},
			new Interval {name ="Maastrichtian", range="72.1mya-66mya"},
			new Interval {name ="Campanian", range="83.6mya-72.1mya"},
			new Interval {name ="Santonian", range="86.3mya-83.6mya"},
			new Interval {name ="Coniacian", range="89.8mya-86.3mya"},
			new Interval {name ="Turonian", range="93.9mya-89.8mya"},
			new Interval {name ="Cenomanian", range="100.5mya-93.9mya"},
			new Interval {name ="Early Cretaceous", range="145mya-100.5mya"},
			new Interval {name ="Albian", range="113mya-100.5mya"},
			new Interval {name ="Aptian", range="125mya-113mya"},
			new Interval {name ="Barremian", range="129.4mya-125mya"},
			new Interval {name ="Hauterivian", range="132.9mya-129.4mya"},
			new Interval {name ="Valanginian", range="139.8mya-132.9mya"},
			new Interval {name ="Berriasian", range="145mya-139.8mya"},
			new Interval {name ="Jurassic", range="201.3mya-145mya"},
			new Interval {name ="Late Jurassic", range="163.5mya-145mya"},
			new Interval {name ="Tithonian", range="152.1mya-145mya"},
			new Interval {name ="Kimmeridgian", range="157.3mya-152.1mya"},
			new Interval {name ="Oxfordian", range="163.5mya-157.3mya"},
			new Interval {name ="Middle Jurassic", range="174.1mya-163.5mya"},
			new Interval {name ="Callovian", range="166.1mya-163.5mya"},
			new Interval {name ="Bathonian", range="168.3mya-166.1mya"},
			new Interval {name ="Bajocian", range="170.3mya-168.3mya"},
			new Interval {name ="Aalenian", range="174.1mya-170.3mya"},
			new Interval {name ="Early Jurassic", range="201.3mya-174.1mya"},
			new Interval {name ="Toarcian", range="182.7mya-174.1mya"},
			new Interval {name ="Pliensbachian", range="190.8mya-182.7mya"},
			new Interval {name ="Sinemurian", range="199.3mya-190.8mya"},
			new Interval {name ="Hettangian", range="201.3mya-199.3mya"},
			new Interval {name ="Triassic", range="251.902mya-201.3mya"},
			new Interval {name ="Late Triassic", range="237mya-201.3mya"},
			new Interval {name ="Rhaetian", range="208.5mya-201.3mya"},
			new Interval {name ="Norian", range="227mya-208.5mya"},
			new Interval {name ="Carnian", range="237mya-227mya"},
			new Interval {name ="Middle Triassic", range="247.2mya-237mya"},
			new Interval {name ="Ladinian", range="242mya-237mya"},
			new Interval {name ="Anisian", range="247.2mya-242mya"},
			new Interval {name ="Olenekian", range="251.2mya-247.2mya"},
			new Interval {name ="Early Triassic", range="251.902mya-247.2mya"},
			new Interval {name ="Induan", range="251.902mya-251.2mya"},
			new Interval {name ="Paleozoic", range="541mya-251.902mya"},
			new Interval {name ="Permian", range="298.9mya-251.902mya"},
			new Interval {name ="Changhsingian", range="254.14mya-251.902mya"},
			new Interval {name ="Lopingian", range="259.1mya-251.902mya"},
			new Interval {name ="Wuchiapingian", range="259.1mya-254.14mya"},
			new Interval {name ="Guadalupian", range="272.95mya-259.1mya"},
			new Interval {name ="Capitanian", range="265.1mya-259.1mya"},
			new Interval {name ="Wordian", range="268.8mya-265.1mya"},
			new Interval {name ="Roadian", range="272.95mya-268.8mya"},
			new Interval {name ="Kungurian", range="283.5mya-272.95mya"},
			new Interval {name ="Cisuralian", range="298.9mya-272.95mya"},
			new Interval {name ="Artinskian", range="290.1mya-283.5mya"},
			new Interval {name ="Sakmarian", range="295mya-290.1mya"},
			new Interval {name ="Asselian", range="298.9mya-295mya"},
			new Interval {name ="Carboniferous", range="358.9mya-298.9mya"},
			new Interval {name ="Pennsylvanian", range="323.2mya-298.9mya"},
			new Interval {name ="Gzhelian", range="303.7mya-298.9mya"},
			new Interval {name ="Kasimovian", range="307mya-303.7mya"},
			new Interval {name ="Moscovian", range="315.2mya-307mya"},
			new Interval {name ="Bashkirian", range="323.2mya-315.2mya"},
			new Interval {name ="Mississippian", range="358.9mya-323.2mya"},
			new Interval {name ="Serpukhovian", range="330.9mya-323.2mya"},
			new Interval {name ="Visean", range="346.7mya-330.9mya"},
			new Interval {name ="Tournaisian", range="358.9mya-346.7mya"},
			new Interval {name ="Devonian", range="419.2mya-358.9mya"},
			new Interval {name ="Late Devonian", range="382.7mya-358.9mya"},
			new Interval {name ="Famennian", range="372.2mya-358.9mya"},
			new Interval {name ="Frasnian", range="382.7mya-372.2mya"},
			new Interval {name ="Middle Devonian", range="393.3mya-382.7mya"},
			new Interval {name ="Givetian", range="387.7mya-382.7mya"},
			new Interval {name ="Eifelian", range="393.3mya-387.7mya"},
			new Interval {name ="Early Devonian", range="419.2mya-393.3mya"},
			new Interval {name ="Emsian", range="407.6mya-393.3mya"},
			new Interval {name ="Pragian", range="410.8mya-407.6mya"},
			new Interval {name ="Lochkovian", range="419.2mya-410.8mya"},
			new Interval {name ="Silurian", range="443.8mya-419.2mya"},
			new Interval {name ="Pridoli", range="423mya-419.2mya"},
			new Interval {name ="Ludlow", range="427.4mya-423mya"},
			new Interval {name ="Ludfordian", range="425.6mya-423mya"},
			new Interval {name ="Gorstian", range="427.4mya-425.6mya"},
			new Interval {name ="Wenlock", range="433.4mya-427.4mya"},
			new Interval {name ="Homerian", range="430.5mya-427.4mya"},
			new Interval {name ="Sheinwoodian", range="433.4mya-430.5mya"},
			new Interval {name ="Llandovery", range="443.8mya-433.4mya"},
			new Interval {name ="Telychian", range="438.5mya-433.4mya"},
			new Interval {name ="Aeronian", range="440.8mya-438.5mya"},
			new Interval {name ="Rhuddanian", range="443.8mya-440.8mya"},
			new Interval {name ="Ordovician", range="485.4mya-443.8mya"},
			new Interval {name ="Late Ordovician", range="458.4mya-443.8mya"},
			new Interval {name ="Hirnantian", range="445.2mya-443.8mya"},
			new Interval {name ="Katian", range="453mya-445.2mya"},
			new Interval {name ="Sandbian", range="458.4mya-453mya"},
			new Interval {name ="Middle Ordovician", range="470mya-458.4mya"},
			new Interval {name ="Darriwilian", range="467.3mya-458.4mya"},
			new Interval {name ="Dapingian", range="470mya-467.3mya"},
			new Interval {name ="Early Ordovician", range="485.4mya-470mya"},
			new Interval {name ="Floian", range="477.7mya-470mya"},
			new Interval {name ="Tremadocian", range="485.4mya-477.7mya"},
			new Interval {name ="Cambrian", range="541mya-485.4mya"},
			new Interval {name ="Furongian", range="497mya-485.4mya"},
			new Interval {name ="Stage 10", range="489.5mya-485.4mya"},
			new Interval {name ="Jiangshanian", range="494mya-489.5mya"},
			new Interval {name ="Paibian", range="497mya-494mya"},
			new Interval {name ="Guzhangian", range="500.5mya-497mya"},
			new Interval {name ="Miaolingian", range="509mya-497mya"},
			new Interval {name ="Drumian", range="504.5mya-500.5mya"},
			new Interval {name ="Wuliuan", range="509mya-504.5mya"},
			new Interval {name ="Stage 4", range="514mya-509mya"},
			new Interval {name ="Series 2", range="521mya-509mya"},
			new Interval {name ="Stage 3", range="521mya-514mya"},
			new Interval {name ="Terreneuvian", range="541mya-521mya"},
			new Interval {name ="Stage 2", range="529mya-521mya"},
			new Interval {name ="Fortunian", range="541mya-529mya"},
			new Interval {name ="Neoproterozoic", range="1000mya-541mya"},
			new Interval {name ="Ediacaran", range="635mya-541mya"},
			new Interval {name ="Cryogenian", range="720mya-635mya"},
			new Interval {name ="Tonian", range="1000mya-720mya"},
			new Interval {name ="Mesoproterozoic", range="1600mya-1000mya"},
			new Interval {name ="Stenian", range="1200mya-1000mya"},
			new Interval {name ="Ectasian", range="1400mya-1200mya"},
			new Interval {name ="Calymmian", range="1600mya-1400mya"},
			new Interval {name ="Paleoproterozoic", range="2500mya-1600mya"},
			new Interval {name ="Statherian", range="1800mya-1600mya"},
			new Interval {name ="Orosirian", range="2050mya-1800mya"},
			new Interval {name ="Rhyacian", range="2300mya-2050mya"},
			new Interval {name ="Siderian", range="2500mya-2300mya"},
			new Interval {name ="Neoarchean", range="2800mya-2500mya"},
			new Interval {name ="Mesoarchean", range="3200mya-2800mya"},
			new Interval {name ="Paleoarchean", range="3600mya-3200mya"},
			new Interval {name ="Eoarchean", range="4000mya-3600mya"},
			new Interval {name ="Rancholabrean", range="0.3mya-0.0114mya"},
			new Interval {name ="Irvingtonian", range="1.806mya-0.3mya"},
			new Interval {name ="Blancan", range="4.9mya-1.806mya"},
			new Interval {name ="Hemphillian", range="10.3mya-4.9mya"},
			new Interval {name ="Clarendonian", range="13.6mya-10.3mya"},
			new Interval {name ="Barstovian", range="16.3mya-13.6mya"},
			new Interval {name ="Hemingfordian", range="18.6mya-16.3mya"},
			new Interval {name ="Arikareean", range="29.8mya-18.6mya"},
			new Interval {name ="Whitneyan", range="32.1mya-29.8mya"},
			new Interval {name ="Orellan", range="33.9mya-32.1mya"},
			new Interval {name ="Chadronian", range="37.8mya-33.9mya"},
			new Interval {name ="Duchesnean", range="39.9mya-37.8mya"},
			new Interval {name ="Uintan", range="47.9mya-39.9mya"},
			new Interval {name ="Bridgerian", range="52mya-47.9mya"},
			new Interval {name ="Wasatchian", range="56mya-52mya"},
			new Interval {name ="Clarkforkian", range="57.5mya-56mya"},
			new Interval {name ="Tiffanian", range="62.25mya-57.5mya"},
			new Interval {name ="Torrejonian", range="64.75mya-62.25mya"},
			new Interval {name ="Puercan", range="66mya-64.75mya"},
			new Interval {name ="Maastrichtian", range="72.1mya-66mya"},
			new Interval {name ="Campanian", range="83.6mya-72.1mya"},
			new Interval {name ="Santonian", range="86.3mya-83.6mya"},
			new Interval {name ="Coniacian", range="89.8mya-86.3mya"},
			new Interval {name ="Turonian", range="93.9mya-89.8mya"},
			new Interval {name ="Cenomanian", range="100.5mya-93.9mya"},
			new Interval {name ="Albian", range="113mya-100.5mya"},
			new Interval {name ="Aptian", range="125mya-113mya"},
			new Interval {name ="Barremian", range="129.4mya-125mya"},
			new Interval {name ="Hauterivian", range="132.9mya-129.4mya"},
			new Interval {name ="Valanginian", range="139.8mya-132.9mya"},
			new Interval {name ="Berriasian", range="145mya-139.8mya"},
			new Interval {name ="Tithonian", range="152.1mya-145mya"},
			new Interval {name ="Kimmeridgian", range="157.3mya-152.1mya"},
			new Interval {name ="Oxfordian", range="163.5mya-157.3mya"},
			new Interval {name ="Callovian", range="166.1mya-163.5mya"},
			new Interval {name ="Bathonian", range="168.3mya-166.1mya"},
			new Interval {name ="Bajocian", range="170.3mya-168.3mya"},
			new Interval {name ="Aalenian", range="174.1mya-170.3mya"},
			new Interval {name ="Toarcian", range="182.7mya-174.1mya"},
			new Interval {name ="Pliensbachian", range="190.8mya-182.7mya"},
			new Interval {name ="Sinemurian", range="199.3mya-190.8mya"},
			new Interval {name ="Hettangian", range="201.3mya-199.3mya"},
			new Interval {name ="Rhaetian", range="208.5mya-201.3mya"},
			new Interval {name ="Sevatian", range="214mya-208.5mya"},
			new Interval {name ="Alaunian", range="216mya-214mya"},
			new Interval {name ="Lacian", range="227mya-216mya"},
			new Interval {name ="Tuvalian", range="230.5mya-227mya"},
			new Interval {name ="Julian", range="237mya-230.5mya"},
			new Interval {name ="Ladinian", range="242mya-237mya"},
			new Interval {name ="Anisian", range="247.2mya-242mya"},
			new Interval {name ="Spathian", range="248.5mya-247.2mya"},
			new Interval {name ="Smithian", range="251.2mya-248.5mya"},
			new Interval {name ="Dienerian", range="251.6mya-251.2mya"},
			new Interval {name ="Griesbachian", range="251.902mya-251.6mya"},
			new Interval {name ="Ochoan", range="259.1mya-259mya"},
			new Interval {name ="Capitanian", range="265.1mya-259.1mya"},
			new Interval {name ="Wordian", range="268.8mya-265.1mya"},
			new Interval {name ="Roadian", range="272.95mya-268.8mya"},
			new Interval {name ="Leonardian", range="282mya-272.95mya"},
			new Interval {name ="Wolfcampian", range="298.9mya-282mya"},
			new Interval {name ="Virgilian", range="303.7mya-298.9mya"},
			new Interval {name ="Missourian", range="306mya-303.7mya"},
			new Interval {name ="Desmoinesian", range="312mya-306mya"},
			new Interval {name ="Atokan", range="319mya-312mya"},
			new Interval {name ="Morrowan", range="323.2mya-319mya"},
			new Interval {name ="Chesterian", range="335mya-323.2mya"},
			new Interval {name ="Meramecian", range="343.5mya-335mya"},
			new Interval {name ="Osagean", range="351.5mya-343.5mya"},
			new Interval {name ="Kinderhookian", range="358.9mya-351.5mya"},
			new Interval {name ="Chatauquan", range="370mya-358.9mya"},
			new Interval {name ="Senecan", range="385mya-370mya"},
			new Interval {name ="Erian", range="393.3mya-385mya"},
			new Interval {name ="Ulsterian", range="419.2mya-393.3mya"},
			new Interval {name ="Cayugan", range="425.6mya-419.2mya"},
			new Interval {name ="Niagaran", range="438.5mya-425.6mya"},
			new Interval {name ="Alexandrian", range="443.8mya-438.5mya"},
			new Interval {name ="Gamachian", range="445.2mya-443.8mya"},
			new Interval {name ="Richmondian", range="449mya-445.2mya"},
			new Interval {name ="Maysvillian", range="450mya-449mya"},
			new Interval {name ="Edenian", range="451mya-450mya"},
			new Interval {name ="Mohawkian", range="456.5mya-451mya"},
			new Interval {name ="Whiterock", range="470mya-456.5mya"},
			new Interval {name ="Blackhillsian", range="475mya-470mya"},
			new Interval {name ="Tulean", range="479.5mya-475mya"},
			new Interval {name ="Stairsian", range="480mya-479.5mya"},
			new Interval {name ="Skullrockian", range="486.7mya-480mya"},
			new Interval {name ="Sunwaptan", range="493mya-486.7mya"},
			new Interval {name ="Steptoean", range="497mya-493mya"},
			new Interval {name ="Marjuman", range="504.5mya-497mya"},
			new Interval {name ="Topazan", range="506.5mya-504.5mya"},
			new Interval {name ="Delamaran", range="511mya-506.5mya"},
			new Interval {name ="Dyeran", range="515.5mya-511mya"},
			new Interval {name ="Montezuman", range="520mya-515.5mya"},
			new Interval {name ="Begadean", range="541mya-520mya"},
			new Interval {name ="Hadrynian", range="850mya-541mya"}
		};
		public static ObservableCollection<Interval> Intervals {
			get {
				return intervals;
			}
		}

		//TODO: Load these from a table in geomapmaker.public
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
