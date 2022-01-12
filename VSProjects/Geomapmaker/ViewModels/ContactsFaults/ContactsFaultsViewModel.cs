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
                !string.IsNullOrEmpty(IsConcealedString);
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
                // Get the CIM layer definition
                CIMFeatureLayer layerDef = layer.GetDefinition() as CIMFeatureLayer;

                // Create a CIM template 
                CIMFeatureTemplate myTemplateDef = new CIMFeatureTemplate()
                {
                    Name = Symbol.Key,
                    Description = Symbol.Description,
                };

                myTemplateDef.WriteTags(new[] { "AZGS", "GeMS" });

                // Set default attributes
                myTemplateDef.DefaultValues = new Dictionary<string, object>
                {
                    { "Type", Symbol.Key },
                    { "Symbol", Symbol.Key },
                    { "IdentityConfidence", IdentityConfidence },
                    { "ExistenceConfidence", ExistenceConfidence },
                    { "LocationConfidenceMeters", LocationConfidenceMeters },
                    { "IsConcealed", IsConcealedString },
                    { "Notes", Notes },
                    { "DataSourceID", DataSource }
                };

                // Set the default construction tool
                myTemplateDef.SetDefaultToolDamlID("esri_editing_SketchPointTool");

                // Remove construction tools from being available with this template
                List<string> filter = new List<string>
                {
                    // esri_editing_ConstructPointsAlongLineCommand
                    "BCCF295A-9C64-4ADC-903E-62D827C10EF7"
                };

                myTemplateDef.ToolFilter = filter.ToArray();

                // Get all templates on this layer
                // NOTE - layerDef.FeatureTemplates could be null 
                //    if Create Features window hasn't been opened
                List<CIMEditingTemplate> layerTemplates = layerDef.FeatureTemplates?.ToList();

                if (layerTemplates == null)
                {
                    layerTemplates = new List<CIMEditingTemplate>();
                }

                // Add the new template to the layer template list
                layerTemplates.Add(myTemplateDef);

                // Update the layerdefinition with the templates
                layerDef.FeatureTemplates = layerTemplates.ToArray();

                // Check the AutoGenerateFeatureTemplates flag, 
                //     set to false so our changes will stick
                if (layerDef.AutoGenerateFeatureTemplates)
                    layerDef.AutoGenerateFeatureTemplates = false;

                // Commit
                layer.SetDefinition(layerDef);
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