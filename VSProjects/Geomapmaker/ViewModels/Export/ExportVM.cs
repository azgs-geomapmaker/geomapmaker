using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.Export
{
    public class ExportVM : ProWindow, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        public event EventHandler WindowCloseEvent;

        public ICommand CommandCancel => new RelayCommand(() => CloseProwindow());

        public ICommand CommandExport => new RelayCommand(() => Export());

        public ExportVM()
        {

        }

        public void CloseProwindow()
        {
            WindowCloseEvent(this, new EventArgs());
        }

        public async void Export()
        {
            // Get the project name
            string projectName = Project.Current.Name;

            // Remove extension
            projectName = projectName.Replace(".aprx", "");

            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = projectName,
                DefaultExt = ".gdb",
                //Filter = "File geodatabase (.gdb)|*.gdb"
            };

            // Show save file dialog box
            bool? result = dialog.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                ProgressDialog progDialog = new ProgressDialog("Exporting Geodatabase");

                progDialog.Show();

                // Get the maps spatial reference or default to WGS84
                SpatialReference spatialReferences = MapView.Active?.Map?.SpatialReference ?? SpatialReferences.WGS84;

                // Save document
                string filename = dialog.FileName;

                // Create a FileGeodatabaseConnectionPath with the name of the file geodatabase you wish to create
                FileGeodatabaseConnectionPath fileGeodatabaseConnectionPath = new FileGeodatabaseConnectionPath(new Uri(filename));

                // Create and use the file geodatabase
                using (Geodatabase geodatabase = SchemaBuilder.CreateGeodatabase(fileGeodatabaseConnectionPath))
                {
                    SchemaBuilder schemaBuilder = new SchemaBuilder(geodatabase);

                    // Initialize a new FeatureDataset named as 'GeologicMap'
                    FeatureDatasetDescription geologicMapFeatureDataset = new FeatureDatasetDescription("GeologicMap", spatialReferences);

                    // Create the FeatureDataset
                    schemaBuilder.Create(geologicMapFeatureDataset);

                    // Build status
                    bool buildStatus = schemaBuilder.Build();

                    // Build errors
                    if (!buildStatus)
                    {
                        IReadOnlyList<string> errors = schemaBuilder.ErrorMessages;
                    }

                    StandaloneTable ds = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "DataSources");

                    if (ds != null)
                    {
                        IReadOnlyList<string> valueArray = Geoprocessing.MakeValueArray(ds, filename);

                        await Geoprocessing.ExecuteToolAsync("conversion.TableToGeodatabase", valueArray);
                    }

                    StandaloneTable dmu = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "DescriptionOfMapUnits");

                    if (dmu != null)
                    {
                        IReadOnlyList<string> valueArray = Geoprocessing.MakeValueArray(dmu, filename);

                        await Geoprocessing.ExecuteToolAsync("conversion.TableToGeodatabase", valueArray);
                    }

                    StandaloneTable geoDict = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "GeoMaterialDict");

                    if (geoDict != null)
                    {
                        IReadOnlyList<string> valueArray = Geoprocessing.MakeValueArray(geoDict, filename);

                        await Geoprocessing.ExecuteToolAsync("conversion.TableToGeodatabase", valueArray);
                    }

                    StandaloneTable glossary = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "Glossary");

                    if (glossary != null)
                    {
                        IReadOnlyList<string> valueArray = Geoprocessing.MakeValueArray(glossary, filename);

                        await Geoprocessing.ExecuteToolAsync("conversion.TableToGeodatabase", valueArray);
                    }

                    StandaloneTable symbology = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "Symbology");

                    if (symbology != null)
                    {
                        IReadOnlyList<string> valueArray = Geoprocessing.MakeValueArray(symbology, filename);

                        await Geoprocessing.ExecuteToolAsync("conversion.TableToGeodatabase", valueArray);
                    }
                }

                progDialog.Hide();

                CloseProwindow();
            }
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

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    internal class ShowExport : Button
    {
        private Views.Export.Export _export = null;

        protected override void OnClick()
        {
            //already open?
            if (_export != null)
            {
                _export.Close();
                return;
            }

            _export = new Views.Export.Export
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            _export.Closed += (o, e) =>
            {
                _export = null;
            };

            _export.exportVM.WindowCloseEvent += (s, e) => _export.Close();

            _export.Show();
        }
    }
}
