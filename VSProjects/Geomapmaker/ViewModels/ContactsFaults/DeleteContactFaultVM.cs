using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Editing.Templates;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.ContactsFaults
{
    public class DeleteContactFaultVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ContactsFaultsViewModel ParentVM { get; set; }

        public ICommand CommandDelete => new RelayCommand(() => DeleteTemplate(), () => IsValid());

        public DeleteContactFaultVM(ContactsFaultsViewModel parentVM)
        {
            ParentVM = parentVM;
        }

        private bool IsValid()
        {
            return Selected != null && !HasErrors;
        }

        private async void DeleteTemplate()
        {
            // Find the ContactsFaults layer
            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "ContactsAndFaults");

            if (layer == null)
            {
                MessageBox.Show("ContactsAndFaults layer not found in active map.");
                return;
            }

            await QueuedTask.Run(async () =>
            {
                IEnumerable<EditingTemplate> currentTemplates = layer.GetTemplates();

                // Remove the old template
                layer.RemoveTemplate(Label);

                // Refresh list of templates
                ParentVM.RefreshTemplates();
            });
        }

        public string Visibility => Selected == null ? "Hidden" : "Visible";

        private EditingTemplate _selected;
        public EditingTemplate Selected
        {
            get => _selected;
            set
            {
                SetProperty(ref _selected, value, () => Selected);

                if (Selected != null)
                {
                    SetTemplateValuesAsync(Selected);
                }

                NotifyPropertyChanged("Visibility");
            }
        }

        private async void SetTemplateValuesAsync(EditingTemplate template)
        {
            await QueuedTask.Run(() =>
            {
                CIMFeatureTemplate templateDef = template.GetDefinition() as CIMFeatureTemplate;

                // Find the symbol
                Symbol = ParentVM.SymbolOptions.FirstOrDefault(a => a.Key == templateDef.DefaultValues["symbol"].ToString());
                NotifyPropertyChanged("Symbol");

                // Set values from template for edit
                Type = templateDef.DefaultValues["type"].ToString();
                NotifyPropertyChanged("Type");

                Label = templateDef.DefaultValues["label"].ToString();
                NotifyPropertyChanged("Label");

                IdentityConfidence = templateDef.DefaultValues["identityconfidence"].ToString();
                NotifyPropertyChanged("IdentityConfidence");

                ExistenceConfidence = templateDef.DefaultValues["existenceconfidence"].ToString();
                NotifyPropertyChanged("ExistenceConfidence");

                LocationConfidenceMeters = templateDef.DefaultValues["locationconfidencemeters"].ToString();
                NotifyPropertyChanged("LocationConfidenceMeters");

                IsConcealedString = templateDef.DefaultValues["isconcealed"].ToString();
                NotifyPropertyChanged("IsConcealedString");

                DataSource = templateDef.DefaultValues["datasourceid"].ToString();
                NotifyPropertyChanged("DataSource");

                // Notes is an optional field
                if (templateDef.DefaultValues.ContainsKey("notes"))
                {
                    Notes = templateDef.DefaultValues["notes"].ToString();
                    NotifyPropertyChanged("Notes");
                }
            });
        }

        public string Type { get; set; }

        public string Label { get; set; }

        public CFSymbol Symbol { get; set; }
        
        public string IdentityConfidence { get; set; }
        
        public string ExistenceConfidence { get; set; }
        
        public string LocationConfidenceMeters { get; set; }
        
        public string IsConcealedString { get; set; }
        
        public string Notes { get; set; }
        
        public string DataSource { get; set; }

        #region ### Validation ####

        // Error collection
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

        #endregion

    }
}
