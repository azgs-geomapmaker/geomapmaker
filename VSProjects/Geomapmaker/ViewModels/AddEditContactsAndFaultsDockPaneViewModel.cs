﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;

namespace Geomapmaker
{
    internal class AddEditContactsAndFaultsDockPaneViewModel : DockPane
    {
        private const string _dockPaneID = "Geomapmaker_AddEditContactsAndFaultsDockPane";

        public ICommand CommandSubmit { get; }
        public ICommand CommandReset { get; }

        protected AddEditContactsAndFaultsDockPaneViewModel()
        {
            CommandSubmit = new RelayCommand(() => SaveAsync(), () => CanSubmit());
            CommandReset = new RelayCommand(() => Reset());
            RefreshCFSymbolsAsync();

            SelectedCF = new ContactFault();
            SelectedCFSymbol = CFSymbolsOptions.FirstOrDefault();
            SelectedCF.DataSource = GeomapmakerModule.DataSourceId;
            ShapeJson = "{ }";
            GeomapmakerModule.ContactsAndFaultsVM = this;
        }

        private List<CFSymbol> _cfSymbolsOptions = new List<CFSymbol>();
        public List<CFSymbol> CFSymbolsOptions
        {
            get => _cfSymbolsOptions;
            set => SetProperty(ref _cfSymbolsOptions, value, () => CFSymbolsOptions);
        }

        public async void RefreshCFSymbolsAsync()
        {
            CFSymbolsOptions = await Data.CFSymbols.GetCFSymbolList();
        }

        private bool CanSubmit()
        {
            return !(SelectedCF == null
                || SelectedCF.Symbol == null
                || string.IsNullOrWhiteSpace(SelectedCF.IdentityConfidence)
                || string.IsNullOrWhiteSpace(SelectedCF.ExistenceConfidence)
                || string.IsNullOrWhiteSpace(SelectedCF.LocationConfidenceMeters)
                || Shape == null);
        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
            {
                return;
            }

            pane.Activate();
        }

        internal static new void Hide()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
            {
                return;
            }

            pane.Hide();
        }

        public void Reset()
        {
            //Just clear whichever and ignore the other error
            GeomapmakerModule.ContactsAndFaultsAddTool?.Clear();
            GeomapmakerModule.ContactsAndFaultsEditTool?.Clear();

            SelectedCFSymbol = CFSymbolsOptions.FirstOrDefault();
            SelectedCF = new ContactFault();
            ShapeJson = "{ }";
            Prepopulate = false;
        }

        private bool prepopulate;
        public bool Prepopulate
        {
            get => prepopulate;
            set
            {
                SetProperty(ref prepopulate, value, () => Prepopulate); //Have to do this to trigger stuff, I guess.

                // if the toggle-btn is active
                if (value)
                {
                    GeomapmakerModule.ContactsAndFaultsAddTool.SetPopulate();
                }
            }
        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Contacts and Faults";
        public string Heading
        {
            get => _heading;
            set => SetProperty(ref _heading, value, () => Heading);
        }

        private CFSymbol selectedCFSymbol;
        public CFSymbol SelectedCFSymbol
        {
            get => selectedCFSymbol;
            set
            {
                SetProperty(ref selectedCFSymbol, value, () => SelectedCFSymbol); // Have to do this to trigger stuff, I guess.
                SelectedCF.Symbol = selectedCFSymbol;
            }
        }

        // TODO: Need to separate this from the Type (symbol) combobox. The interaction is not working correctly.
        private ContactFault selectedCF;
        public ContactFault SelectedCF
        {
            get => selectedCF;
            set
            {
                value.DataSource = GeomapmakerModule.DataSourceId; // For display
                SetProperty(ref selectedCF, value, () => SelectedCF); // Have to do this to trigger stuff, I guess.
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
                CommandManager.InvalidateRequerySuggested(); // Force update of submit button
                                                             // TODO: The previous line is not enabling Submit when geometry is changed during editing
            }
        }

        private string shapeJson = null;
        public string ShapeJson
        {
            get => shapeJson;
            set => SetProperty(ref shapeJson, value, () => ShapeJson);
        }

        public async Task SaveAsync()
        {
            FeatureLayer cfLayer = MapView.Active.Map.GetLayersAsFlattenedList().First((l) => l.Name == "ContactsAndFaults") as FeatureLayer;

            // Define some default attribute values
            Dictionary<string, object> attributes = new Dictionary<string, object>
            {
                ["SHAPE"] = SelectedCF.Shape,//Geometry
                ["TYPE"] = SelectedCF.Symbol.Description,
                ["Symbol"] = SelectedCF.Symbol.Key,
                ["IdentityConfidence"] = SelectedCF.IdentityConfidence,
                ["ExistenceConfidence"] = SelectedCF.ExistenceConfidence,
                ["LocationConfidenceMeters"] = SelectedCF.LocationConfidenceMeters,
                ["IsConcealed"] = SelectedCF.IsConcealed ? "Y" : "N", // Convert the bool to 'y'/'n'
                ["Notes"] = SelectedCF.Notes,
                ["DataSourceID"] = GeomapmakerModule.DataSourceId
            };

            // Create the new feature
            EditOperation op = new EditOperation();
            if (SelectedCF.ID == null)
            {
                op.Name = string.Format("Create {0}", "ContactsAndFaults");
                op.Create(cfLayer, attributes);
            }
            else
            {
                op.Name = string.Format("Modify {0}", "MapUnitPolys");
                op.Modify(cfLayer, (long)SelectedCF.ID, SelectedCF.Shape, attributes);
            }
            await op.ExecuteAsync();

            if (!op.IsSucceeded)
            {
                MessageBox.Show("Hogan's goat!");
            }

            // Update renderer with new symbol
            // TODO: This approach (just adding the new symbol to the renderer) does not remove a symbol if it is no longer used.
            List<CIMUniqueValueClass> listUniqueValueClasses = new List<CIMUniqueValueClass>(Data.CFSymbols.cfRenderer.Groups[0].Classes);

            List<CIMUniqueValue> listUniqueValues = new List<CIMUniqueValue> {
                new CIMUniqueValue {
                    FieldValues = new string[] { SelectedCF.Symbol.Key }
                }
            };

            CIMUniqueValueClass uniqueValueClass = new CIMUniqueValueClass
            {
                Editable = true,
                Label = SelectedCF.Symbol.Key,
                Patch = PatchShape.AreaPolygon,
                Symbol = CIMSymbolReference.FromJson(SelectedCF.Symbol.SymbolJson, null),
                Visible = true,
                Values = listUniqueValues.ToArray()
            };
            listUniqueValueClasses.Add(uniqueValueClass);

            CIMUniqueValueGroup uvg = new CIMUniqueValueGroup
            {
                Classes = listUniqueValueClasses.ToArray(),
            };
            List<CIMUniqueValueGroup> listUniqueValueGroups = new List<CIMUniqueValueGroup> { uvg };

            Data.CFSymbols.cfRenderer = new CIMUniqueValueRenderer
            {
                UseDefaultSymbol = false,
                Groups = listUniqueValueGroups.ToArray(),
                Fields = new string[] { "symbol" }
                //ValueExpressionInfo = cEI //fields used for testing
            };

            await QueuedTask.Run(() =>
            {
                cfLayer.ClearSelection();
                cfLayer.SetRenderer(Data.CFSymbols.cfRenderer);
            });

            Reset();
        }
    }
}