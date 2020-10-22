using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
using Newtonsoft.Json.Linq;
using Npgsql;

namespace Geomapmaker {
    /// <summary>
    /// Represents the ComboBox
    /// </summary>
    internal class ProjectsComboBox : ComboBox {
        NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1;User Id=geomapmaker; " +
                   "Password=password;Database=geomapmaker;");


        private bool _isInitialized;

        /// <summary>
        /// Combo Box constructor
        /// </summary>
        public ProjectsComboBox() {
            //UpdateCombo();
            DataHelper.UserLoginHandler += onUserLogin;
        }

        void onUserLogin() {
            _isInitialized = false;
            UpdateCombo();
        }

        /// <summary>
        /// Updates the combo box with all the items.
        /// </summary>

        private void UpdateCombo() {

            if (_isInitialized)
                SelectedItem = null; // ItemCollection.FirstOrDefault(); //set the default item in the comboBox

            if (!_isInitialized) {
                Clear();
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM geomapmaker.projects where id in (select project_id from user_project_links where user_id = " + DataHelper.userID + ") order by name asc", conn);
                NpgsqlDataReader dr = command.ExecuteReader();

                DataTable dT = new DataTable();
                dT.Load(dr);

                //Add(new ComboBoxItem("<choose>")); //TODO: This is only here because I'm not sure how to have a combobox without an initial selection
                foreach (DataRow row in dT.Rows) {
                    //Debug.Write("Hi there \n");
                    //Debug.Write("{0} \n", row["name"].ToString());
                    Debug.WriteLine(row["name"].ToString());
                    Add(new ProjectComboBoxItem(row["name"].ToString(), row["notes"].ToString(), row["connection_properties"].ToString()));
                }

                conn.Close();
            }

            Enabled = true; //enables the ComboBox
            //SelectedItem = ItemCollection.FirstOrDefault(); //set the default item in the comboBox

        }


        /// <summary>
        /// The on comboBox selection change event. 
        /// </summary>
        /// <param name="item">The newly selected combo box item</param>
        protected override async void OnSelectionChange(ComboBoxItem item) {
            FrameworkApplication.State.Activate("project_selected");

            //Debug.WriteLine("item type = " + item.GetType());
            if (item == null)
                return;

            if (string.IsNullOrEmpty(item.Text))
                return;

            // TODO  Code behavior when selection changes.
            if (item is ProjectComboBoxItem) { //TODO: This is only here because I'm not sure how to have a combobox without an initial selection
                var collProps = JObject.Parse(((ProjectComboBoxItem)item).connectionProperties);
                //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"connection properties: " + ((ProjectComboBoxItem)item).connectionProperties);
                //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show($"connection properties: " + collProps["database"]);
                //Debug.WriteLine("type of collProps = " + collProps.GetType());
                AddEditMapUnitsDockPaneViewModel.Hide();
                await openDatabase(collProps);
                await DataHelper.populateMapUnits(); //TODO: maybe move this into openDatabase?
                //AddEditMapUnitsDockPaneViewModel.Show();
            }
        }

        //List<FeatureLayer> currentLayers = new List<FeatureLayer>();
        //List<StandaloneTable> currentTables = new List<StandaloneTable>();

