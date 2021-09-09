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

namespace Geomapmaker
{
    public static class DataHelper
    {
        public static int userID;
        public static String userName;

        public static ArcGIS.Core.Data.DatabaseConnectionProperties connectionProperties;
        public static void setConnectionProperties(JObject props)
        {
            connectionProperties = new DatabaseConnectionProperties(EnterpriseDatabaseType.PostgreSQL)
            {
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
        public static string MapUnitName
        {
            get => mapUnitName;
            set
            {
                mapUnitName = value;
                MapUnitNameChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static event EventHandler MapUnitsChanged;

        private static ObservableCollection<MapUnit> mapUnits = new ObservableCollection<MapUnit>();
        public static ObservableCollection<MapUnit> MapUnits
        {
            get => mapUnits;
            set
            {
                mapUnits = value;
                MapUnitsChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static event EventHandler CFChanged;
        private static ObservableCollection<CFSymbol> cfSymbols = new ObservableCollection<CFSymbol>();
        public static ObservableCollection<CFSymbol> CFSymbols
        {
            get => cfSymbols;
            set
            {
                cfSymbols = value;
                CFChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static event EventHandler DataSourcesChanged;
        private static ObservableCollection<DataSource> dataSources = new ObservableCollection<DataSource>();
        public static ObservableCollection<DataSource> DataSources
        {
            get => dataSources;
            set
            {
                dataSources = value;
                DataSourcesChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static event EventHandler DataSourceChanged;
        private static DataSource dataSource = null;
        public static DataSource DataSource
        {
            get => dataSource;
            set
            {
                dataSource = value;
                DataSourceChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static event EventHandler DummyChanged;
        private static string dummy = "Hi there";
        public static string Dummy
        {
            get => dummy;
            set
            {
                dummy = value;
                DummyChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        //Populate our map units list from the database and create unique value renderer for mapunitpolys featureclass 
        //using colors from the descriptionofmapunits table. This uses a variation of the technique presented here: 
        //https://community.esri.com/message/960006-how-to-render-color-of-individual-features-based-on-field-in-another-table
        //and here: https://community.esri.com/thread/217968-unique-value-renderer-specifying-values
        public static async Task populateMapUnits()
        {
            Debug.WriteLine("populateMapUnits enter");

            //var mapUnits = new ObservableCollection<ComboBoxItem>();
            var mapUnits = new ObservableCollection<MapUnit>();

            if (DataHelper.connectionProperties == null)
            {
                return;
            }

            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {

                using (Geodatabase geodatabase = new Geodatabase(DataHelper.connectionProperties))
                {

                    //Get list of map units currently in feature class
                    List<string> muInFeatureClass = new List<string>();
                    QueryDef cfQDef = new QueryDef
                    {
                        Tables = "MapUnitPolys"
                    };
                    using (RowCursor rowCursor = geodatabase.Evaluate(cfQDef, false))
                    {
                        while (rowCursor.MoveNext())
                        {
                            using (Row row = rowCursor.Current)
                            {
                                // Null-check MC
                                if (row["MapUnit"] != null)
                                {
                                    muInFeatureClass.Add(row["MapUnit"].ToString());
                                }
                            }
                        }
                    }

                    //Table t = geodatabase.OpenDataset<Table>("DescriptionOfMapUnits");
                    QueryDef mapUnitsQDef = new QueryDef
                    {
                        Tables = "DescriptionOfMapUnits",
                        PostfixClause = "order by objectid"
                    };

                    using (RowCursor rowCursor = geodatabase.Evaluate(mapUnitsQDef, false))
                    {
                        List<CIMUniqueValueClass> listUniqueValueClasses = new List<CIMUniqueValueClass>();
                        while (rowCursor.MoveNext())
                        {
                            using (Row row = rowCursor.Current)
                            {
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
                                mapUnit.DescriptionSourceID = row["DescriptionSourceID"] == null ? null : row["DescriptionSourceID"].ToString();
                                mapUnit.GeoMaterial = row["GeoMaterial"] == null ? null : row["GeoMaterial"].ToString();
                                mapUnit.GeoMaterialConfidence = row["GeoMaterialConfidence"] == null ? null : row["GeoMaterialConfidence"].ToString();

                                //add it to our list
                                mapUnits.Add(mapUnit);

                                //Only add to renderer if map unit is in the feature class
                                if (muInFeatureClass.Contains(row["MapUnit"].ToString()))
                                {
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
                                    var fill = new CIMSolidFill();
                                    fill.Color = cF.CreateRGBColor(Double.Parse(strVals[0]), Double.Parse(strVals[1]), Double.Parse(strVals[2]));
                                    var stroke = new CIMSolidStroke();
                                    stroke.Color = cF.CreateRGBColor(255, 255, 255, 0);
                                    CIMUniqueValueClass uniqueValueClass = new CIMUniqueValueClass
                                    {
                                        Editable = true,
                                        Label = mapUnit.MU,
                                        //Patch = PatchShape.Default,
                                        Patch = PatchShape.AreaPolygon,
                                        Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(fill, stroke).MakeSymbolReference(),
                                        Visible = true,
                                        Values = listUniqueValues.ToArray()
                                    };
                                    listUniqueValueClasses.Add(uniqueValueClass);
                                }

                            }
                        }

                        //Create a list of CIMUniqueValueGroup
                        CIMUniqueValueGroup uvg = new CIMUniqueValueGroup
                        {
                            Classes = listUniqueValueClasses.ToArray(),
                        };
                        List<CIMUniqueValueGroup> listUniqueValueGroups = new List<CIMUniqueValueGroup> { uvg };

                        //Use the list to create the CIMUniqueValueRenderer
                        DataHelper.mapUnitRenderer = new CIMUniqueValueRenderer
                        {
                            UseDefaultSymbol = false,
                            //DefaultLabel = "all other values",
                            //DefaultSymbolPatch = PatchShape.AreaPolygon,
                            //DefaultSymbol = SymbolFactory.Instance.ConstructPolygonSymbol(ColorFactory.Instance.GreyRGB).MakeSymbolReference(),
                            Groups = listUniqueValueGroups.ToArray(),
                            Fields = new string[] { "mapunit" }
                        };

                        //Set renderer in mapunit. The try/catch is there because the first time through, this is called 
                        //before the layer has been added to the map. We just ignore the error in that case.
                        try
                        {
                            var muLayer = MapView.Active.Map.GetLayersAsFlattenedList().First((l) => l.Name == "MapUnitPolys") as FeatureLayer;
                            muLayer.SetRenderer(DataHelper.mapUnitRenderer);
                        }
                        catch { }

                    }

                }
            });
            DataHelper.MapUnits = mapUnits;
        }


        public static async Task populateContactsAndFaults()
        {
            Debug.WriteLine("populateContactsAndFaults enter");

            var cfSymbols = new ObservableCollection<CFSymbol>();

            if (DataHelper.connectionProperties == null)
            {
                return;
            }

            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {

                using (Geodatabase geodatabase = new Geodatabase(DataHelper.connectionProperties))
                {

                    List<string> cfInFeatureClass = new List<string>();
                    QueryDef cfQDef = new QueryDef
                    {
                        Tables = "ContactsAndFaults"
                    };
                    using (RowCursor rowCursor = geodatabase.Evaluate(cfQDef, false))
                    {
                        while (rowCursor.MoveNext())
                        {
                            using (Row row = rowCursor.Current)
                            {
                                //cfInFeatureClass.Add(row["symbol"] + "." + row["label"]);
                                cfInFeatureClass.Add(row["symbol"].ToString());
                            }
                        }
                    }

                    QueryDef cfSymbolQDef = new QueryDef
                    {
                        Tables = "CFSymbology",
                        SubFields = "key,description,symbol", //TODO: I'm not sure why I have to explicitly list these to get description, but it seems I do
                        PostfixClause = "order by key"
                    };

                    using (RowCursor rowCursor = geodatabase.Evaluate(cfSymbolQDef, false))
                    {
                        List<CIMUniqueValueClass> listUniqueValueClasses = new List<CIMUniqueValueClass>();
                        while (rowCursor.MoveNext())
                        {
                            using (Row row = rowCursor.Current)
                            {
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
                                SymbolStyleItem sSI = new SymbolStyleItem()
                                {
                                    Symbol = CIMSymbolReference.FromJson(cfS.symbol).Symbol,
                                    PatchWidth = 50,
                                    PatchHeight = 25
                                };
                                cfS.preview = sSI.PreviewImage;

                                //add it to our list
                                cfSymbols.Add(cfS);

                                //Only add to renderer if present in the feature class
                                if (cfInFeatureClass.Contains(cfS.key))
                                {
                                    //Create a "CIMUniqueValueClass" for the cf and add it to the list of unique values.
                                    //This is what creates the mapping from cf derived attribute to symbol
                                    List<CIMUniqueValue> listUniqueValues = new List<CIMUniqueValue> {
                                        new CIMUniqueValue {
                                            FieldValues = new string[] { cfS.key }
                                        }
                                    };

                                    CIMUniqueValueClass uniqueValueClass = new CIMUniqueValueClass
                                    {
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
                        CIMUniqueValueGroup uvg = new CIMUniqueValueGroup
                        {
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
                        DataHelper.cfRenderer = new CIMUniqueValueRenderer
                        {
                            UseDefaultSymbol = false,
                            Groups = listUniqueValueGroups.ToArray(),
                            Fields = new string[] { "symbol" }
                            //ValueExpressionInfo = cEI //fields used for testing
                        };

                        //Set renderer in cf. The try/catch is there because the first time through, this is called 
                        //before the layer has been added to the map. We just ignore the error in that case.
                        try
                        {
                            var cfLayer = MapView.Active.Map.GetLayersAsFlattenedList().First((l) => l.Name == "ContactsAndFaults") as FeatureLayer;
                            cfLayer.SetRenderer(DataHelper.cfRenderer);
                        }
                        catch { }

                    }

                }
            });
            DataHelper.CFSymbols = cfSymbols;
        }



        public static async Task populateDataSources()
        {
            Debug.WriteLine("populateDataSources enter");

            var dataSources = new ObservableCollection<DataSource>();

            if (DataHelper.connectionProperties == null)
            {
                return;
            }

            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {

                using (Geodatabase geodatabase = new Geodatabase(DataHelper.connectionProperties))
                {

                    QueryDef dsQDef = new QueryDef
                    {
                        Tables = "DataSources",
                        PostfixClause = "order by source"
                    };

                    using (RowCursor rowCursor = geodatabase.Evaluate(dsQDef, false))
                    {
                        while (rowCursor.MoveNext())
                        {
                            using (Row row = rowCursor.Current)
                            {
                                Debug.WriteLine(row["source"].ToString());

                                //create and load map unit
                                DataSource dS = new DataSource();
                                dS.ID = long.Parse(row["objectid"].ToString());
                                dS.Source = row["source"].ToString();
                                dS.DataSource_ID = row["datasources_id"].ToString();
                                dS.Url = row["url"] == null ? "" : row["url"].ToString();
                                dS.Notes = row["notes"] == null ? "" : row["notes"].ToString();

                                //add it to our list
                                dataSources.Add(dS);
                            }
                        }

                    }

                }
            });
            DataHelper.DataSources = dataSources;
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
        public static void UserLogin(int uID, String uName)
        {
            userID = uID;
            userName = uName;
            UserLoginHandler?.Invoke();
        }

        public delegate void ProjectSelectedDelegate();
        public static event ProjectSelectedDelegate ProjectSelectedHandler;
        public static void ProjectSelected()
        {
            ProjectSelectedHandler?.Invoke();
        }


        //These intervals and their ranges are, with a little post-processing, from:
        //	https://macrostrat.org/api/v1/defs/intervals?timescale_id=11
        //	https://macrostrat.org/api/v1/defs/intervals?timescale_id=17
        //TODO: Might be nice to load this from a table in geomapmaker.public
        private static ObservableCollection<Interval> intervals = new ObservableCollection<Interval>() {
            new Interval {name ="Cenozoic", range="66 - 0ma"},
            new Interval {name ="Holocene", range="0.0117 - 0ma"},
            new Interval {name ="Quaternary", range="2.588 - 0ma"},
            new Interval {name ="Pleistocene", range="2.588 - 0.0117ma"},
            new Interval {name ="Late Pleistocene", range="0.126 - 0.0117ma"},
            new Interval {name ="Middle Pleistocene", range="0.781 - 0.126ma"},
            new Interval {name ="Calabrian", range="1.806 - 0.781ma"},
            new Interval {name ="Gelasian", range="2.588 - 1.806ma"},
            new Interval {name ="Neogene", range="23.03 - 2.588ma"},
            new Interval {name ="Pliocene", range="5.333 - 2.588ma"},
            new Interval {name ="Piacenzian", range="3.6 - 2.588ma"},
            new Interval {name ="Zanclean", range="5.333 - 3.6ma"},
            new Interval {name ="Miocene", range="23.03 - 5.333ma"},
            new Interval {name ="Messinian", range="7.246 - 5.333ma"},
            new Interval {name ="Tortonian", range="11.62 - 7.246ma"},
            new Interval {name ="Serravallian", range="13.82 - 11.62ma"},
            new Interval {name ="Langhian", range="15.97 - 13.82ma"},
            new Interval {name ="Burdigalian", range="20.44 - 15.97ma"},
            new Interval {name ="Aquitanian", range="23.03 - 20.44ma"},
            new Interval {name ="Paleogene", range="66 - 23.03ma"},
            new Interval {name ="Oligocene", range="33.9 - 23.03ma"},
            new Interval {name ="Chattian", range="28.1 - 23.03ma"},
            new Interval {name ="Rupelian", range="33.9 - 28.1ma"},
            new Interval {name ="Eocene", range="56 - 33.9ma"},
            new Interval {name ="Priabonian", range="37.8 - 33.9ma"},
            new Interval {name ="Bartonian", range="41.3 - 37.8ma"},
            new Interval {name ="Lutetian", range="47.8 - 41.3ma"},
            new Interval {name ="Ypresian", range="56 - 47.8ma"},
            new Interval {name ="Paleocene", range="66 - 56ma"},
            new Interval {name ="Thanetian", range="59.2 - 56ma"},
            new Interval {name ="Selandian", range="61.6 - 59.2ma"},
            new Interval {name ="Danian", range="66 - 61.6ma"},
            new Interval {name ="Mesozoic", range="251.902 - 66ma"},
            new Interval {name ="Cretaceous", range="145 - 66ma"},
            new Interval {name ="Late Cretaceous", range="100.5 - 66ma"},
            new Interval {name ="Maastrichtian", range="72.1 - 66ma"},
            new Interval {name ="Campanian", range="83.6 - 72.1ma"},
            new Interval {name ="Santonian", range="86.3 - 83.6ma"},
            new Interval {name ="Coniacian", range="89.8 - 86.3ma"},
            new Interval {name ="Turonian", range="93.9 - 89.8ma"},
            new Interval {name ="Cenomanian", range="100.5 - 93.9ma"},
            new Interval {name ="Early Cretaceous", range="145 - 100.5ma"},
            new Interval {name ="Albian", range="113 - 100.5ma"},
            new Interval {name ="Aptian", range="125 - 113ma"},
            new Interval {name ="Barremian", range="129.4 - 125ma"},
            new Interval {name ="Hauterivian", range="132.9 - 129.4ma"},
            new Interval {name ="Valanginian", range="139.8 - 132.9ma"},
            new Interval {name ="Berriasian", range="145 - 139.8ma"},
            new Interval {name ="Jurassic", range="201.3 - 145ma"},
            new Interval {name ="Late Jurassic", range="163.5 - 145ma"},
            new Interval {name ="Tithonian", range="152.1 - 145ma"},
            new Interval {name ="Kimmeridgian", range="157.3 - 152.1ma"},
            new Interval {name ="Oxfordian", range="163.5 - 157.3ma"},
            new Interval {name ="Middle Jurassic", range="174.1 - 163.5ma"},
            new Interval {name ="Callovian", range="166.1 - 163.5ma"},
            new Interval {name ="Bathonian", range="168.3 - 166.1ma"},
            new Interval {name ="Bajocian", range="170.3 - 168.3ma"},
            new Interval {name ="Aalenian", range="174.1 - 170.3ma"},
            new Interval {name ="Early Jurassic", range="201.3 - 174.1ma"},
            new Interval {name ="Toarcian", range="182.7 - 174.1ma"},
            new Interval {name ="Pliensbachian", range="190.8 - 182.7ma"},
            new Interval {name ="Sinemurian", range="199.3 - 190.8ma"},
            new Interval {name ="Hettangian", range="201.3 - 199.3ma"},
            new Interval {name ="Triassic", range="251.902 - 201.3ma"},
            new Interval {name ="Late Triassic", range="237 - 201.3ma"},
            new Interval {name ="Rhaetian", range="208.5 - 201.3ma"},
            new Interval {name ="Norian", range="227 - 208.5ma"},
            new Interval {name ="Carnian", range="237 - 227ma"},
            new Interval {name ="Middle Triassic", range="247.2 - 237ma"},
            new Interval {name ="Ladinian", range="242 - 237ma"},
            new Interval {name ="Anisian", range="247.2 - 242ma"},
            new Interval {name ="Olenekian", range="251.2 - 247.2ma"},
            new Interval {name ="Early Triassic", range="251.902 - 247.2ma"},
            new Interval {name ="Induan", range="251.902 - 251.2ma"},
            new Interval {name ="Paleozoic", range="541 - 251.902ma"},
            new Interval {name ="Permian", range="298.9 - 251.902ma"},
            new Interval {name ="Changhsingian", range="254.14 - 251.902ma"},
            new Interval {name ="Lopingian", range="259.1 - 251.902ma"},
            new Interval {name ="Wuchiapingian", range="259.1 - 254.14ma"},
            new Interval {name ="Guadalupian", range="272.95 - 259.1ma"},
            new Interval {name ="Capitanian", range="265.1 - 259.1ma"},
            new Interval {name ="Wordian", range="268.8 - 265.1ma"},
            new Interval {name ="Roadian", range="272.95 - 268.8ma"},
            new Interval {name ="Kungurian", range="283.5 - 272.95ma"},
            new Interval {name ="Cisuralian", range="298.9 - 272.95ma"},
            new Interval {name ="Artinskian", range="290.1 - 283.5ma"},
            new Interval {name ="Sakmarian", range="295 - 290.1ma"},
            new Interval {name ="Asselian", range="298.9 - 295ma"},
            new Interval {name ="Carboniferous", range="358.9 - 298.9ma"},
            new Interval {name ="Pennsylvanian", range="323.2 - 298.9ma"},
            new Interval {name ="Gzhelian", range="303.7 - 298.9ma"},
            new Interval {name ="Kasimovian", range="307 - 303.7ma"},
            new Interval {name ="Moscovian", range="315.2 - 307ma"},
            new Interval {name ="Bashkirian", range="323.2 - 315.2ma"},
            new Interval {name ="Mississippian", range="358.9 - 323.2ma"},
            new Interval {name ="Serpukhovian", range="330.9 - 323.2ma"},
            new Interval {name ="Visean", range="346.7 - 330.9ma"},
            new Interval {name ="Tournaisian", range="358.9 - 346.7ma"},
            new Interval {name ="Devonian", range="419.2 - 358.9ma"},
            new Interval {name ="Late Devonian", range="382.7 - 358.9ma"},
            new Interval {name ="Famennian", range="372.2 - 358.9ma"},
            new Interval {name ="Frasnian", range="382.7 - 372.2ma"},
            new Interval {name ="Middle Devonian", range="393.3 - 382.7ma"},
            new Interval {name ="Givetian", range="387.7 - 382.7ma"},
            new Interval {name ="Eifelian", range="393.3 - 387.7ma"},
            new Interval {name ="Early Devonian", range="419.2 - 393.3ma"},
            new Interval {name ="Emsian", range="407.6 - 393.3ma"},
            new Interval {name ="Pragian", range="410.8 - 407.6ma"},
            new Interval {name ="Lochkovian", range="419.2 - 410.8ma"},
            new Interval {name ="Silurian", range="443.8 - 419.2ma"},
            new Interval {name ="Pridoli", range="423 - 419.2ma"},
            new Interval {name ="Ludlow", range="427.4 - 423ma"},
            new Interval {name ="Ludfordian", range="425.6 - 423ma"},
            new Interval {name ="Gorstian", range="427.4 - 425.6ma"},
            new Interval {name ="Wenlock", range="433.4 - 427.4ma"},
            new Interval {name ="Homerian", range="430.5 - 427.4ma"},
            new Interval {name ="Sheinwoodian", range="433.4 - 430.5ma"},
            new Interval {name ="Llandovery", range="443.8 - 433.4ma"},
            new Interval {name ="Telychian", range="438.5 - 433.4ma"},
            new Interval {name ="Aeronian", range="440.8 - 438.5ma"},
            new Interval {name ="Rhuddanian", range="443.8 - 440.8ma"},
            new Interval {name ="Ordovician", range="485.4 - 443.8ma"},
            new Interval {name ="Late Ordovician", range="458.4 - 443.8ma"},
            new Interval {name ="Hirnantian", range="445.2 - 443.8ma"},
            new Interval {name ="Katian", range="453 - 445.2ma"},
            new Interval {name ="Sandbian", range="458.4 - 453ma"},
            new Interval {name ="Middle Ordovician", range="470 - 458.4ma"},
            new Interval {name ="Darriwilian", range="467.3 - 458.4ma"},
            new Interval {name ="Dapingian", range="470 - 467.3ma"},
            new Interval {name ="Early Ordovician", range="485.4 - 470ma"},
            new Interval {name ="Floian", range="477.7 - 470ma"},
            new Interval {name ="Tremadocian", range="485.4 - 477.7ma"},
            new Interval {name ="Cambrian", range="541 - 485.4ma"},
            new Interval {name ="Furongian", range="497 - 485.4ma"},
            new Interval {name ="Stage 10", range="489.5 - 485.4ma"},
            new Interval {name ="Jiangshanian", range="494 - 489.5ma"},
            new Interval {name ="Paibian", range="497 - 494ma"},
            new Interval {name ="Guzhangian", range="500.5 - 497ma"},
            new Interval {name ="Miaolingian", range="509 - 497ma"},
            new Interval {name ="Drumian", range="504.5 - 500.5ma"},
            new Interval {name ="Wuliuan", range="509 - 504.5ma"},
            new Interval {name ="Stage 4", range="514 - 509ma"},
            new Interval {name ="Series 2", range="521 - 509ma"},
            new Interval {name ="Stage 3", range="521 - 514ma"},
            new Interval {name ="Terreneuvian", range="541 - 521ma"},
            new Interval {name ="Stage 2", range="529 - 521ma"},
            new Interval {name ="Fortunian", range="541 - 529ma"},
            new Interval {name ="Neoproterozoic", range="1000 - 541ma"},
            new Interval {name ="Ediacaran", range="635 - 541ma"},
            new Interval {name ="Cryogenian", range="720 - 635ma"},
            new Interval {name ="Tonian", range="1000 - 720ma"},
            new Interval {name ="Mesoproterozoic", range="1600 - 1000ma"},
            new Interval {name ="Stenian", range="1200 - 1000ma"},
            new Interval {name ="Ectasian", range="1400 - 1200ma"},
            new Interval {name ="Calymmian", range="1600 - 1400ma"},
            new Interval {name ="Paleoproterozoic", range="2500 - 1600ma"},
            new Interval {name ="Statherian", range="1800 - 1600ma"},
            new Interval {name ="Orosirian", range="2050 - 1800ma"},
            new Interval {name ="Rhyacian", range="2300 - 2050ma"},
            new Interval {name ="Siderian", range="2500 - 2300ma"},
            new Interval {name ="Neoarchean", range="2800 - 2500ma"},
            new Interval {name ="Mesoarchean", range="3200 - 2800ma"},
            new Interval {name ="Paleoarchean", range="3600 - 3200ma"},
            new Interval {name ="Eoarchean", range="4000 - 3600ma"},
            new Interval {name ="Rancholabrean", range="0.3 - 0.0114ma"},
            new Interval {name ="Irvingtonian", range="1.806 - 0.3ma"},
            new Interval {name ="Blancan", range="4.9 - 1.806ma"},
            new Interval {name ="Hemphillian", range="10.3 - 4.9ma"},
            new Interval {name ="Clarendonian", range="13.6 - 10.3ma"},
            new Interval {name ="Barstovian", range="16.3 - 13.6ma"},
            new Interval {name ="Hemingfordian", range="18.6 - 16.3ma"},
            new Interval {name ="Arikareean", range="29.8 - 18.6ma"},
            new Interval {name ="Whitneyan", range="32.1 - 29.8ma"},
            new Interval {name ="Orellan", range="33.9 - 32.1ma"},
            new Interval {name ="Chadronian", range="37.8 - 33.9ma"},
            new Interval {name ="Duchesnean", range="39.9 - 37.8ma"},
            new Interval {name ="Uintan", range="47.9 - 39.9ma"},
            new Interval {name ="Bridgerian", range="52 - 47.9ma"},
            new Interval {name ="Wasatchian", range="56 - 52ma"},
            new Interval {name ="Clarkforkian", range="57.5 - 56ma"},
            new Interval {name ="Tiffanian", range="62.25 - 57.5ma"},
            new Interval {name ="Torrejonian", range="64.75 - 62.25ma"},
            new Interval {name ="Puercan", range="66 - 64.75ma"},
            new Interval {name ="Maastrichtian", range="72.1 - 66ma"},
            new Interval {name ="Campanian", range="83.6 - 72.1ma"},
            new Interval {name ="Santonian", range="86.3 - 83.6ma"},
            new Interval {name ="Coniacian", range="89.8 - 86.3ma"},
            new Interval {name ="Turonian", range="93.9 - 89.8ma"},
            new Interval {name ="Cenomanian", range="100.5 - 93.9ma"},
            new Interval {name ="Albian", range="113 - 100.5ma"},
            new Interval {name ="Aptian", range="125 - 113ma"},
            new Interval {name ="Barremian", range="129.4 - 125ma"},
            new Interval {name ="Hauterivian", range="132.9 - 129.4ma"},
            new Interval {name ="Valanginian", range="139.8 - 132.9ma"},
            new Interval {name ="Berriasian", range="145 - 139.8ma"},
            new Interval {name ="Tithonian", range="152.1 - 145ma"},
            new Interval {name ="Kimmeridgian", range="157.3 - 152.1ma"},
            new Interval {name ="Oxfordian", range="163.5 - 157.3ma"},
            new Interval {name ="Callovian", range="166.1 - 163.5ma"},
            new Interval {name ="Bathonian", range="168.3 - 166.1ma"},
            new Interval {name ="Bajocian", range="170.3 - 168.3ma"},
            new Interval {name ="Aalenian", range="174.1 - 170.3ma"},
            new Interval {name ="Toarcian", range="182.7 - 174.1ma"},
            new Interval {name ="Pliensbachian", range="190.8 - 182.7ma"},
            new Interval {name ="Sinemurian", range="199.3 - 190.8ma"},
            new Interval {name ="Hettangian", range="201.3 - 199.3ma"},
            new Interval {name ="Rhaetian", range="208.5 - 201.3ma"},
            new Interval {name ="Sevatian", range="214 - 208.5ma"},
            new Interval {name ="Alaunian", range="216 - 214ma"},
            new Interval {name ="Lacian", range="227 - 216ma"},
            new Interval {name ="Tuvalian", range="230.5 - 227ma"},
            new Interval {name ="Julian", range="237 - 230.5ma"},
            new Interval {name ="Ladinian", range="242 - 237ma"},
            new Interval {name ="Anisian", range="247.2 - 242ma"},
            new Interval {name ="Spathian", range="248.5 - 247.2ma"},
            new Interval {name ="Smithian", range="251.2 - 248.5ma"},
            new Interval {name ="Dienerian", range="251.6 - 251.2ma"},
            new Interval {name ="Griesbachian", range="251.902 - 251.6ma"},
            new Interval {name ="Ochoan", range="259.1 - 259ma"},
            new Interval {name ="Capitanian", range="265.1 - 259.1ma"},
            new Interval {name ="Wordian", range="268.8 - 265.1ma"},
            new Interval {name ="Roadian", range="272.95 - 268.8ma"},
            new Interval {name ="Leonardian", range="282 - 272.95ma"},
            new Interval {name ="Wolfcampian", range="298.9 - 282ma"},
            new Interval {name ="Virgilian", range="303.7 - 298.9ma"},
            new Interval {name ="Missourian", range="306 - 303.7ma"},
            new Interval {name ="Desmoinesian", range="312 - 306ma"},
            new Interval {name ="Atokan", range="319 - 312ma"},
            new Interval {name ="Morrowan", range="323.2 - 319ma"},
            new Interval {name ="Chesterian", range="335 - 323.2ma"},
            new Interval {name ="Meramecian", range="343.5 - 335ma"},
            new Interval {name ="Osagean", range="351.5 - 343.5ma"},
            new Interval {name ="Kinderhookian", range="358.9 - 351.5ma"},
            new Interval {name ="Chatauquan", range="370 - 358.9ma"},
            new Interval {name ="Senecan", range="385 - 370ma"},
            new Interval {name ="Erian", range="393.3 - 385ma"},
            new Interval {name ="Ulsterian", range="419.2 - 393.3ma"},
            new Interval {name ="Cayugan", range="425.6 - 419.2ma"},
            new Interval {name ="Niagaran", range="438.5 - 425.6ma"},
            new Interval {name ="Alexandrian", range="443.8 - 438.5ma"},
            new Interval {name ="Gamachian", range="445.2 - 443.8ma"},
            new Interval {name ="Richmondian", range="449 - 445.2ma"},
            new Interval {name ="Maysvillian", range="450 - 449ma"},
            new Interval {name ="Edenian", range="451 - 450ma"},
            new Interval {name ="Mohawkian", range="456.5 - 451ma"},
            new Interval {name ="Whiterock", range="470 - 456.5ma"},
            new Interval {name ="Blackhillsian", range="475 - 470ma"},
            new Interval {name ="Tulean", range="479.5 - 475ma"},
            new Interval {name ="Stairsian", range="480 - 479.5ma"},
            new Interval {name ="Skullrockian", range="486.7 - 480ma"},
            new Interval {name ="Sunwaptan", range="493 - 486.7ma"},
            new Interval {name ="Steptoean", range="497 - 493ma"},
            new Interval {name ="Marjuman", range="504.5 - 497ma"},
            new Interval {name ="Topazan", range="506.5 - 504.5ma"},
            new Interval {name ="Delamaran", range="511 - 506.5ma"},
            new Interval {name ="Dyeran", range="515.5 - 511ma"},
            new Interval {name ="Montezuman", range="520 - 515.5ma"},
            new Interval {name ="Begadean", range="541 - 520ma"},
            new Interval {name ="Hadrynian", range="850 - 541ma"}
        };
        public static ObservableCollection<Interval> Intervals
        {
            get
            {
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
        public static ObservableCollection<string> GeoMaterials
        {
            get
            {
                return geoMaterials;
            }
        }

        private static ObservableCollection<string> geoMaterialConfidences = new ObservableCollection<string>() {
            "High",
            "Medium",
            "Low"
        };
        public static ObservableCollection<string> GeoMaterialConfidences
        {
            get
            {
                return geoMaterialConfidences;
            }
        }

        private static ObservableCollection<string> identityConfidences = new ObservableCollection<string>() {
            "High",
            "Medium",
            "Low"
        };
        public static ObservableCollection<string> IdentityConfidences
        {
            get
            {
                return identityConfidences;
            }
        }

        private static ObservableCollection<string> existenceConfidences = new ObservableCollection<string>() {
            "High",
            "Medium",
            "Low"
        };
        public static ObservableCollection<string> ExistenceConfidences
        {
            get
            {
                return existenceConfidences;
            }
        }

        private static ObservableCollection<KeyValuePair<int, string>> locationConfidenceMeters = new ObservableCollection<KeyValuePair<int, string>>() {

            new KeyValuePair<int, string>(-9, "Not Known"),
            new KeyValuePair<int, string>(10, "10m"),
            new KeyValuePair<int, string>(25, "25m"),
            new KeyValuePair<int, string>(50, "50m"),
            new KeyValuePair<int, string>(100, "100m"),
            new KeyValuePair<int, string>(250, "250m"),
        };

        public static ObservableCollection<KeyValuePair<int, string>> LocationConfidenceMeters
        {
            get
            {
                return locationConfidenceMeters;
            }
        }
    }
}
