﻿using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.MapUnitPolys
{
    public class EditMapUnitPolysVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public MapUnitPolysViewModel ParentVM { get; set; }

        public EditMapUnitPolysVM(MapUnitPolysViewModel parentVM)
        {
            ParentVM = parentVM;

            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "MapUnitPolys");

            if (layer == null)
            {
                return;
            }

            QueuedTask.Run(() =>
            {
                Selection mupSelection = layer.GetSelection();

                IReadOnlyList<long> Oids = mupSelection.GetObjectIDs();

                Set_MUP_Oids(Oids.ToList());
            });

            // Trigger validation
            Selected = Selected;
            IdentityConfidence = IdentityConfidence;
        }

        // Update Map Unit Polygons
        public ICommand CommandUpdate => new RelayCommand(() => UpdateAsync(), () => CanUpdate());

        // Reset Object ids
        public ICommand CommandClearOids => new RelayCommand(() => ClearOids(), () => MapUnitPolysOids != null && MapUnitPolysOids.Count > 0);

        private bool CanUpdate()
        {
            return MapUnitPolysOids.Count() > 0 && Selected != null && HasErrors == false;
        }

        private async void UpdateAsync()
        {
            bool editOperationSucceeded = false;

            FeatureLayer polyLayer = MapView.Active.Map.GetLayersAsFlattenedList().First((l) => l.Name == "MapUnitPolys") as FeatureLayer;

            await QueuedTask.Run(() =>
            {
                Table enterpriseTable = polyLayer.GetTable();

                EditOperation editOperation = new EditOperation();

                // Convert Dictionary Keys to a read-only list for queryFilter
                ReadOnlyCollection<long> oids = new ReadOnlyCollection<long>(MapUnitPolysOids.Keys.ToList());

                editOperation.Callback(context =>
                {
                    QueryFilter queryFilter = new QueryFilter
                    {
                        ObjectIDs = oids
                    };

                    using (RowCursor rowCursor = enterpriseTable.Search(queryFilter, false))
                    {
                        while (rowCursor.MoveNext())
                        {
                            using (Row row = rowCursor.Current)
                            {
                                // In order to update the Map and/or the attribute table.
                                // Has to be called before any changes are made to the row.
                                context.Invalidate(row);

                                row["MapUnit"] = Selected.MapUnit;
                                row["IdentityConfidence"] = IdentityConfidence;
                                row["Label"] = null;
                                row["Symbol"] = null;
                                row["Notes"] = Notes;
                                row["DataSourceID"] = DataSource;

                                // After all the changes are done, persist it.
                                row.Store();

                                // Has to be called after the store too.
                                context.Invalidate(row);
                            }
                        }
                    }
                }, enterpriseTable);

                editOperation.Execute();

                editOperationSucceeded = editOperation.IsSucceeded;
            });

            if (editOperationSucceeded)
            {
                ParentVM.CloseProwindow();
            }
        }

        private bool _toggleMupTool;
        public bool ToggleMupTool
        {
            get => _toggleMupTool;
            set
            {
                SetProperty(ref _toggleMupTool, value, () => ToggleMupTool);

                // if the toggle-btn is active
                if (value)
                {
                    // Active the mup tool
                    FrameworkApplication.SetCurrentToolAsync("Geomapmaker_SelectMapUnitPolysTool");
                }
                else
                {
                    // Switch back to map explore tool
                    FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
                }
            }
        }

        private void ClearOids()
        {
            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "MapUnitPolys");

            QueuedTask.Run(() =>
            {
                layer?.ClearSelection();
            });

            MapUnitPolysOids = new Dictionary<long, string>();
            NotifyPropertyChanged("OidsListBox");
        }

        private Dictionary<long, string> _mapUnitPolysOids { get; set; } = new Dictionary<long, string>();
        public Dictionary<long, string> MapUnitPolysOids
        {
            get => _mapUnitPolysOids;
            set
            {
                _mapUnitPolysOids = value;
                NotifyPropertyChanged("OidsListBox");
                NotifyPropertyChanged("MapUnitPolysOids");

                // Error is displayed on the selection
                ValidateMUPsOids(MapUnitPolysOids, "SelectedOid");
            }

        }

        // Collection for displaying in View
        public List<string> OidsListBox => MapUnitPolysOids.Select(a => $"{a.Value} ({a.Key})").ToList();

        // Error is displayed on the selection
        public string SelectedOid { get; set; }

        private MapUnitPolyTemplate _selected;
        public MapUnitPolyTemplate Selected
        {
            get => _selected;
            set
            {
                _selected = value;

                ValidateMapUnit(Selected, "Selected");

                // Trigger validation
                IdentityConfidence = IdentityConfidence;
            }
        }

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

        public async void Set_MUP_Oids(List<long> oids)
        {
            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "MapUnitPolys");

            if (layer == null)
            {
                return;
            }

            // Remove any errors from CF Lines
            _validationErrors.Remove("SelectedOid");
            RaiseErrorsChanged("SelectedOid");

            Inspector insp = new Inspector();

            await QueuedTask.Run(() =>
            {
                foreach (long id in oids)
                {
                    if (MapUnitPolysOids.ContainsKey(id))
                    {
                        MapUnitPolysOids.Remove(id);
                    }
                    else
                    {
                        insp.Load(layer, id);

                        string label = insp["mapunit"]?.ToString();

                        MapUnitPolysOids.Add(id, label);
                    }
                }
            });

            if (MapUnitPolysOids.Count == 0)
            {
                layer.ClearSelection();
            }
            else
            {
                QueryFilter queryFilter = new QueryFilter
                {
                    ObjectIDs = MapUnitPolysOids.Keys.ToList()
                };

                layer.Select(queryFilter);
            }

            NotifyPropertyChanged("OidsListBox");

            // Trigger Validation
            MapUnitPolysOids = MapUnitPolysOids;
        }

        #region Validation

        private readonly Dictionary<string, ICollection<string>> _validationErrors = new Dictionary<string, ICollection<string>>();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            // Return null if parameters is null/empty OR there are no errors for that parameter
            // Otherwise, return the errors for that parameter.
            return string.IsNullOrEmpty(propertyName) || !_validationErrors.ContainsKey(propertyName) ?
                null : (System.Collections.IEnumerable)_validationErrors[propertyName];
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

        private void ValidateMapUnit(MapUnitPolyTemplate selected, string propertyKey)
        {
            // Required field
            if (selected == null)
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            else
            {
                _validationErrors.Remove(propertyKey);
            }

            RaiseErrorsChanged(propertyKey);
        }

        private void ValidateMUPsOids(Dictionary<long, string> contactFaultOids, string propertyKey)
        {
            // Required field
            if (contactFaultOids == null || contactFaultOids.Count == 0)
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
}