        private async Task openDatabase(JObject props) {
            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() => {
                var map = MapView.Active.Map;
                //map.RemoveLayers(map.Layers);
                //map.RemoveStandaloneTables(map.StandaloneTables);
                map.RemoveLayers(DataHelper.currentLayers);
                map.RemoveStandaloneTables(DataHelper.currentTables);
                DataHelper.currentLayers.Clear();
                DataHelper.currentTables.Clear();

                //Get Layers that are NOT Group layers and are unchecked
                //var layers = MapView.Active.Map.Layers.ToList();
                //MapView.Active.Map.RemoveLayers(layers);

                // Opening a Non-Versioned SQL Server instance.
                ArcGIS.Core.Data.DatabaseConnectionProperties connectionProperties = new DatabaseConnectionProperties(EnterpriseDatabaseType.PostgreSQL) {
                    AuthenticationMode = AuthenticationMode.DBMS,

                    // Where testMachine is the machine where the instance is running and testInstance is the name of the SqlServer instance.
                    //Instance = @"127.0.0.1",
                    Instance = props["instance"].ToString(),

                    // Provided that a database called LocalGovernment has been created on the testInstance and geodatabase has been enabled on the database.
                    //Database = "geomapmaker",
                    Database = props["database"].ToString(),

                    // Provided that a login called gdb has been created and corresponding schema has been created with the required permissions.
                    //User = "geomapmaker",
                    User = props["user"].ToString(),
                    //Password = "password",
                    Password = props["password"].ToString(),
                    //Version = "dbo.DEFAULT"
                };
                DataHelper.connectionProperties = connectionProperties;

                using (Geodatabase geodatabase = new Geodatabase(connectionProperties)) {
                    DataHelper.connectionString = geodatabase.GetConnectionString();
                    Debug.WriteLine("DataHelper.connectionString set to " + DataHelper.connectionString);


                    using (Table table = geodatabase.OpenDataset<Table>("DescriptionOfMapUnits")) {

                        QueryFilter queryFilter = new QueryFilter {
                            //WhereClause = "COSTCTRN = 'Information Technology'",
                            SubFields = "MapUnit, AreaFillRGB",
                            PostfixClause = "ORDER BY objectid"
                        };

                        List<CIMColor> colors = new List<CIMColor>();
                        List<String> mapUnits = new List<String>();
                        using (RowCursor rowCursor = table.Search(queryFilter, false)) {
                            List<CIMUniqueValueClass> listUniqueValueClasses = new List<CIMUniqueValueClass>();
                            while (rowCursor.MoveNext()) {
                                using (Row row = rowCursor.Current) {
                                    string mu = Convert.ToString(row["MapUnit"]);
                                    string colorString = Convert.ToString(row["AreaFillRGB"]);
                                    string[] strVals = colorString.Split(';');

                                    // Create a "CIMUniqueValueClass" for the map unit.
                                    List<CIMUniqueValue> listUniqueValues = new List<CIMUniqueValue> {
                                        new CIMUniqueValue {
                                            FieldValues = new string[] { mu }
                                        }
                                    };
                                    var cF = ColorFactory.Instance;
                                    CIMUniqueValueClass uniqueValueClass = new CIMUniqueValueClass {
                                        Editable = true,
                                        Label = mu,
                                        //Patch = PatchShape.Default,
                                        Patch = PatchShape.AreaPolygon, 
                                        //Symbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.RedRGB).MakeSymbolReference(),
                                        Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(cF.CreateRGBColor(Double.Parse(strVals[0]), Double.Parse(strVals[1]), Double.Parse(strVals[2]))).MakeSymbolReference(),
                                        Visible = true,
                                        Values = listUniqueValues.ToArray()
                                    };

                                    listUniqueValueClasses.Add(uniqueValueClass);
                                }
                            }

                            //Create a list of CIMUniqueValueGroup
                            CIMUniqueValueGroup uvg = new CIMUniqueValueGroup {
                                Classes = listUniqueValueClasses.ToArray(),
                            };
                            List<CIMUniqueValueGroup> listUniqueValueGroups = new List<CIMUniqueValueGroup> { uvg };

                            //Create the CIMUniqueValueRenderer
                             DataHelper.mapUnitRenderer = new CIMUniqueValueRenderer {
                                UseDefaultSymbol = false,
                                //DefaultLabel = "all other values",
                                //DefaultSymbolPatch = PatchShape.AreaPolygon,
                                //DefaultSymbol = SymbolFactory.Instance.ConstructPolygonSymbol(ColorFactory.Instance.GreyRGB).MakeSymbolReference(),
                                Groups = listUniqueValueGroups.ToArray(),
                                Fields = new string[] { "mapunit" }
                            };

                        }
                    }

                    var featureClasses = geodatabase.GetDefinitions<FeatureClassDefinition>();
                    foreach (FeatureClassDefinition fCD in featureClasses) {
                        FeatureClass fC = geodatabase.OpenDataset<FeatureClass>(fCD.GetName());
                        FeatureLayer flyr = LayerFactory.Instance.CreateFeatureLayer(fC, MapView.Active.Map);
                        DataHelper.currentLayers.Add(flyr);

                        if (fC.GetName().Contains("MapUnitPolys")) {
                            flyr.SetRenderer(DataHelper.mapUnitRenderer);
                        }
                    }

                    /*
                    UniqueValueRendererDefinition renderDef = new UniqueValueRendererDefinition();
                    using (Table table = geodatabase.OpenDataset<Table>("DescriptionOfMapUnits")) {

                        QueryFilter queryFilter = new QueryFilter {
                            //WhereClause = "COSTCTRN = 'Information Technology'",
                            SubFields = "MapUnit, AreaFillRGB",
                            PostfixClause = "ORDER BY objectid"
                        };

                        List<CIMColor> colors = new List<CIMColor>();
                        List<String> mapUnits = new List<String>();
                        using (RowCursor rowCursor = table.Search(queryFilter, false)) {
                            while (rowCursor.MoveNext()) {
                                using (Row row = rowCursor.Current) {
                                    string mu = Convert.ToString(row["MapUnit"]);
                                    string colorString = Convert.ToString(row["AreaFillRGB"]);
                                    string[] strVals = colorString.Split(';');
                                    double[] values = new double[] { 49, 237, 28, 255 }; //default ugly green
                                    if (strVals.Length ==3) {
                                        values = new double[] { Double.Parse(strVals[0]), Double.Parse(strVals[1]), Double.Parse(strVals[2]), 1};
                                    } 
                                    CIMRGBColor color = new CIMRGBColor();
                                    color.Values = values;
                                    //colors.Add(color);
                                    var cF = ColorFactory.Instance;
                                    colors.Add(cF.CreateRGBColor(Double.Parse(strVals[0]), Double.Parse(strVals[1]), Double.Parse(strVals[2])));
                                    mapUnits.Add(mu);
                                }
                            }
                        }

                        CIMFixedColorRamp ramp = new CIMFixedColorRamp();
                        ramp.Colors = colors.ToArray();
                        renderDef.ColorRamp = ramp;
                        //renderDef.ValueFields = mapUnits.ToArray();
                        renderDef.ValueFields = new string[] { "mapunit" };
                    }
                    //renderDef.ValueFields = new string[] {"MapUnit"};
                    var featureClasses = geodatabase.GetDefinitions<FeatureClassDefinition>();
                    foreach (FeatureClassDefinition fCD in featureClasses) {
                        FeatureClass fC = geodatabase.OpenDataset<FeatureClass>(fCD.GetName());
                        FeatureLayer flyr = LayerFactory.Instance.CreateFeatureLayer(fC, MapView.Active.Map);
                        DataHelper.currentLayers.Add(flyr);

                        if (fC.GetName().Contains("MapUnitPolys")) {
                            var renderer = flyr.CreateRenderer(renderDef);
                            flyr.SetRenderer(renderer);
                        }
                    }
                    */

                    /* This works, but uses symbol field in mapunitpolys
                    var featureClasses = geodatabase.GetDefinitions<FeatureClassDefinition>();
                    foreach (FeatureClassDefinition fCD in featureClasses) {
                        FeatureClass fC = geodatabase.OpenDataset<FeatureClass>(fCD.GetName());
                        FeatureLayer flyr = LayerFactory.Instance.CreateFeatureLayer(fC, MapView.Active.Map);
                        DataHelper.currentLayers.Add(flyr); 

                        //This sets the color to what is in the Symbol field of the MapUnitPolys record
                        //Colors there must be either color names (Red, Green, etc.) or hex encoded (#RRGGBB).
                        //Using this Symbol field is not ideal. I'd prefer to use the color from DescriptionOfMapUnitPolys.
                        //Not sure how to do that yet.
                        if (fC.GetName().Contains("MapUnitPolys")) {
                            var renderer = flyr.GetRenderer() as CIMSimpleRenderer;
                            var primitiveName = Guid.NewGuid().ToString();

                            var cimPO = new CIMPrimitiveOverride() {
                                PrimitiveName = primitiveName,
                                PropertyName = @"Color",
                                Expression = null,
                                ValueExpressionInfo = new CIMExpressionInfo() {
                                    Title = "Custom",
                                    Expression = @"$feature.Symbol",
                                    ReturnType = ExpressionReturnType.Default
                                }
                            };

                            //TODO: Probably better to iterate the SymbolLayers looking for CIMSolidFill explicitly 
                            //rather than relying on position.
                            (renderer.Symbol.Symbol as CIMPolygonSymbol).SymbolLayers[1].PrimitiveName = primitiveName;

                            var overrideList = new CIMPrimitiveOverride[1];
                            overrideList[0] = cimPO;

                            //Apply symbol overrides
                            renderer.Symbol.PrimitiveOverrides = overrideList;

                            //Apply the renderer to the feature layer
                            flyr.SetRenderer(renderer);
                        }

                    }
                    */

                    var tables = geodatabase.GetDefinitions<TableDefinition>();
                    IStandaloneTableFactory tableFactory = StandaloneTableFactory.Instance;
                    foreach (TableDefinition tD in tables)
                    {
                        Table t = geodatabase.OpenDataset<Table>(tD.GetName());
                        //tableFactory.CreateStandaloneTable(t, MapView.Active.Map);
                        DataHelper.currentTables.Add(tableFactory.CreateStandaloneTable(t, MapView.Active.Map));
                    }

                }
                DataHelper.ProjectSelected();
            });
        }
        
    }
}
