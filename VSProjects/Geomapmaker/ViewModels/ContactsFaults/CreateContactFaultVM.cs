using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Editing.Templates;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.ContactsFaults
{
    public class CreateContactFaultVM : PropertyChangedBase, INotifyDataErrorInfo
    {
        public ICommand CommandCreateTemplate => new RelayCommand(() => CreateTemplateAsync(), () => IsValidTemplate());

        public ICommand CommandSketch => new RelayCommand(async () => await CreateSketchAsync(), () => IsValidSketch());

        public ContactsFaultsViewModel ParentVM { get; set; }

        public CreateContactFaultVM(ContactsFaultsViewModel parentVM)
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

        /// <summary>
        /// Determines the visibility (enabled state) of the button
        /// </summary>
        /// <returns>true if enabled</returns>
        private bool IsValidTemplate()
        {
            return !HasErrors;
        }

        private bool IsValidSketch()
        {
            // Sketch does not require the label to be unique!

            // if label has any errors
            if (_validationErrors.ContainsKey("Label"))
            {
                // Get collection of label errors
                ICollection<string> labelErrors = _validationErrors["Label"];

                // Check if the required string error (an empty string) is present
                // Otherwise the error must be the uniqueness of the label
                return !labelErrors.Any(a => a == "");
            }
            else
            {
                return !HasErrors;
            }
        }

        private async Task CreateSketchAsync()
        {
            // Find the ContactsFaults layer
            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "ContactsAndFaults");

            if (layer == null)
            {
                MessageBox.Show("ContactsAndFaults layer not found in active map.");
                return;
            }

            ParentVM.CloseProwindow();

            await QueuedTask.Run(async () =>
            {
                IEnumerable<EditingTemplate> currentTemplates = layer.GetTemplates();

                if (currentTemplates.Any(a => a.Name == GeomapmakerModule.CF_SketchTemplateName))
                {
                    // Remove the temporary template
                    layer.RemoveTemplate(GeomapmakerModule.CF_SketchTemplateName);
                }

                if (currentTemplates.Any(a => a.Name == "ContactsAndFaults"))
                {
                    // Remove the default template
                    layer.RemoveTemplate("ContactsAndFaults");
                }

                //
                // Create the temp Template
                //

                // load the schema
                Inspector insp = new Inspector();
                insp.LoadSchema(layer);

                // set some default attributes
                insp["type"] = Type;
                insp["symbol"] = Symbol.Key;
                insp["label"] = Label;
                insp["identityconfidence"] = IdentityConfidence;
                insp["existenceconfidence"] = ExistenceConfidence;
                insp["locationconfidencemeters"] = LocationConfidenceMeters;
                insp["isconcealed"] = IsConcealedString;
                insp["datasourceid"] = DataSource;

                if (!string.IsNullOrEmpty(Notes))
                {
                    insp["notes"] = Notes;
                }

                // Create CIM template 
                EditingTemplate newTemplate = layer.CreateTemplate(GeomapmakerModule.CF_SketchTemplateName, GeomapmakerModule.CF_SketchTemplateName, insp);

                // Activate tool for the temp template
                await newTemplate.ActivateDefaultToolAsync();
            });

            // Update Renderer
            Data.ContactsAndFaults.AddSymbolToRenderer(Symbol.Key, Symbol.SymbolJson);
        }

        /// <summary>
        /// Create a new template
        /// </summary>
        private async Task CreateTemplateAsync()
        {
            // Find the ContactsFaults layer
            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "ContactsAndFaults");

            if (layer == null)
            {
                MessageBox.Show("ContactsAndFaults layer not found in active map.");
                return;
            }

            ParentVM.CloseProwindow();

            await QueuedTask.Run(async () =>
            {
                IEnumerable<EditingTemplate> currentTemplates = layer.GetTemplates();

                if (currentTemplates.Any(a => a.Name == "ContactsAndFaults"))
                {
                    // Remove the default template
                    layer.RemoveTemplate("ContactsAndFaults");
                }

                //
                // Create New Contact Fault Template
                //

                // load the schema
                Inspector insp = new Inspector();
                insp.LoadSchema(layer);

                // set some default attributes
                insp["type"] = Type;
                insp["symbol"] = Symbol.Key;
                insp["label"] = Label;
                insp["identityconfidence"] = IdentityConfidence;
                insp["existenceconfidence"] = ExistenceConfidence;
                insp["locationconfidencemeters"] = LocationConfidenceMeters;
                insp["isconcealed"] = IsConcealedString;
                insp["datasourceid"] = DataSource;

                if (!string.IsNullOrEmpty(Notes))
                {
                    insp["notes"] = Notes;
                }

                // Create CIM template 
                EditingTemplate newTemplate = layer.CreateTemplate(Label, Symbol.Description, insp);

                await newTemplate.ActivateDefaultToolAsync();
            });

            // Update Renderer
            Data.ContactsAndFaults.AddSymbolToRenderer(Symbol.Key, Symbol.SymbolJson);
        }

        public void PrepopulateCF(ContactFaultTemplate cf)
        {
            Label = cf.Label;
            Type = cf.Type;
            Symbol = ParentVM.SymbolOptions.FirstOrDefault(a => a.Key == cf.Symbol);
            IdentityConfidence = cf.IdentityConfidence;
            ExistenceConfidence = cf.ExistenceConfidence;
            LocationConfidenceMeters = cf.LocationConfidenceMeters;
            IsConcealed = cf.IsConcealed;
            Notes = cf.Notes;

            // Turn off the toggle button
            Prepopulate = false;
        }

        private bool _prepopulate;
        public bool Prepopulate
        {
            get => _prepopulate;
            set
            {
                SetProperty(ref _prepopulate, value, () => Prepopulate);

                // if the toggle-btn is active
                if (value)
                {
                    // Active the populate tool
                    FrameworkApplication.SetCurrentToolAsync("Geomapmaker_PopulateCFTool");
                }
                else
                {
                    // Switch back to map explore tool
                    FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
                }
            }
        }

        private string _type;
        public string Type
        {
            get => _type;
            set
            {
                SetProperty(ref _type, value, () => Type);
                ValidateRequiredString(Type, "Type");
            }
        }

        private string _label;
        public string Label
        {
            get => _label;
            set
            {
                SetProperty(ref _label, value, () => Label);
                ValidateLabel(Label, "Label");
            }
        }

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

            // Count of all symbols
            int totalSymbolsCount = filteredSymbols.Count();

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

            // Count of filtered symbols
            int filteredSymbolsCount = filteredSymbols.Count();

            // Update options
            SymbolOptions = filteredSymbols;

            // Update messsage
            SymbolsFilteredMessage = totalSymbolsCount != filteredSymbolsCount ? $"{filteredSymbolsCount} of {totalSymbolsCount} symbols" : $"{totalSymbolsCount} symbols";
        }

        private string _symbolsFilteredMessage;
        public string SymbolsFilteredMessage
        {
            get => _symbolsFilteredMessage;
            set => SetProperty(ref _symbolsFilteredMessage, value, () => SymbolsFilteredMessage);
        }

        private List<CFSymbol> _symbolOptions;
        public List<CFSymbol> SymbolOptions
        {
            get => _symbolOptions;
            set => SetProperty(ref _symbolOptions, value, () => SymbolOptions);
        }

        private CFSymbol _symbol;
        public CFSymbol Symbol
        {
            get => _symbol;
            set
            {
                SetProperty(ref _symbol, value, () => Symbol);
                ValidateSymbol(Symbol, "Symbol");
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
            }
        }

        private bool _isConcealed;
        public bool IsConcealed
        {
            get => _isConcealed;
            set => SetProperty(ref _isConcealed, value, () => IsConcealed);
        }

        // Convert bool to a Y/N char
        public string IsConcealedString => IsConcealed ? "Y" : "N";

        private string _notes;
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value, () => Notes);
        }

        private string _dataSource = GeomapmakerModule.DataSourceId;
        public string DataSource
        {
            get => _dataSource;
            set => SetProperty(ref _dataSource, value, () => DataSource);
        }

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

        private void ValidateLabel(string text, string propertyKey)
        {
            // Required field
            if (string.IsNullOrEmpty(text))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            else if (Label.ToLower() == GeomapmakerModule.CF_SketchTemplateName.ToLower())
            {
                _validationErrors[propertyKey] = new List<string>() { "Reserved template label." };
            }
            else if (ParentVM.Templates.Any(a => a.Label.ToLower() == Label.ToLower()))
            {
                _validationErrors[propertyKey] = new List<string>() { "Label is taken." };
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

        #endregion
    }
}
