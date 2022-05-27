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
using ArcGIS.Desktop.Framework.Dialogs;
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
                MessageBox.Show("ContactsAndFaults layer not found in active map.", "One or more errors occured.");
                return;
            }

            await QueuedTask.Run(() =>
            {
                IEnumerable<EditingTemplate> currentTemplates = layer.GetTemplates();

                // Remove the old template
                layer.RemoveTemplate(Label);

                // Refresh list of templates
                ParentVM.RefreshTemplates();
            });

            //Remove old symbology if needed.
            Data.ContactsAndFaults.RebuildContactsFaultsSymbology();

            ParentVM.CloseProwindow();
        }

        public string Visibility => Selected == null ? "Hidden" : "Visible";

        private ContactFaultTemplate _selected;
        public ContactFaultTemplate Selected
        {
            get => _selected;
            set
            {
                SetProperty(ref _selected, value, () => Selected);

                if (Selected != null)
                {
                    // Set values from template for edit
                    Type = Selected.Type;
                    NotifyPropertyChanged("Type");

                    Label = Selected.Label;
                    NotifyPropertyChanged("Label");

                    IdentityConfidence = Selected.IdentityConfidence;
                    NotifyPropertyChanged("IdentityConfidence");

                    ExistenceConfidence = Selected.ExistenceConfidence;
                    NotifyPropertyChanged("ExistenceConfidence");

                    LocationConfidenceMeters = Selected.LocationConfidenceMeters;
                    NotifyPropertyChanged("LocationConfidenceMeters");

                    IsConcealedString = Selected.IsConcealed ? "Y" : "N";
                    NotifyPropertyChanged("IsConcealedString");

                    Notes = Selected.Notes;
                    NotifyPropertyChanged("Notes");

                    DataSource = Selected.DataSource;
                    NotifyPropertyChanged("DataSource");

                    // Find CFSymbol from the key stored in symbol
                    Symbol = ParentVM.SymbolOptions.FirstOrDefault(a => a.Key == Selected.Symbol);
                    NotifyPropertyChanged("Symbol");
                }

                NotifyPropertyChanged("Visibility");
            }
        }

        public string Type { get; set; }

        public string Label { get; set; }

        public GemsSymbol Symbol { get; set; }

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
