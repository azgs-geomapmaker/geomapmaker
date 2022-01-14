using ArcGIS.Core.CIM;
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
        public ICommand CommandSubmit => new RelayCommand(() => SubmitAsync(), () => CanSubmit());

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
        }

        /// <summary>
        /// Determines the visibility (enabled state) of the button
        /// </summary>
        /// <returns>true if enabled</returns>
        private bool CanSubmit()
        {
            return Symbol != null &&
                !string.IsNullOrEmpty(IdentityConfidence) &&
                !string.IsNullOrEmpty(ExistenceConfidence) &&
                !string.IsNullOrEmpty(LocationConfidenceMeters) &&
                !string.IsNullOrEmpty(IsConcealedString) &&
                !string.IsNullOrEmpty(DataSource);
        }

        /// <summary>
        /// Execute the submit command
        /// </summary>
        private async Task SubmitAsync()
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
                CIMUniqueValueRenderer layerRenderer = layer.GetRenderer() as CIMUniqueValueRenderer;

                CIMUniqueValueGroup layerGroup = layerRenderer?.Groups?.FirstOrDefault();

                List<CIMUniqueValueClass> listUniqueValueClasses = layerGroup == null ? new List<CIMUniqueValueClass>() : new List<CIMUniqueValueClass>(layerGroup.Classes);

                // Template Fields
                List<string> Fields = new List<string> { "type", "symbol", "label", "identityconfidence", "existenceconfidence", "locationconfidencemeters", "isconcealed", "datasourceid" };
                // Template Values
                List<string> FieldValues = new List<string> { Symbol.Key, Symbol.Key, Symbol.Description.Substring(0, 50), IdentityConfidence, ExistenceConfidence, LocationConfidenceMeters, IsConcealedString, DataSource };

                // Add note if not null
                if (!string.IsNullOrEmpty(Notes))
                {
                    Fields.Add("notes");
                    FieldValues.Add(Notes);
                }

                CIMUniqueValue[] listUniqueValues = new CIMUniqueValue[] {
                    new CIMUniqueValue {
                        FieldValues = FieldValues.ToArray()
                    }
                };

                CIMUniqueValueClass uniqueValueClass = new CIMUniqueValueClass
                {
                    Editable = true,
                    Label = Symbol.Key,
                    Description = Symbol.Description,
                    Patch = PatchShape.AreaPolygon,
                    Symbol = CIMSymbolReference.FromJson(Symbol.SymbolJson, null),
                    Visible = true,
                    Values = listUniqueValues
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
            Notes = "Test123123";

            //Symbol = cf.Symbol;
            //IdentityConfidence = cf.IdentityConfidence;
            //ExistenceConfidence = cf.ExistenceConfidence;
            //LocationConfidenceMeters = cf.LocationConfidenceMeters;
            //IsConcealed = cf.IsConcealed;
            //Notes = cf.Notes;
            //DataSource = cf.DataSource;

            // Turn off the togge button
            Prepopulate = false;
        }

        private bool prepopulate;
        public bool Prepopulate
        {
            get => prepopulate;
            set
            {
                SetProperty(ref prepopulate, value, () => Prepopulate); //Have to do this to trigger stuff, I guess.

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

        private CFSymbol _symbol;
        public CFSymbol Symbol
        {
            get => _symbol;
            set => SetProperty(ref _symbol, value, () => Symbol);
        }

        private string _identityConfidence;
        public string IdentityConfidence
        {
            get => _identityConfidence;
            set => SetProperty(ref _identityConfidence, value, () => IdentityConfidence);
        }

        private string _existenceConfidence;
        public string ExistenceConfidence
        {
            get => _existenceConfidence;
            set => SetProperty(ref _existenceConfidence, value, () => ExistenceConfidence);
        }

        private string _locationConfidenceMeters;
        public string LocationConfidenceMeters
        {
            get => _locationConfidenceMeters;
            set => SetProperty(ref _locationConfidenceMeters, value, () => LocationConfidenceMeters);
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

        #endregion

    }
}
