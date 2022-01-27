using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Editing.Attributes;
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
    public class EditContactFaultVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ContactsFaultsViewModel ParentVM { get; set; }

        public ICommand CommandUpdate => new RelayCommand(() => UpdateTemplate(), () => IsValid());

        public EditContactFaultVM(ContactsFaultsViewModel parentVM)
        {
            ParentVM = parentVM;

            // Initialize required fields
            Type = "";
            Label = "";
            Symbol = null;
            IdentityConfidence = "";
            ExistenceConfidence = "";
            LocationConfidenceMeters = "";
        }

        private async void UpdateTemplate()
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
                layer.RemoveTemplate(originalValues.Label);

                // load the schema
                Inspector insp = new Inspector();
                insp.LoadSchema(layer);

                // set default attributes
                insp["type"] = Type;
                insp["symbol"] = Symbol.Key;
                insp["label"] = Label;
                insp["identityconfidence"] = IdentityConfidence;
                insp["existenceconfidence"] = ExistenceConfidence;
                insp["locationconfidencemeters"] = LocationConfidenceMeters;
                insp["isconcealed"] = IsConcealedString;
                insp["datasourceid"] = DataSource;

                // Optional fields
                if (!string.IsNullOrEmpty(Notes))
                {
                    insp["notes"] = Notes;
                }

                // Set up tags
                string[] tags = new[] { "ContactFault" };

                // Default construction tool - use daml-id
                string defaultTool = "esri_editing_LineConstructor";

                // TODO remove tools below 
                // filter - use daml-id
                List<string> filter = new List<string>();
                //filter.Add("esri_editing_ConstructPointsAlongLineCommand");

                // Create CIM template 
                EditingTemplate newTemplate = layer.CreateTemplate(Label, Symbol.Description, insp, defaultTool, tags, filter.ToArray());

                // Update Renderer
                await Data.CFSymbology.AddSymbolToRenderer(Symbol.Key, Symbol.SymbolJson);

                // Refresh list of templates
                ParentVM.RefreshTemplates();
            });
        }

        private bool IsValid()
        {
            return Selected != null && !HasErrors;
        }

        private EditingTemplate _selected;
        public EditingTemplate Selected
        {
            get => _selected;
            set
            {
                SetProperty(ref _selected, value, () => Selected);

                if (Selected != null)
                {
                    SetEditValues(Selected);
                }
                else
                {
                    // Reset original values if nothing is selected
                    originalValues = null;
                }
                NotifyPropertyChanged("Visibility");
            }
        }

        // Used to store the values of a selected template to determine if a change was made
        public ContactFault originalValues { get; set; }

        private async void SetEditValues(EditingTemplate template)
        {
            await QueuedTask.Run(() =>
            {
                CIMFeatureTemplate templateDef = template.GetDefinition() as CIMFeatureTemplate;

                // Clear out filters
                KeyFilter = "";
                DescriptionFilter = "";

                // Find the symbol
                Symbol = ParentVM.SymbolOptions.FirstOrDefault(a => a.Key == templateDef.DefaultValues["symbol"].ToString());

                // Set values from template for edit
                Type = templateDef.DefaultValues["type"].ToString();
                Label = templateDef.DefaultValues["label"].ToString();
                IdentityConfidence = templateDef.DefaultValues["identityconfidence"].ToString();
                ExistenceConfidence = templateDef.DefaultValues["existenceconfidence"].ToString();
                LocationConfidenceMeters = templateDef.DefaultValues["locationconfidencemeters"].ToString();
                IsConcealed = templateDef.DefaultValues["isconcealed"].ToString() == "Y";
                DataSource = templateDef.DefaultValues["datasourceid"].ToString();

                originalValues = new ContactFault()
                {
                    Type = templateDef.DefaultValues["type"].ToString(),
                    Label = templateDef.DefaultValues["label"].ToString(),
                    IdentityConfidence = templateDef.DefaultValues["identityconfidence"].ToString(),
                    ExistenceConfidence = templateDef.DefaultValues["existenceconfidence"].ToString(),
                    LocationConfidenceMeters = templateDef.DefaultValues["locationconfidencemeters"].ToString(),
                    IsConcealed = templateDef.DefaultValues["isconcealed"].ToString() == "Y",
                    DataSource = templateDef.DefaultValues["datasourceid"].ToString(),
                };

                // Notes is an optional field
                if (templateDef.DefaultValues.ContainsKey("notes"))
                {
                    Notes = templateDef.DefaultValues["notes"].ToString();
                }

            });

            ValidateChangeWasMade();
        }

        public string Visibility => Selected == null ? "Hidden" : "Visible";

        private string _type;
        public string Type
        {
            get => _type;
            set
            {
                SetProperty(ref _type, value, () => Type);
                ValidateRequiredString(Type, "Type");
                ValidateChangeWasMade();
            }
        }

        private string _label;
        public string Label
        {
            get => _label;
            set
            {
                SetProperty(ref _label, value, () => Label);
                ValidateRequiredString(Label, "Label");
                ValidateChangeWasMade();
            }
        }

        // Filter symbol options by key
        private string _keyFilter;
        public string KeyFilter
        {
            get => _keyFilter;
            set
            {
                SetProperty(ref _keyFilter, value, () => KeyFilter);
                FilterSymbols(KeyFilter, DescriptionFilter);
            }
        }

        // Filter symbol options by description
        private string _descriptionFilter;
        public string DescriptionFilter
        {
            get => _descriptionFilter;
            set
            {
                SetProperty(ref _descriptionFilter, value, () => DescriptionFilter);
                FilterSymbols(KeyFilter, DescriptionFilter);
            }
        }

        // Filter the Symbol options by key and description
        private void FilterSymbols(string keyFilter, string DescriptionFilter)
        {
            // Start with all the symbols from the parent vm
            List<CFSymbol> filteredSymbols = ParentVM.SymbolOptions;

            // Filter by key
            if (!string.IsNullOrEmpty(keyFilter))
            {
                filteredSymbols = filteredSymbols.Where(a => a.Key.StartsWith(keyFilter)).ToList();
            }

            // Filter by description
            if (!string.IsNullOrEmpty(DescriptionFilter))
            {
                filteredSymbols = filteredSymbols.Where(a => a.Description.Contains(DescriptionFilter)).ToList();
            }

            SymbolOptions = filteredSymbols;
        }

        private List<CFSymbol> _symbolOptions { get; set; }
        public List<CFSymbol> SymbolOptions
        {
            get => _symbolOptions;
            set
            {
                _symbolOptions = value;
                NotifyPropertyChanged();
            }
        }

        private CFSymbol _symbol;
        public CFSymbol Symbol
        {
            get => _symbol;
            set
            {
                SetProperty(ref _symbol, value, () => Symbol);
                ValidateSymbol(Symbol, "Symbol");
                ValidateChangeWasMade();
            }
        }

        private string _identityConfidence;
        public string IdentityConfidence
        {
            get => _identityConfidence;
            set
            {
                SetProperty(ref _identityConfidence, value, () => IdentityConfidence);
                ValidateRequiredString(IdentityConfidence, "IdentityConfidence");
                ValidateChangeWasMade();
            }
        }

        private string _existenceConfidence;
        public string ExistenceConfidence
        {
            get => _existenceConfidence;
            set
            {
                SetProperty(ref _existenceConfidence, value, () => ExistenceConfidence);
                ValidateRequiredString(ExistenceConfidence, "ExistenceConfidence");
                ValidateChangeWasMade();
            }
        }

        private string _locationConfidenceMeters;
        public string LocationConfidenceMeters
        {
            get => _locationConfidenceMeters;
            set
            {
                SetProperty(ref _locationConfidenceMeters, value, () => LocationConfidenceMeters);
                ValidateRequiredString(LocationConfidenceMeters, "LocationConfidenceMeters");
                ValidateChangeWasMade();
            }
        }

        private bool _isConcealed;
        public bool IsConcealed
        {
            get => _isConcealed;
            set
            {
                SetProperty(ref _isConcealed, value, () => IsConcealed);
                ValidateChangeWasMade();
            }
        }

        // Convert bool to a Y/N char
        public string IsConcealedString => IsConcealed ? "Y" : "N";

        private string _notes;
        public string Notes
        {
            get => _notes;
            set
            {
                SetProperty(ref _notes, value, () => Notes);
                ValidateChangeWasMade();
            }
        }

        private string _dataSource;
        public string DataSource
        {
            get => _dataSource;
            set
            {
                SetProperty(ref _dataSource, value, () => DataSource);
                ValidateChangeWasMade();
            }
        }

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

        // Validate symbol
        private void ValidateSymbol(CFSymbol symbol, string propertyKey)
        {
            // Required field
            if (symbol == null)
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            else
            {
                _validationErrors.Remove(propertyKey);
            }

            RaiseErrorsChanged(propertyKey);
        }

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

        private void ValidateChangeWasMade()
        {
            // Error message isn't display on a field. Just prevents user from hitting update until a change is made.
            const string propertyKey = "SilentError";

            if (Selected == null)
            {
                _validationErrors.Remove(propertyKey);
                return;
            }

            // Compare current values with original
            if (originalValues != null &&
                Type == originalValues.Type &&
                Label == originalValues.Label &&
                IdentityConfidence == originalValues.IdentityConfidence &&
                ExistenceConfidence == originalValues.ExistenceConfidence &&
                LocationConfidenceMeters == originalValues.LocationConfidenceMeters &&
                IsConcealed == originalValues.IsConcealed &&
                Notes == originalValues.Notes &&
                DataSource == originalValues.DataSource
            )
            {
                _validationErrors[propertyKey] = new List<string>() { "No changes have been made." };
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
