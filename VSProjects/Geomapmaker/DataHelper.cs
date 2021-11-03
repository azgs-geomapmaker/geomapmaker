using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Data;
using Geomapmaker.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Geomapmaker
{
    public static class DataHelper
    {
        public static int userID;
        public static string userName;

        public static DatabaseConnectionProperties connectionProperties;
        public static void SetConnectionProperties(DatabaseConnectionProperties props)
        {
            connectionProperties = props;

            // My plan is to slowly break this class up into smaller classes in the Data folder. 
            // I'm setting the connection properties twice while I migrate things -camp
            Data.DbConnectionProperties.SetProperties(props);
        }

        public static List<FeatureLayer> currentLayers = new List<FeatureLayer>();
        public static List<StandaloneTable> currentTables = new List<StandaloneTable>();

        public static CIMUniqueValueRenderer mapUnitRenderer;
        public static CIMUniqueValueRenderer cfRenderer;

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

        // Populate our map units list from the database and create unique value renderer for mapunitpolys featureclass 
        // using colors from the descriptionofmapunits table. This uses a variation of the technique presented here: 
        // https://community.esri.com/message/960006-how-to-render-color-of-individual-features-based-on-field-in-another-table
        // and here: https://community.esri.com/thread/217968-unique-value-renderer-specifying-values
        public static async Task PopulateMapUnits()
        {
            ObservableCollection<MapUnit> mapUnits = new ObservableCollection<MapUnit>();

            if (connectionProperties == null)
            {
                return;
            }

            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {

                using (Geodatabase geodatabase = new Geodatabase(connectionProperties))
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

                    QueryDef mapUnitsQDef = new QueryDef
                    {
                        WhereClause = "ParagraphStyle='Standard'",
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
                                //Debug.WriteLine(row["Name"].ToString());

                                // Create and load map unit
                                MapUnit mapUnit = new MapUnit
                                {
                                    ID = int.Parse(row["ObjectID"].ToString()),
                                    MU = row["MapUnit"]?.ToString(),
                                    Name = row["Name"].ToString(),
                                    FullName = row["FullName"]?.ToString(),
                                    Age = row["Age"]?.ToString(), //TODO: more formatting here
                                    //mapUnit.RelativeAge = row["RelativeAge"].ToString(); //TODO: this is column missing in the table right now
                                    Description = row["Description"]?.ToString(),
                                    HierarchyKey = row["HierarchyKey"]?.ToString(),
                                    ParagraphStyle = row["ParagraphStyle"]?.ToString(),
                                    //mapUnit.ParagraphStyle = JsonConvert.DeserializeObject<List<string>>(row["ParagraphStyle"].ToString());
                                    //mapUnit.Label = row["Label"].ToString();
                                    //mapUnit.Symbol = row["Symbol"].ToString();
                                    //mapUnit.AreaFillRGB = row["AreaFillRGB"].ToString(); //TODO: more formatting here
                                    HexColor = row["hexcolor"]?.ToString(),
                                    //mapUnit.Color = row["hexcolor"];
                                    DescriptionSourceID = row["DescriptionSourceID"]?.ToString(),
                                    GeoMaterial = row["GeoMaterial"]?.ToString(),
                                    GeoMaterialConfidence = row["GeoMaterialConfidence"]?.ToString(),
                                };

                                // Add it to our list
                                mapUnits.Add(mapUnit);

                                // Only add to renderer if map unit is in the feature class
                                if (row["MapUnit"] != null && muInFeatureClass.Contains(row["MapUnit"].ToString()))
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
                                    IColorFactory cF = ColorFactory.Instance;
                                    CIMSolidFill fill = new CIMSolidFill
                                    {
                                        Color = cF.CreateRGBColor(double.Parse(strVals[0]), double.Parse(strVals[1]), double.Parse(strVals[2]))
                                    };
                                    CIMSolidStroke stroke = new CIMSolidStroke
                                    {
                                        Color = cF.CreateRGBColor(255, 255, 255, 0)
                                    };
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

                        // Create a list of CIMUniqueValueGroup
                        CIMUniqueValueGroup uvg = new CIMUniqueValueGroup
                        {
                            Classes = listUniqueValueClasses.ToArray(),
                        };
                        List<CIMUniqueValueGroup> listUniqueValueGroups = new List<CIMUniqueValueGroup> { uvg };

                        // Use the list to create the CIMUniqueValueRenderer
                        mapUnitRenderer = new CIMUniqueValueRenderer
                        {
                            UseDefaultSymbol = false,
                            //DefaultLabel = "all other values",
                            //DefaultSymbolPatch = PatchShape.AreaPolygon,
                            //DefaultSymbol = SymbolFactory.Instance.ConstructPolygonSymbol(ColorFactory.Instance.GreyRGB).MakeSymbolReference(),
                            Groups = listUniqueValueGroups.ToArray(),
                            Fields = new string[] { "mapunit" }
                        };

                        // Set renderer in mapunit. The try/catch is there because the first time through, this is called 
                        // before the layer has been added to the map. We just ignore the error in that case.
                        try
                        {
                            FeatureLayer muLayer = MapView.Active.Map.GetLayersAsFlattenedList().First((l) => l.Name == "MapUnitPolys") as FeatureLayer;
                            muLayer.SetRenderer(mapUnitRenderer);
                        }
                        catch { }

                    }

                }
            });

            MapUnits = mapUnits;

            // Temp fix while migrating to new data class
            await DescriptionOfMapUnits.RefreshMapUnitsAsync();
        }

        public static async Task PopulateContactsAndFaults()
        {
            Debug.WriteLine("populateContactsAndFaults enter");

            ObservableCollection<CFSymbol> cfSymbols = new ObservableCollection<CFSymbol>();

            if (connectionProperties == null)
            {
                return;
            }

            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {

                using (Geodatabase geodatabase = new Geodatabase(connectionProperties))
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

                                // Create and load map unit
                                CFSymbol cfS = new CFSymbol
                                {
                                    key = row["key"].ToString(),
                                    description = row["description"] == null ? "" : row["description"].ToString(),
                                    symbol = row["symbol"].ToString()
                                };

                                // Wrap the symbol JSON in CIMSymbolReference, so we can use that class to deserialize it.
                                cfS.symbol = cfS.symbol.Insert(0, "{\"type\": \"CIMSymbolReference\", \"symbol\": ");
                                cfS.symbol = cfS.symbol.Insert(cfS.symbol.Length, "}");

                                // Create the preview image used in the ComboBox
                                SymbolStyleItem sSI = new SymbolStyleItem()
                                {
                                    Symbol = CIMSymbolReference.FromJson(cfS.symbol).Symbol,
                                    PatchWidth = 50,
                                    PatchHeight = 25
                                };
                                cfS.preview = sSI.PreviewImage;

                                // Add it to our list
                                cfSymbols.Add(cfS);

                                // Only add to renderer if present in the feature class
                                if (cfInFeatureClass.Contains(cfS.key))
                                {
                                    // Create a "CIMUniqueValueClass" for the cf and add it to the list of unique values.
                                    // This is what creates the mapping from cf derived attribute to symbol
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
                        cfRenderer = new CIMUniqueValueRenderer
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
                            FeatureLayer cfLayer = MapView.Active.Map.GetLayersAsFlattenedList().First((l) => l.Name == "ContactsAndFaults") as FeatureLayer;
                            cfLayer.SetRenderer(cfRenderer);
                        }
                        catch { }

                    }

                }
            });

            CFSymbols = cfSymbols;
        }

        public static async Task PopulateDataSources()
        {
            Debug.WriteLine("populateDataSources enter");

            ObservableCollection<DataSource> dataSources = new ObservableCollection<DataSource>();

            if (connectionProperties == null)
            {
                return;
            }

            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {

                using (Geodatabase geodatabase = new Geodatabase(connectionProperties))
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
                                DataSource dS = new DataSource
                                {
                                    ID = long.Parse(row["objectid"].ToString()),
                                    Source = row["source"].ToString(),
                                    DataSource_ID = row["datasources_id"].ToString(),
                                    Url = row["url"] == null ? "" : row["url"].ToString(),
                                    Notes = row["notes"] == null ? "" : row["notes"].ToString()
                                };

                                //add it to our list
                                dataSources.Add(dS);
                            }
                        }

                    }

                }
            });

            DataSources = dataSources;
        }

        public delegate void UserLoginDelegate();
        public static event UserLoginDelegate UserLoginHandler;
        public static void UserLogin(int uID, string uName)
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

        public static ObservableCollection<string> GeoMaterials { get; } = new ObservableCollection<string>() {
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

        public static ObservableCollection<string> GeoMaterialConfidences { get; } = new ObservableCollection<string>() {
            "High",
            "Medium",
            "Low"
        };

        public static ObservableCollection<string> IdentityConfidences { get; } = new ObservableCollection<string>() {
            "High",
            "Medium",
            "Low"
        };

        public static ObservableCollection<string> ExistenceConfidences { get; } = new ObservableCollection<string>() {
            "High",
            "Medium",
            "Low"
        };

        public static ObservableCollection<string> LocationConfidenceMeters { get; } = new ObservableCollection<string>() {
            "10",
            "25",
            "50",
            "100",
            "250",
        };
    }

    /// <summary>
    /// Value converter for radio button groups
    /// </summary>
    public class RadioConfidenceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Equals(value, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }
    }

    /// <summary>
    /// TODO THIS NEEDS TO BE MOVED INTO VIEWMODEL Value converter for slider
    /// </summary>
    public class SliderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case "Low":
                    return 0;
                case "Medium":
                    return 1;
                case "High":
                    return 2;
                default:
                    return -1;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case 0.0:
                    return "Low";
                case 1.0:
                    return "Medium";
                case 2.0:
                    return "High";
                default:
                    return "N/A";
            }
        }
    }

    /// <summary>
    /// TODO THIS NEEDS TO BE MOVED INTO VIEWMODEL
    /// </summary>
    public class ConcealedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "Y" : "N";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
