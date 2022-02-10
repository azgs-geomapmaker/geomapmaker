using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Editing.Templates;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.MapUnitPolys
{
    public class MapUnitPolysViewModel : ProWindow, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        public ICommand CommandCancel => new RelayCommand((proWindow) =>
        {
            if (proWindow != null)
            {
                (proWindow as ProWindow).Close();
            }

        }, () => true);

        public ICommand CommandCreate => new RelayCommand(() => CreateAsync(), () => CanCreate());

        private bool CanCreate()
        {
            return ContactFaultOids.Count() > 0 && Selected != null && HasErrors == false;
        }

        private async void CreateAsync()
        {
            // Hide error if visible
            HasConstructError = false;
            NotifyPropertyChanged("ConstructionError");

            FeatureLayer cfLayer = MapView.Active.Map.GetLayersAsFlattenedList().First((l) => l.Name == "ContactsAndFaults") as FeatureLayer;

            EditOperation op = new EditOperation
            {
                Name = "Create MapUnitPolys"
            };

            FeatureLayer polyLayer = MapView.Active.Map.GetLayersAsFlattenedList().First((l) => l.Name == "MapUnitPolys") as FeatureLayer;

            EditingTemplate newTemplate;

            await QueuedTask.Run(() =>
            {

                Inspector insp = new Inspector();
                insp.LoadSchema(polyLayer);

                // set attributes
                insp["Symbol"] = Selected.Symbol;
                insp["MapUnit"] = Selected.MU;
                insp["IdentityConfidence"] = IdentityConfidence;
                insp["Notes"] = Notes;
                insp["DataSourceID"] = DataSource;

                newTemplate = polyLayer.CreateTemplate("Temporary", "Temporary", insp);

                op.ConstructPolygons(newTemplate, cfLayer, ContactFaultOids.Keys, null, false);

                try
                {
                    op.Execute();

                    if (op.IsSucceeded)
                    {
                        ContactFaultOids = new Dictionary<long, string>();
                        cfLayer.ClearSelection();
                    }
                }
                catch (Exception e)
                {
                    // Edit op failed
                    HasConstructError = true;
                    NotifyPropertyChanged("ConstructionError");
                }

                polyLayer.RemoveTemplate(newTemplate);
            });
        }

        public MapUnitPolysViewModel()
        {
            FrameworkApplication.SetCurrentToolAsync("Geomapmaker_MapUnitPolyTool");

            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "ContactsAndFaults");

            if (layer == null)
            {
                return;
            }

            QueuedTask.Run(() =>
            {
                Selection cfSelection = layer.GetSelection();

                IReadOnlyList<long> cfOids = cfSelection.GetObjectIDs();

                Set_CF_Oids(cfOids.ToList());
            });
        }

        // Collection of ID/Labels for selected CF lines
        public Dictionary<long, string> ContactFaultOids { get; set; } = new Dictionary<long, string>();

        // Collection for displaying in View
        public List<string> OidsListBox => ContactFaultOids.Select(a => $"{a.Key} ({a.Value})").ToList();

        private List<MapUnit> _mapUnits { get; set; }
        public List<MapUnit> MapUnits
        {
            get => _mapUnits;
            set
            {
                _mapUnits = value;
                NotifyPropertyChanged();
            }
        }

        private MapUnit _selected;
        public MapUnit Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                NotifyPropertyChanged("FieldVisibility");

                // Trigger validation
                IdentityConfidence = IdentityConfidence;
            }
        }

        public string FieldVisibility => Selected == null ? "Hidden" : "Visible";

        public bool HasConstructError = false;
        public string ConstructionError => HasConstructError ? "Visible" : "Hidden";

        private string _identityConfidence;
        public string IdentityConfidence
        {
            get => _identityConfidence;
            set
            {
                _identityConfidence = value;
                ValidateRequiredString(IdentityConfidence, "IdentityConfidence");
                NotifyPropertyChanged();
            }
        }

        private string _notes;
        public string Notes
        {
            get => _notes;
            set
            {
                _notes = value;
                NotifyPropertyChanged();
            }
        }

        private string _dataSource = GeomapmakerModule.DataSourceId;
        public string DataSource
        {
            get => _dataSource;
            set
            {
                _notes = value;
                NotifyPropertyChanged();
            }
        }

        public async void Set_CF_Oids(List<long> oids)
        {
            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "ContactsAndFaults");

            if (layer == null)
            {
                return;
            }

            Inspector insp = new Inspector();

            await QueuedTask.Run(() =>
            {
                foreach (long id in oids)
                {
                    if (ContactFaultOids.ContainsKey(id))
                    {
                        ContactFaultOids.Remove(id);
                    }
                    else
                    {
                        insp.Load(layer, id);

                        string label = insp["label"]?.ToString();

                        ContactFaultOids.Add(id, label);
                    }
                }
            });

            if (ContactFaultOids.Count == 0)
            {
                layer.ClearSelection();
            }
            else
            {
                QueryFilter queryFilter = new QueryFilter
                {
                    ObjectIDs = ContactFaultOids.Keys.ToList()
                };

                layer.Select(queryFilter);
            }

            NotifyPropertyChanged("OidsListBox");
        }

        public async void RefreshMapUnitsAsync()
        {
            List<MapUnit> mapUnitList = await Data.DescriptionOfMapUnits.GetMapUnitsAsync();

            MapUnits = mapUnitList.Where(a => a.ParagraphStyle == "Standard").OrderBy(a => a.Name).ToList();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Validation

        private readonly Dictionary<string, ICollection<string>> _validationErrors = new Dictionary<string, ICollection<string>>();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public IEnumerable GetErrors(string propertyName)
        {
            // Return null if parameters is null/empty OR there are no errors for that parameter
            // Otherwise, return the errors for that parameter.
            return string.IsNullOrEmpty(propertyName) || !_validationErrors.ContainsKey(propertyName) ?
                null : _validationErrors[propertyName];
        }

        public bool HasErrors => _validationErrors.Count > 0;

        private void ValidateRequiredString(string text, string propertyKey)
        {
            // Required field
            if (string.IsNullOrEmpty(text))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            else
            {
                _validationErrors.Remove(propertyKey);
            }

            RaiseErrorsChanged(propertyKey);
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
                Owner = FrameworkApplication.Current.MainWindow
            };

            _mapunitpolys.mapUnitPolysVM.RefreshMapUnitsAsync();

            _mapunitpolys.Closed += (o, e) =>
            {
                // Reset the map tool to explore
                FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");

                _mapunitpolys = null;
            };

            _mapunitpolys.Show();
        }

    }
}
