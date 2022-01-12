using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.ContactsFaults
{
    public class ContactsFaultsViewModel : ProWindow, INotifyPropertyChanged
    {
        public ICommand CommandSubmit => new RelayCommand(() => SubmitAsync(), () => CanSubmit());

        public ICommand CommandClose => new RelayCommand((proWindow) =>
        {
            if (proWindow != null)
            {
                (proWindow as ProWindow).Close();
            }

        }, () => true);

        public ContactsFaultsViewModel()
        {

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
                string[] Fields = new string[] { "type", "symbol", "label", "identityconfidence", "existenceconfidence", "locationconfidencemeters", "isconcealed", "datasourceid" };
                // Template Values
                string[] FieldValues = new string[] { Symbol.Key, Symbol.Key, Symbol.Description.Substring(0, 50), IdentityConfidence, ExistenceConfidence, LocationConfidenceMeters, IsConcealedString, DataSource };

                // Add note if not null
                if (!string.IsNullOrEmpty(Notes))
                {
                    Fields.Append("notes");
                    FieldValues.Append(Notes);
                }

                CIMUniqueValue[] listUniqueValues = new CIMUniqueValue[] {
                    new CIMUniqueValue {
                        FieldValues = FieldValues
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
                    Fields = Fields
                };

                layer.SetRenderer(updatedRenderer);
            });
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
                _symbol = value;
                NotifyPropertyChanged();
            }
        }

        private string _identityConfidence;
        public string IdentityConfidence
        {
            get => _identityConfidence;
            set
            {
                _identityConfidence = value;
                NotifyPropertyChanged();
            }
        }

        private string _existenceConfidence;
        public string ExistenceConfidence
        {
            get => _existenceConfidence;
            set
            {
                _existenceConfidence = value;
                NotifyPropertyChanged();
            }
        }

        private string _locationConfidenceMeters;
        public string LocationConfidenceMeters
        {
            get => _locationConfidenceMeters;
            set
            {
                _locationConfidenceMeters = value;
                NotifyPropertyChanged();
            }
        }

        private bool _isConcealed;
        public bool IsConcealed
        {
            get => _isConcealed;
            set
            {
                _isConcealed = value;
                NotifyPropertyChanged();
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

        //Update collection of CF symbols
        public async void RefreshCFSymbols()
        {
            SymbolOptions = await Data.CFSymbolOptions.GetCFSymbolOptions();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    internal class ShowContactsFaults : Button
    {

        private Views.ContactsFaults.ContactsFaults _contactsfaults = null;

        protected override void OnClick()
        {
            //already open?
            if (_contactsfaults != null)
            {
                _contactsfaults.Close();
                return;
            }

            _contactsfaults = new Views.ContactsFaults.ContactsFaults
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            _contactsfaults.contactsFaultsVM.RefreshCFSymbols();

            _contactsfaults.Closed += (o, e) => { _contactsfaults = null; };

            _contactsfaults.Show();
        }
    }
}