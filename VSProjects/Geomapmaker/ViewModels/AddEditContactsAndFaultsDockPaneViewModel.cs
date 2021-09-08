using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using Newtonsoft.Json;

namespace Geomapmaker
{
    internal class AddEditContactsAndFaultsDockPaneViewModel : DockPane
    {
        private const string _dockPaneID = "Geomapmaker_AddEditContactsAndFaultsDockPane";

        protected AddEditContactsAndFaultsDockPaneViewModel()
        {
            SelectedCF = new CF();
            SelectedCF.DataSource = DataHelper.DataSource.Source; //for display
            ShapeJson = "{ }";
            GeomapmakerModule.ContactsAndFaultsVM = this;
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

        internal static void Hide()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;
            pane.Hide();
        }

        public void Reset()
        {
            //Just clear whichever and ignore the other error
            if (GeomapmakerModule.ContactsAndFaultsAddTool != null)
            {
                GeomapmakerModule.ContactsAndFaultsAddTool.Clear();
            }

            if (GeomapmakerModule.ContactsAndFaultsEditTool != null)
            {
                GeomapmakerModule.ContactsAndFaultsEditTool.Clear();
            }

            SelectedCFSymbol = null;
            SelectedCF = new CF();
            ShapeJson = "{ }";
            Prepopulate = false;
        }

        private bool prepopulate;
        public bool Prepopulate
        {
            get { return prepopulate; }
            set
            {
                SetProperty(ref prepopulate, value, () => Prepopulate); //Have to do this to trigger stuff, I guess.
                if (value)
                {
                    GeomapmakerModule.ContactsAndFaultsAddTool.SetPopulate();
                }
            }
        }

        public Boolean IsValid
        {
            //TODO: It is possible to enter errant values for the Type (symbol key). Need to figure out validation on that.
            get
            {
                //return true;

                return SelectedCF != null
                    && !string.IsNullOrWhiteSpace(SelectedCF.IdentityConfidence)
                    && !string.IsNullOrWhiteSpace(SelectedCF.ExistenceConfidence)
                    && !string.IsNullOrWhiteSpace(SelectedCF.LocationConfidenceMeters)
                    && !string.IsNullOrWhiteSpace(SelectedCF.IsConcealed)
                    && Shape != null;
            }
        }


        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Contacts and Faults";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        private CFSymbol selectedCFSymbol;
        public CFSymbol SelectedCFSymbol
        {
            get => selectedCFSymbol;
            set
            {
                SetProperty(ref selectedCFSymbol, value, () => SelectedCFSymbol); //Have to do this to trigger stuff, I guess.
                SelectedCF.symbol = selectedCFSymbol;
            }
        }

        //TODO: Need to separate this from the Type (symbol) combobox. The interaction is not working correctly.
        private CF selectedCF;
        public CF SelectedCF
        {
            get => selectedCF;
            set
            {
                value.DataSource = DataHelper.DataSource.Source; //for display
                SetProperty(ref selectedCF, value, () => SelectedCF); //Have to do this to trigger stuff, I guess.
            }
        }

        public Geometry Shape
        {
            get => SelectedCF.Shape; //shape;
            set
            {
                //SetProperty(ref shape, value, () => Shape);
                SelectedCF.Shape = value;
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

        public async Task saveCF()
        {
            Debug.WriteLine("saveCF enter");
            //MessageBox.Show("Saving");

            //return ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() => {

            var cfLayer = MapView.Active.Map.GetLayersAsFlattenedList().First((l) => l.Name == "ContactsAndFaults") as FeatureLayer;

            //Define some default attribute values
            Dictionary<string, object> attributes = new Dictionary<string, object>();

            attributes["SHAPE"] = SelectedCF.Shape;//Geometry
            attributes["Symbol"] = SelectedCF.symbol.key;
            attributes["IdentityConfidence"] = SelectedCF.IdentityConfidence;
            attributes["ExistenceConfidence"] = SelectedCF.ExistenceConfidence;
            attributes["LocationConfidenceMeters"] = SelectedCF.LocationConfidenceMeters;
            attributes["IsConcealed"] = SelectedCF.IsConcealed;
            attributes["Notes"] = SelectedCF.Notes;
            attributes["DataSourceID"] = DataHelper.DataSource.DataSource_ID;
            //TODO: other fields

            //Create the new feature
            var op = new EditOperation();
            if (SelectedCF.ID == null)
            {
                op.Name = string.Format("Create {0}", "ContactsAndFaults");
                op.Create(cfLayer, attributes);
            }
            else
            {
                op.Name = string.Format("Modify {0}", "MapUnitPolys");
                op.Modify(cfLayer, (Int64)SelectedCF.ID, SelectedCF.Shape, attributes);
            }
            await op.ExecuteAsync();

            if (!op.IsSucceeded)
            {
                MessageBox.Show("Hogan's goat!");
            }

            //Update renderer with new symbol
            //TODO: This approach (just adding the new symbol to the renderer) does not remove a symbol if it is no longer used.
            List<CIMUniqueValueClass> listUniqueValueClasses = new List<CIMUniqueValueClass>(DataHelper.cfRenderer.Groups[0].Classes);
            List<CIMUniqueValue> listUniqueValues = new List<CIMUniqueValue> {
                                        new CIMUniqueValue {
                                            FieldValues = new string[] { SelectedCF.symbol.key }
                                        }
                                    };

            CIMUniqueValueClass uniqueValueClass = new CIMUniqueValueClass
            {
                Editable = true,
                Label = SelectedCF.symbol.key,
                //Patch = PatchShape.Default,
                Patch = PatchShape.AreaPolygon,
                Symbol = CIMSymbolReference.FromJson(SelectedCF.symbol.symbol, null),
                Visible = true,
                Values = listUniqueValues.ToArray()
            };
            listUniqueValueClasses.Add(uniqueValueClass);
            CIMUniqueValueGroup uvg = new CIMUniqueValueGroup
            {
                Classes = listUniqueValueClasses.ToArray(),
            };
            List<CIMUniqueValueGroup> listUniqueValueGroups = new List<CIMUniqueValueGroup> { uvg };
            DataHelper.cfRenderer = new CIMUniqueValueRenderer
            {
                UseDefaultSymbol = false,
                Groups = listUniqueValueGroups.ToArray(),
                Fields = new string[] { "symbol" }
                //ValueExpressionInfo = cEI //fields used for testing
            };

            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                cfLayer.ClearSelection();
                cfLayer.SetRenderer(DataHelper.cfRenderer);
            });


            //});

        }



    }



    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class AddEditContactsAndFaults_ShowButton : Button
    {
        protected override void OnClick()
        {
            AddEditContactsAndFaultsDockPaneViewModel.Show();
        }
    }
}
