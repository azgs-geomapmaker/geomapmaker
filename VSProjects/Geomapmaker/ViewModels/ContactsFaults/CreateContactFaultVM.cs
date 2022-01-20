using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Editing.Templates;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System;
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
        public ICommand CommandCreateTemplate => new RelayCommand(() => CreateTemplateAsync(), () => IsValid());

        public ICommand CommandContactFaultTool => new RelayCommand(() => CreateContactFault());

        public ICommand CommandClose => new RelayCommand((proWindow) =>
        {
            if (proWindow != null)
            {
                (proWindow as ProWindow).Close();
            }

        }, () => true);

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
        private bool IsValid()
        {
            return Symbol != null &&
                !string.IsNullOrEmpty(IdentityConfidence) &&
                !string.IsNullOrEmpty(ExistenceConfidence) &&
                !string.IsNullOrEmpty(LocationConfidenceMeters) &&
                !string.IsNullOrEmpty(IsConcealedString) &&
                !string.IsNullOrEmpty(DataSource);
        }

        private void CreateContactFault()
        {
            FrameworkApplication.SetCurrentToolAsync("Geomapmaker_ContactFaultTool");
        }

        /// <summary>
        /// Execute the submit command
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

            await QueuedTask.Run(() =>
            {
                IEnumerable<EditingTemplate> currentTemplates = layer.GetTemplates();

                if (currentTemplates.Any(a => a.Name == Label))
                {
                    MessageBox.Show($"{Label} template already exists.");
                    return;
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

                // set up tags
                string[] tags = new[] { "ContactFault", "GeMS" };

                // default construction tool - use daml-id
                string defaultTool = "esri_editing_LineConstructor";

                // TODO remove tools below 
                // filter - use daml-id
                List<string> filter = new List<string>();
                //filter.Add("esri_editing_ConstructPointsAlongLineCommand");

                // Create CIM template 
                EditingTemplate newTemplate = layer.CreateTemplate(Label, Symbol.Description, insp, defaultTool, tags, filter.ToArray());

                //
                // Update Renderer
                //

                CIMUniqueValueRenderer layerRenderer = layer.GetRenderer() as CIMUniqueValueRenderer;

                CIMUniqueValueGroup layerGroup = layerRenderer?.Groups?.FirstOrDefault();

                List<CIMUniqueValueClass> listUniqueValueClasses = layerGroup == null ? new List<CIMUniqueValueClass>() : new List<CIMUniqueValueClass>(layerGroup.Classes);

                // Check if the renderer already has symbology for that key
                if (listUniqueValueClasses.Any(a => a.Label == Symbol.Key))
                {
                    return;
                }

                // Template Fields
                List<string> Fields = new List<string> { "symbol", };
                // Template Values
                List<string> FieldValues = new List<string> { Symbol.Key };

                CIMUniqueValue[] listUniqueValues = new CIMUniqueValue[] {
                    new CIMUniqueValue {
                        FieldValues = FieldValues.ToArray()
                    }
                };

                CIMUniqueValueClass uniqueValueClass = new CIMUniqueValueClass
                {
                    Editable = false,
                    Label = Symbol.Key,
                    Description = Symbol.Description,
                    Patch = PatchShape.AreaPolygon,
                    Symbol = CIMSymbolReference.FromJson(Symbol.SymbolJson, null),
                    Visible = true,
                    Values = listUniqueValues,
                };
                listUniqueValueClasses.Add(uniqueValueClass);

                CIMUniqueValueGroup uvg = new CIMUniqueValueGroup
                {
                    Classes = listUniqueValueClasses.ToArray(),
                };
                CIMUniqueValueGroup[] listUniqueValueGroups = new CIMUniqueValueGroup[] { uvg };

                CIMUniqueValueRenderer updatedRenderer = new CIMUniqueValueRenderer
                {
                    UseDefaultSymbol = false,
                    Groups = listUniqueValueGroups,
                    Fields = Fields.ToArray()
                };

                layer.SetRenderer(updatedRenderer);
            });
        }

        public void PrepopulateCF(ContactFault cf)
        {
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
                    FrameworkApplication.SetCurrentToolAsync("Geomapmaker_PopulateCFTool");
                }
                else
                {
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
                ValidateRequiredString(Label, "Label");
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

        #endregion

    }
}
