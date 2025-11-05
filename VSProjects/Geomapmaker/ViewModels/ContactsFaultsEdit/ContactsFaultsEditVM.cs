using ArcGIS.Core.Data;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Geomapmaker.ViewModels.ContactsFaultsEdit
{
    public class ContactsFaultsEditVM : ProWindow, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        public event EventHandler WindowCloseEvent;

        public ICommand CommandCancel => new RelayCommand(() => CloseProwindow());

        public ICommand CommandRefreshCFTemplates => new RelayCommand(() => RefreshCFTemplates());

        public async void CloseProwindow()
        {
            ClearOids();

            WindowCloseEvent(this, new EventArgs());
        }

        private List<ContactFaultTemplate> _cfTemplates { get; set; }
        public List<ContactFaultTemplate> CFTemplates
        {
            get => _cfTemplates;
            set
            {
                _cfTemplates = value;
                NotifyPropertyChanged();
            }
        }

        public async void RefreshCFTemplates()
        {
            //Data.MapUnitPolys.RebuildMUPSymbologyAndTemplates();
            CFTemplates = await Data.ContactsAndFaults.GetContactFaultTemplatesAsync();
        }

        public ContactsFaultsEditVM()
        {
            FeatureLayer layer = MapView.Active?.Map?.GetLayersAsFlattenedList()?.OfType<FeatureLayer>()?.FirstOrDefault(l => l.Name == "ContactsAndFaults");

            if (layer == null)
            {
                return;
            }

            QueuedTask.Run(() =>
            {
                Selection cfSelection = layer.GetSelection();

                IReadOnlyList<long> Oids = cfSelection.GetObjectIDs();

                Set_CF_Oids(Oids.ToList());
            });

            // Trigger validation
            Selected = Selected;
            //IdentityConfidence = IdentityConfidence;
        }

        // Update Map Unit Polygons
        public ICommand CommandUpdate => new RelayCommand(() => UpdateAsync(), () => CanUpdate());

        // Reset Object ids
        public ICommand CommandClearOids => new RelayCommand(() => ClearOids(), () => ContactsFaultsOids != null && ContactsFaultsOids.Count > 0);

        private bool CanUpdate()
        {
            return ContactsFaultsOids.Count() > 0 && Selected != null && HasErrors == false;
        }

        private async void UpdateAsync()
        {
            ProgressDialog progDialog = new ProgressDialog("Getting selected template");

            progDialog.Show();


            bool editOperationSucceeded = false;

            FeatureLayer cfLayer = MapView.Active?.Map?.GetLayersAsFlattenedList()?.First((l) => l.Name == "ContactsAndFaults") as FeatureLayer;

            await QueuedTask.Run(() =>
            {
                using (Table enterpriseTable = cfLayer.GetTable())
                {
                    if (enterpriseTable != null)
                    {
                        EditOperation editOperation = new EditOperation()
                        {
                            Name = "Edit ContactsAndFaults"
                        };

                        // Convert Dictionary Keys to a read-only list for queryFilter
                        ReadOnlyCollection<long> oids = new ReadOnlyCollection<long>(ContactsFaultsOids.Keys.ToList());

                        editOperation.Callback(context =>
                        {
                            ArcGIS.Core.Data.QueryFilter queryFilter = new ArcGIS.Core.Data.QueryFilter
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

                                        row["Label"] = Selected.Label;
                                        row["Type"] = Selected.Type;
                                        row["Symbol"] = Selected.Symbol;
                                        row["IsConcealed"] = Selected.IsConcealed;
                                        row["LocationConfidenceMeters"] = Selected.LocationConfidenceMeters;
                                        row["IdentityConfidence"] = Selected.IdentityConfidence;
                                        row["ExistenceConfidence"] = Selected.ExistenceConfidence;
                                        row["Notes"] = Notes != null ? Notes : Selected.Notes;
                                        row["DataSourceId"] = DataSource;


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
                    }
                }

                if (editOperationSucceeded) {
                    //MapView.Active?.Map?.ClearSelection();
                    //ContactsFaultsOids = new Dictionary<long, string>();
                    //CloseProwindow();
                }

            });

            progDialog.Hide();

        }

        private bool _toggleCFTool;
        public bool ToggleCFTool
        {
            get => _toggleCFTool;
            set
            {
                _toggleCFTool = value;

                // if the toggle-btn is active
                if (value)
                {
                    // Active the cf tool
                    FrameworkApplication.SetCurrentToolAsync("Geomapmaker_SelectContactsFaultsTool");
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
            FeatureLayer layer = MapView.Active?.Map?.GetLayersAsFlattenedList()?.OfType<FeatureLayer>()?.FirstOrDefault(l => l.Name == "ContactsAndFaults");

            QueuedTask.Run(() =>
            {
                layer?.ClearSelection();
            });

            ContactsFaultsOids = new Dictionary<long, string>();
            NotifyPropertyChanged("OidsListBox");
        }

        private Dictionary<long, string> _contactsFaultsOids { get; set; } = new Dictionary<long, string>();
        public Dictionary<long, string> ContactsFaultsOids
        {
            get => _contactsFaultsOids;
            set
            {
                _contactsFaultsOids = value;
                NotifyPropertyChanged("OidsListBox");
                NotifyPropertyChanged("ContactsFaultsOids");

                // Error is displayed on the selection
                ValidateCFsOids(ContactsFaultsOids, "SelectedOid");
            }

        }

        // Collection for displaying in View
        public List<string> OidsListBox => ContactsFaultsOids.Select(a => $"{a.Value} ({a.Key})").ToList();

        // Error is displayed on the selection
        public string SelectedOid { get; set; }

        private ContactFaultTemplate _selected;
        public ContactFaultTemplate Selected
        {
            get => _selected;
            set
            {
                _selected = value;

                ValidateCF(Selected, "Selected");

                // Trigger validation
                //IdentityConfidence = IdentityConfidence;
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
                _dataSource = value;
                NotifyPropertyChanged();
            }
        }

        public async void Set_CF_Oids(List<long> oids)
        {
            FeatureLayer layer = MapView.Active?.Map?.GetLayersAsFlattenedList()?.OfType<FeatureLayer>()?.FirstOrDefault(l => l.Name == "ContactsAndFaults");

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
                    if (ContactsFaultsOids.ContainsKey(id))
                    {
                        ContactsFaultsOids.Remove(id);
                    }
                    else
                    {
                        insp.Load(layer, id);

                        string label = insp["label"]?.ToString();

                        ContactsFaultsOids.Add(id, label);
                    }
                }
            });

            if (ContactsFaultsOids.Count == 0)
            {
                layer.ClearSelection();
            }
            else
            {
                ArcGIS.Core.Data.QueryFilter queryFilter = new ArcGIS.Core.Data.QueryFilter
                {
                    ObjectIDs = ContactsFaultsOids.Keys.ToList()
                };

                layer.Select(queryFilter);
            }

            NotifyPropertyChanged("OidsListBox");

            // Trigger Validation
            ContactsFaultsOids = ContactsFaultsOids;
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

        private void ValidateCF(ContactFaultTemplate selected, string propertyKey)
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

        private void ValidateCFsOids(Dictionary<long, string> contactFaultOids, string propertyKey)
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

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    internal class ShowContactsFaultsEdit : Button
    {
        private Views.ContactsFaultsEdit.ContactsFaultsEdit _contactsfaultsedit = null;

        protected override async void OnClick()
        {
            //already open?
            if (_contactsfaultsedit != null)
            {
                _contactsfaultsedit.Close();
                return;
            }

            _contactsfaultsedit = new Views.ContactsFaultsEdit.ContactsFaultsEdit
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            _contactsfaultsedit.Closed += (o, e) =>
            {
                // Reset the map tool to explore
                FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");

                _contactsfaultsedit = null;
            };

            _contactsfaultsedit.contactsFaultsEditVM.WindowCloseEvent += (s, e) => _contactsfaultsedit.Close();

            _contactsfaultsedit.Show();

            await QueuedTask.Run(async () =>
            {
                _contactsfaultsedit.contactsFaultsEditVM.CFTemplates = await Data.ContactsAndFaults.GetContactFaultTemplatesAsync();
            });
        }
    }
}
