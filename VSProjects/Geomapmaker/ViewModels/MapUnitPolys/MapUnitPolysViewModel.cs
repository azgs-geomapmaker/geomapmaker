using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.MapUnitPolys
{
    public class MapUnitPolysViewModel : ProWindow, INotifyPropertyChanged
    {
        public event EventHandler WindowCloseEvent;

        public ICommand CommandCancel => new RelayCommand(() => CloseProwindow());

        public void CloseProwindow()
        {
            WindowCloseEvent(this, new EventArgs());
        }

        public CreateMapUnitPolysVM Create { get; set; }
        public EditMapUnitPolysVM Edit { get; set; }
        public DeleteMapUnitPolysVM Delete { get; set; }

        public MapUnitPolysViewModel()
        {
            Create = new CreateMapUnitPolysVM(this);
            Edit = new EditMapUnitPolysVM(this);
            Delete = new DeleteMapUnitPolysVM(this);
        }

        private List<MapUnitPolyTemplate> _mapUnits { get; set; }
        public List<MapUnitPolyTemplate> MapUnits
        {
            get => _mapUnits;
            set
            {
                _mapUnits = value;
                NotifyPropertyChanged();
            }
        }

        public async void RefreshMapUnitsAsync()
        {
            Data.MapUnitPolys.RebuildMUPSymbologyAndTemplates();
            MapUnits = await Data.MapUnitPolys.GetMapUnitPolyTemplatesAsync();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    internal class ShowMapUnitPolys : Button
    {
        private Views.MapUnitPolys.MapUnitPolys _mapunitpolys = null;

        protected override void OnClick()
        {
            //already open?
            if (_mapunitpolys != null)
            {
                _mapunitpolys.Close();
                return;
            }

            _mapunitpolys = new Views.MapUnitPolys.MapUnitPolys
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            _mapunitpolys.mapUnitPolysVM.RefreshMapUnitsAsync();

            _mapunitpolys.Closed += (o, e) =>
            {
                // Reset the map tool to explore
                FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");

                _mapunitpolys = null;
            };

            _mapunitpolys.mapUnitPolysVM.WindowCloseEvent += (s, e) => _mapunitpolys.Close();

            _mapunitpolys.Show();
        }

    }
}
















//using ArcGIS.Core.CIM;
//using ArcGIS.Core.Data;
//using ArcGIS.Desktop.Editing;
//using ArcGIS.Desktop.Editing.Attributes;
//using ArcGIS.Desktop.Editing.Templates;
//using ArcGIS.Desktop.Framework;
//using ArcGIS.Desktop.Framework.Contracts;
//using ArcGIS.Desktop.Framework.Controls;
//using ArcGIS.Desktop.Framework.Threading.Tasks;
//using ArcGIS.Desktop.Mapping;
//using Geomapmaker.Models;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using System.Windows.Input;

//namespace Geomapmaker.ViewModels.MapUnitPolys
//{
//    public class MapUnitPolysViewModel : ProWindow, INotifyPropertyChanged, INotifyDataErrorInfo
//    {
//        public event EventHandler WindowCloseEvent;

//        public ICommand CommandCancel => new RelayCommand(() => CloseProwindow());

//        public void CloseProwindow()
//        {
//            WindowCloseEvent(this, new EventArgs());
//        }

//        // Create a Map Unit Polygon
//        public ICommand CommandCreate => new RelayCommand(() => CreateAsync(), () => CanCreate());

//        // Reset Object ids
//        public ICommand CommandClearOids => new RelayCommand(() => ClearOids(), () => ContactFaultOids != null && ContactFaultOids.Count > 0);

//        private void ClearOids()
//        {
//            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "ContactsAndFaults");

//            QueuedTask.Run(() =>
//            {
//                layer?.ClearSelection();
//            });

//            ContactFaultOids = new Dictionary<long, string>();
//            NotifyPropertyChanged("OidsListBox");
//        }

//        private bool CanCreate()
//        {
//            return ContactFaultOids.Count() > 0 && Selected != null && HasErrors == false;
//        }

//        // Create the Map Unit Polygons from the selected Contacts and Faults
//        private async void CreateAsync()
//        {
//            // Contacts and Faults layer
//            FeatureLayer cfLayer = MapView.Active.Map.GetLayersAsFlattenedList().First((l) => l.Name == "ContactsAndFaults") as FeatureLayer;

//            EditOperation op = new EditOperation
//            {
//                Name = "Create MapUnitPolys"
//            };

//            // Map Unit Poly layer
//            FeatureLayer polyLayer = MapView.Active.Map.GetLayersAsFlattenedList().First((l) => l.Name == "MapUnitPolys") as FeatureLayer;

//            await QueuedTask.Run(() =>
//            {
//                EditingTemplate tmpTemplate = polyLayer.GetTemplate(Selected.MapUnit);

//                CIMFeatureTemplate templateDef = tmpTemplate.GetDefinition() as CIMFeatureTemplate;

//                templateDef.DefaultValues["IdentityConfidence"] = IdentityConfidence;
//                templateDef.DefaultValues["Notes"] = Notes;

//                tmpTemplate.SetDefinition(templateDef);

//                EditingTemplate updatedTemplate = polyLayer.GetTemplate(Selected.MapUnit);

//                // Contruct polygons from cf lines
//                op.ConstructPolygons(updatedTemplate, cfLayer, ContactFaultOids.Keys, null, false);

//                op.Execute();

//                // Check if the polygon create was a success
//                if (op.IsSucceeded)
//                {
//                    ContactFaultOids = new Dictionary<long, string>();

//                    // Reset default values
//                    templateDef.DefaultValues["IdentityConfidence"] = null;
//                    templateDef.DefaultValues["Notes"] = null;
//                    tmpTemplate.SetDefinition(templateDef);

//                    OperationManager opManager = MapView.Active.Map.OperationManager;

//                    // Remove Undo items we don't want users to be able to undo
//                    List<Operation> mapUnitPolyLayerUndos = opManager.FindUndoOperations(a => a.Name == "Update layer definition: MapUnitPolys" || a.Name == "Update layer renderer: MapUnitPolys");
//                    foreach (Operation undoOp in mapUnitPolyLayerUndos)
//                    {
//                        opManager.RemoveUndoOperation(undoOp);
//                    }

//                    cfLayer.ClearSelection();
//                    polyLayer.ClearSelection();
//                }
//                else
//                {
//                    // Display error
//                    _validationErrors["SelectedOid"] = new List<string>() { "Could not create polygon from CF lines. Select more lines." };
//                    RaiseErrorsChanged("SelectedOid");
//                }

//            });

//        }

//        public MapUnitPolysViewModel()
//        {
//            FrameworkApplication.SetCurrentToolAsync("Geomapmaker_MapUnitPolyTool");

//            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "ContactsAndFaults");

//            if (layer == null)
//            {
//                return;
//            }

//            QueuedTask.Run(() =>
//            {
//                Selection cfSelection = layer.GetSelection();

//                IReadOnlyList<long> cfOids = cfSelection.GetObjectIDs();

//                Set_CF_Oids(cfOids.ToList());
//            });

//            // Trigger validation
//            Selected = Selected;
//        }

//        // Collection of ID/Labels for selected CF lines
//        private Dictionary<long, string> _contactFaultOids { get; set; } = new Dictionary<long, string>();
//        public Dictionary<long, string> ContactFaultOids {
//            get => _contactFaultOids;
//            set
//            {
//                _contactFaultOids = value;
//                NotifyPropertyChanged("OidsListBox");
//                NotifyPropertyChanged("ContactFaultOids");

//                // Error is displayed on the selection
//                ValidateCFOids(ContactFaultOids, "SelectedOid");
//            }

//        }

//        // Collection for displaying in View
//        public List<string> OidsListBox => ContactFaultOids.Select(a => $"{a.Value} ({a.Key})").ToList();

//        public string SelectedOid { get; set; }

//        private List<MapUnitPolyTemplate> _mapUnits { get; set; }
//        public List<MapUnitPolyTemplate> MapUnits
//        {
//            get => _mapUnits;
//            set
//            {
//                _mapUnits = value;
//                NotifyPropertyChanged();
//            }
//        }

//        private MapUnitPolyTemplate _selected;
//        public MapUnitPolyTemplate Selected
//        {
//            get => _selected;
//            set
//            {
//                _selected = value;

//                ValidateMapUnit(Selected, "Selected");

//                // Trigger validation
//                IdentityConfidence = IdentityConfidence;
//            }
//        }

//        private string _identityConfidence;
//        public string IdentityConfidence
//        {
//            get => _identityConfidence;
//            set
//            {
//                _identityConfidence = value;
//                ValidateRequiredString(IdentityConfidence, "IdentityConfidence");
//                NotifyPropertyChanged();
//            }
//        }

//        private string _notes;
//        public string Notes
//        {
//            get => _notes;
//            set
//            {
//                _notes = value;
//                NotifyPropertyChanged();
//            }
//        }

//        private string _dataSource = GeomapmakerModule.DataSourceId;
//        public string DataSource
//        {
//            get => _dataSource;
//            set
//            {
//                _notes = value;
//                NotifyPropertyChanged();
//            }
//        }

//        public async void Set_CF_Oids(List<long> oids)
//        {
//            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "ContactsAndFaults");

//            if (layer == null)
//            {
//                return;
//            }

//            // Remove any errors from CF Lines
//            _validationErrors.Remove("SelectedOid");
//            RaiseErrorsChanged("SelectedOid");

//            Inspector insp = new Inspector();

//            await QueuedTask.Run(() =>
//            {
//                foreach (long id in oids)
//                {
//                    if (ContactFaultOids.ContainsKey(id))
//                    {
//                        ContactFaultOids.Remove(id);
//                    }
//                    else
//                    {
//                        insp.Load(layer, id);

//                        string label = insp["type"]?.ToString();

//                        ContactFaultOids.Add(id, label);
//                    }
//                }
//            });

//            if (ContactFaultOids.Count == 0)
//            {
//                layer.ClearSelection();
//            }
//            else
//            {
//                QueryFilter queryFilter = new QueryFilter
//                {
//                    ObjectIDs = ContactFaultOids.Keys.ToList()
//                };

//                layer.Select(queryFilter);
//            }

//            NotifyPropertyChanged("OidsListBox");

//            // Trigger Validation
//            ContactFaultOids = ContactFaultOids;
//        }

//        public async void RefreshMapUnitsAsync()
//        {
//            Data.MapUnitPolys.RebuildMUPSymbologyAndTemplates();
//            MapUnits = await Data.MapUnitPolys.GetMapUnitPolyTemplatesAsync();
//        }

//        #region INotifyPropertyChanged

//        public event PropertyChangedEventHandler PropertyChanged;

//        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
//        {
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
//        }
//        #endregion

//        #region Validation

//        private readonly Dictionary<string, ICollection<string>> _validationErrors = new Dictionary<string, ICollection<string>>();

//        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

//        private void RaiseErrorsChanged(string propertyName)
//        {
//            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
//        }

//        public IEnumerable GetErrors(string propertyName)
//        {
//            // Return null if parameters is null/empty OR there are no errors for that parameter
//            // Otherwise, return the errors for that parameter.
//            return string.IsNullOrEmpty(propertyName) || !_validationErrors.ContainsKey(propertyName) ?
//                null : _validationErrors[propertyName];
//        }

//        public bool HasErrors => _validationErrors.Count > 0;

//        private void ValidateRequiredString(string text, string propertyKey)
//        {
//            // Required field
//            if (string.IsNullOrEmpty(text))
//            {
//                _validationErrors[propertyKey] = new List<string>() { "" };
//            }
//            else
//            {
//                _validationErrors.Remove(propertyKey);
//            }

//            RaiseErrorsChanged(propertyKey);
//        }

//        private void ValidateMapUnit(MapUnitPolyTemplate selected, string propertyKey)
//        {
//            // Required field
//            if (selected == null)
//            {
//                _validationErrors[propertyKey] = new List<string>() { "" };
//            }
//            else
//            {
//                _validationErrors.Remove(propertyKey);
//            }

//            RaiseErrorsChanged(propertyKey);
//        }

//        private void ValidateCFOids(Dictionary<long, string> contactFaultOids, string propertyKey)
//        {
//            // Required field
//            if (contactFaultOids == null || contactFaultOids.Count == 0)
//            {
//                _validationErrors[propertyKey] = new List<string>() { "" };
//            }
//            else
//            {
//                _validationErrors.Remove(propertyKey);
//            }

//            RaiseErrorsChanged(propertyKey);
//        }

//        #endregion
//    }

//    internal class ShowMapUnitPolys : Button
//    {
//        private Views.MapUnitPolys.MapUnitPolys _mapunitpolys = null;

//        protected override void OnClick()
//        {
//            //already open?
//            if (_mapunitpolys != null)
//            {
//                _mapunitpolys.Close();
//                return;
//            }

//            _mapunitpolys = new Views.MapUnitPolys.MapUnitPolys
//            {
//                Owner = System.Windows.Application.Current.MainWindow
//            };

//            _mapunitpolys.mapUnitPolysVM.RefreshMapUnitsAsync();

//            _mapunitpolys.Closed += (o, e) =>
//            {
//                // Reset the map tool to explore
//                FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");

//                _mapunitpolys = null;
//            };

//            _mapunitpolys.mapUnitPolysVM.WindowCloseEvent += (s, e) => _mapunitpolys.Close();

//            _mapunitpolys.Show();
//        }

//    }
//}
