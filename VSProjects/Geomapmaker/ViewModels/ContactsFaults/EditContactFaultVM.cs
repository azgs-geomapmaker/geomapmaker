﻿using ArcGIS.Desktop.Editing.Attributes;
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
            FeatureLayer layer = MapView.Active?.Map?.GetLayersAsFlattenedList()?.OfType<FeatureLayer>()?.FirstOrDefault(l => l.Name == "ContactsAndFaults");

            if (layer == null)
            {
                MessageBox.Show("ContactsAndFaults layer not found in active map.", "One or more errors occured.");
                return;
            }

            await QueuedTask.Run(() =>
            {
                IEnumerable<EditingTemplate> currentTemplates = layer.GetTemplates();

                // Remove the old template
                layer.RemoveTemplate(OriginalValues.Label);

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

                // Create CIM template 
                EditingTemplate newTemplate = layer.CreateTemplate(Label, Symbol.Description, insp);
            });

            // Add new symbology if needed. Remove old symbology if needed.
            Data.ContactsAndFaults.RebuildContactsFaultsSymbology();

            ParentVM.CloseProwindow();
        }

        private bool IsValid()
        {
            return Selected != null && !HasErrors;
        }

        private ContactFaultTemplate _selected;
        public ContactFaultTemplate Selected
        {
            get => _selected;
            set
            {
                SetProperty(ref _selected, value, () => Selected);

                if (Selected != null)
                {
                    // Clear out filters
                    KeyFilter = "";
                    DescriptionFilter = "";

                    // Collection of all the other templates. (Used to validate uniqueness)
                    AllOtherTemplates = ParentVM.Templates.Where(a => a.Label != Selected.Label).ToList();

                    // Set values from template for edit
                    Type = Selected.Type;
                    Label = Selected.Label;
                    IdentityConfidence = Selected.IdentityConfidence;
                    ExistenceConfidence = Selected.ExistenceConfidence;
                    LocationConfidenceMeters = Selected.LocationConfidenceMeters;
                    IsConcealed = Selected.IsConcealed;
                    Notes = Selected.Notes;
                    DataSource = GeomapmakerModule.DataSourceId;

                    // Find CFSymbol from the key stored in symbol
                    Symbol = ParentVM.SymbolOptions.FirstOrDefault(a => a.Key == Selected.Symbol);

                    // Keep a copy of the original values
                    OriginalValues = Selected;

                    // Trigger validation
                    ValidateChangeWasMade();
                }
                else
                {
                    // Reset original values if nothing is selected
                    OriginalValues = null;

                    // Reset collection
                    AllOtherTemplates = new List<ContactFaultTemplate>();
                }

                NotifyPropertyChanged("Visibility");
            }
        }

        // Used to store the values of a selected template to determine if a change was made
        public ContactFaultTemplate OriginalValues { get; set; }

        // All the templates, except the one we are editting
        public List<ContactFaultTemplate> AllOtherTemplates { get; set; } = new List<ContactFaultTemplate>();

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
                ValidateLabel(Label, "Label");
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
            List<GemsSymbol> filteredSymbols = ParentVM.SymbolOptions;

            // Count of all symbols
            int totalSymbolsCount = filteredSymbols.Count();

            // Filter by key
            if (!string.IsNullOrEmpty(keyFilter))
            {
                filteredSymbols = filteredSymbols.Where(a => a.Key.ToLower().StartsWith(keyFilter.ToLower())).ToList();
            }

            // Filter by description
            if (!string.IsNullOrEmpty(DescriptionFilter))
            {
                filteredSymbols = filteredSymbols.Where(a => a.Description != null && a.Description.ToLower().Contains(DescriptionFilter.ToLower())).ToList();
            }

            // Count of filtered symbols
            int filteredSymbolsCount = filteredSymbols.Count();

            // Update options
            SymbolOptions = filteredSymbols;

            string plural = totalSymbolsCount == 1 ? "" : "s";

            // Update messsage
            SymbolsFilteredMessage = totalSymbolsCount != filteredSymbolsCount ? $"{filteredSymbolsCount} of {totalSymbolsCount} symbol{plural}" : $"{totalSymbolsCount} symbol{plural}";
        }

        private string _symbolsFilteredMessage;
        public string SymbolsFilteredMessage
        {
            get => _symbolsFilteredMessage;
            set => SetProperty(ref _symbolsFilteredMessage, value, () => SymbolsFilteredMessage);
        }

        private List<GemsSymbol> _symbolOptions;
        public List<GemsSymbol> SymbolOptions
        {
            get => _symbolOptions;
            set => SetProperty(ref _symbolOptions, value, () => SymbolOptions);
        }

        private GemsSymbol _symbol;
        public GemsSymbol Symbol
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
                ValidateRequiredNumber(LocationConfidenceMeters, "LocationConfidenceMeters");
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
        private void ValidateSymbol(GemsSymbol symbol, string propertyKey)
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
            else if (AllOtherTemplates.Any(a => a.Label.ToLower() == Label.ToLower()))
            {
                _validationErrors[propertyKey] = new List<string>() { "Label is taken." };
            }
            else
            {
                _validationErrors.Remove(propertyKey);
            }

            RaiseErrorsChanged(propertyKey);
        }

        private void ValidateRequiredNumber(string text, string propertyKey)
        {
            // Required field
            if (string.IsNullOrEmpty(text))
            {
                _validationErrors[propertyKey] = new List<string>() { "" };
            }
            else if (!double.TryParse(text, out _))
            {
                _validationErrors[propertyKey] = new List<string>() { "Value must be numerical." };
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
            if (OriginalValues != null &&
                Type == OriginalValues.Type &&
                Label == OriginalValues.Label &&
                Symbol?.Key == OriginalValues.Symbol &&
                IdentityConfidence == OriginalValues.IdentityConfidence &&
                ExistenceConfidence == OriginalValues.ExistenceConfidence &&
                LocationConfidenceMeters == OriginalValues.LocationConfidenceMeters &&
                IsConcealed == OriginalValues.IsConcealed &&
                Notes == OriginalValues.Notes &&
                DataSource == OriginalValues.DataSource
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
