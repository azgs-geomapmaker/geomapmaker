using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.MapUnitPolys
{
    public class EditMapUnitPolysVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public MapUnitPolysViewModel ParentVM { get; set; }

        public EditMapUnitPolysVM(MapUnitPolysViewModel parentVM)
        {
            ParentVM = parentVM;
        }

        // Update Map Unit Polygons
        public ICommand CommandUpdate => new RelayCommand(() => UpdateAsync(), () => CanUpdate());

        private bool CanUpdate()
        {
            return true;
        }

        private void UpdateAsync()
        {
            throw new NotImplementedException();
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
                    FrameworkApplication.SetCurrentToolAsync("Geomapmaker_SelectContactsFaultsTool");
                }
                else
                {
                    // Switch back to map explore tool
                    FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
                }
            }
        }

        private void ClearMups()
        {
 
        }

        private Dictionary<long, string> _mapUnitPolysOids { get; set; } = new Dictionary<long, string>();
        public Dictionary<long, string> MapUnitPolysOids
        {
            get => _mapUnitPolysOids;
            set
            {
                _mapUnitPolysOids = value;
                //NotifyPropertyChanged("OidsListBox");
                //NotifyPropertyChanged("ContactFaultOids");

                // Error is displayed on the selection
                //ValidateCFOids(ContactFaultOids, "SelectedOid");
            }

        }

        private MapUnitPolyTemplate _selected;
        public MapUnitPolyTemplate Selected
        {
            get => _selected;
            set
            {
                _selected = value;

                //ValidateMapUnit(Selected, "Selected");

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
                //ValidateRequiredString(IdentityConfidence, "IdentityConfidence");
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

        public void Set_MUP_Oids(List<long> oids)
        {
            //FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "ContactsAndFaults");

            //if (layer == null)
            //{
            //    return;
            //}

            //// Remove any errors from CF Lines
            //_validationErrors.Remove("SelectedOid");
            //RaiseErrorsChanged("SelectedOid");

            //Inspector insp = new Inspector();

            //await QueuedTask.Run(() =>
            //{
            //    foreach (long id in oids)
            //    {
            //        if (ContactFaultOids.ContainsKey(id))
            //        {
            //            ContactFaultOids.Remove(id);
            //        }
            //        else
            //        {
            //            insp.Load(layer, id);

            //            string label = insp["type"]?.ToString();

            //            ContactFaultOids.Add(id, label);
            //        }
            //    }
            //});

            //if (ContactFaultOids.Count == 0)
            //{
            //    layer.ClearSelection();
            //}
            //else
            //{
            //    QueryFilter queryFilter = new QueryFilter
            //    {
            //        ObjectIDs = ContactFaultOids.Keys.ToList()
            //    };

            //    layer.Select(queryFilter);
            //}

            //NotifyPropertyChanged("OidsListBox");

            //// Trigger Validation
            //ContactFaultOids = ContactFaultOids;
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

        private void ValidateCFOids(Dictionary<long, string> contactFaultOids, string propertyKey)
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
