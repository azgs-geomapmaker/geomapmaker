using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows;
using System.Windows.Input;
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

namespace Geomapmaker
{
    internal class AddEditMapUnitPolysDockPaneViewModel : DockPane
    {
        private const string _dockPaneID = "Geomapmaker_AddEditMapUnitPolysDockPane";

        protected AddEditMapUnitPolysDockPaneViewModel()
        {
            SelectedMapUnitPoly = new MapUnitPoly();
            SelectedMapUnitPoly.DataSource = DataHelper.DataSource.Source; //for display
            GeomapmakerModule.MapUnitPolysVM = this;
        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;
            pane.Activate();
        }

        internal static new void Hide()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;
            pane.Hide();
        }

        public Boolean IsValid
        {
            //TODO: This is not raising property changed event, so the button never enables. Hard to believe I'll have to raise an event from 
            //each of the properties used below. There must be a better way.
            get
            {
                //return true;

                return
                    SelectedMapUnit != null &&
                    SelectedMapUnitPoly != null &&
                    SelectedMapUnitPoly.IdentityConfidence != null && SelectedMapUnitPoly.IdentityConfidence.Trim() != "" &&
                    //SelectedMapUnitPoly.Shape != null;// &&
                    Shape != null;// &&
                                  //SelectedMapUnitPoly.Notes != null && SelectedMapUnitPoly.Notes.Trim() != "" &&
            }
        }

        public void Reset()
        {
            //Just clear whichever and ignore the other error
            GeomapmakerModule.AddMapUnitPolyTool?.Clear();
            GeomapmakerModule.EditMapUnitPolyTool?.Clear();

            SelectedMapUnitPoly = new MapUnitPoly();
            SelectedMapUnit = null;
            SelectedMapUnitPoly.Shape = null;
            ShapeJson = null;
        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Map Unit Polys";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        //public string SelectedMapUnit { get; set; }
        private MapUnit selectedMapUnit;
        public MapUnit SelectedMapUnit
        {
            get => selectedMapUnit;
            set
            {
                SetProperty(ref selectedMapUnit, value, () => SelectedMapUnit); //Have to do this to trigger stuff, I guess.

                //Echo the color from the MapUnit to the Symbol field in the poly.
                //TODO: I really don't like this. What if user changes color in MapUnit? Need to handle this.
                if (value != null)
                {
                    var mapUnit = Array.Find<MapUnit>(DataHelper.MapUnits.ToArray(), mu => mu.MU == value.MU);
                    SelectedMapUnitPoly.Symbol = mapUnit.HexColor;
                }
            }
        }


        private MapUnitPoly selectedMapUnitPoly;
        public MapUnitPoly SelectedMapUnitPoly
        {
            get => selectedMapUnitPoly;
            set
            {
                value.DataSource = DataHelper.DataSource.Source; //for display
                SetProperty(ref selectedMapUnitPoly, value, () => SelectedMapUnitPoly); //Have to do this to trigger stuff, I guess.
            }
        }

        //private Geometry shape = null;
        public Geometry Shape
        {
            get => SelectedMapUnitPoly.Shape; //shape;
            set
            {
                //SetProperty(ref shape, value, () => Shape);
                SelectedMapUnitPoly.Shape = value;
                ShapeJson = value.ToJson();
                CommandManager.InvalidateRequerySuggested(); //Force update of submit button
                                                             //TODO: The previous line is not enabling Submit when geometry is changed during editing
            }
        }

        private string shapeJson = null;
        public string ShapeJson
        {
            get => shapeJson;
            set
            {
                SetProperty(ref shapeJson, value, () => ShapeJson);
            }
        }

        public async Task saveMapUnitPoly(/*MapUnitPoly mapUnitPoly*/)
        {
            Debug.WriteLine("saveMapUnitPoly enter");
            //MessageBox.Show("Saving");

            //return ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() => {

            var polyLayer = MapView.Active.Map.GetLayersAsFlattenedList().First((l) => l.Name == "MapUnitPolys") as FeatureLayer;

            //Define some default attribute values
            Dictionary<string, object> attributes = new Dictionary<string, object>();

            attributes["SHAPE"] = SelectedMapUnitPoly.Shape;//Geometry
            attributes["Symbol"] = SelectedMapUnitPoly.Symbol;
            attributes["MapUnit"] = SelectedMapUnit.MU;
            attributes["IdentityConfidence"] = SelectedMapUnitPoly.IdentityConfidence;
            attributes["Notes"] = SelectedMapUnitPoly.Notes;
            attributes["DataSourceID"] = DataHelper.DataSource.DataSource_ID;
            //TODO: other fields

            //Create the new feature
            var op = new EditOperation();
            if (SelectedMapUnitPoly.ID == null)
            {
                op.Name = string.Format("Create {0}", "MapUnitPolys");
                op.Create(polyLayer, attributes);
            }
            else
            {
                op.Name = string.Format("Modify {0}", "MapUnitPolys");
                op.Modify(polyLayer, (Int64)SelectedMapUnitPoly.ID, SelectedMapUnitPoly.Shape, attributes);
            }
            await op.ExecuteAsync();

            if (!op.IsSucceeded)
            {
                MessageBox.Show("Hogan's goat!");
            }

            //Moved this here from AddEditMapUnitsViewModel to accommodate keeping only active mu's in renderer.
            await DataHelper.PopulateMapUnits();
            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                polyLayer.ClearSelection();
            });

            //});

        }

        /// <summary>
        /// Button implementation to show the DockPane.
        /// </summary>
        internal class AddEditMapUnitPolysDockPane_ShowButton : Button
        {
            protected override void OnClick()
            {
                AddEditMapUnitPolysDockPaneViewModel.Show();
            }
        }
    }
}
