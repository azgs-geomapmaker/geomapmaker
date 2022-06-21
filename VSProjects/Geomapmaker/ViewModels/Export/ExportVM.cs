using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Report;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.Export
{
    public class ExportVM : ProWindow, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        public event EventHandler WindowCloseEvent;

        public ICommand CommandCancel => new RelayCommand(() => CloseProwindow());

        public ICommand CommandExport => new RelayCommand(() => Export());

        public bool CreateGeodatabase { get; set; } = true;

        public bool CreateReport { get; set; } = true;

        public void CloseProwindow()
        {
            WindowCloseEvent(this, new EventArgs());
        }

        public async void Export()
        {
            FolderBrowserDialog folderPrompt = new FolderBrowserDialog();

            if (folderPrompt.ShowDialog() == System.Windows.Forms.DialogResult.OK) 
            {
                // Export folder path from user
                string exportPath = folderPrompt.SelectedPath;

                // Get the project name
                string projectName = _helpers.Helpers.GetProjectName();

                // Path for the new GDB file
                string geodatabasePath = exportPath + $"\\{projectName}.gdb";

                // Path for the FeatureDataset
                string featureDatasetPath = geodatabasePath + "\\GeologicMap";

                // Path for the report
                string reportPath = exportPath + "\\Report.html";

                CloseProwindow();

                ProgressDialog progDialog = new ProgressDialog("Exporting Geodatabase");

                progDialog.Show();

                if (CreateGeodatabase)
                {
                    ExportGeodatabase(geodatabasePath, featureDatasetPath);
                }

                if (CreateReport)
                {
                    GemsReport report = new GemsReport();

                    report.BuildReport();

                    await report.ExportReportAsync(reportPath);
                }

                progDialog.Hide();

            }
        }

        private async void ExportGeodatabase(string geodatabasePath, string featureDatasetPath)
        {
            // Get the maps spatial reference or default to WGS84
            SpatialReference spatialReferences = MapView.Active?.Map?.SpatialReference ?? SpatialReferences.WGS84;

            // Create a FileGeodatabaseConnectionPath with the name of the file geodatabase you wish to create
            FileGeodatabaseConnectionPath fileGeodatabaseConnectionPath = new FileGeodatabaseConnectionPath(new Uri(geodatabasePath));

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

                FeatureLayer cf = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "ContactsAndFaults");

                if (cf != null)
                {
                    IReadOnlyList<string> valueArray = Geoprocessing.MakeValueArray(cf, featureDatasetPath);

                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToGeodatabase", valueArray);
                }

                FeatureLayer mup = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "MapUnitPolys");

                if (mup != null)
                {
                    IReadOnlyList<string> valueArray = Geoprocessing.MakeValueArray(mup, featureDatasetPath);

                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToGeodatabase", valueArray);
                }

                FeatureLayer stations = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "Stations");

                if (stations != null)
                {
                    IReadOnlyList<string> valueArray = Geoprocessing.MakeValueArray(stations, featureDatasetPath);

                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToGeodatabase", valueArray);
                }

                FeatureLayer op = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "OrientationPoints");

                if (op != null)
                {
                    IReadOnlyList<string> valueArray = Geoprocessing.MakeValueArray(op, featureDatasetPath);

                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToGeodatabase", valueArray);
                }

                StandaloneTable ds = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "DataSources");

                if (ds != null)
                {
                    IReadOnlyList<string> valueArray = Geoprocessing.MakeValueArray(ds, geodatabasePath);

                    await Geoprocessing.ExecuteToolAsync("conversion.TableToGeodatabase", valueArray);
                }

                StandaloneTable dmu = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "DescriptionOfMapUnits");

                if (dmu != null)
                {
                    IReadOnlyList<string> valueArray = Geoprocessing.MakeValueArray(dmu, geodatabasePath);

                    await Geoprocessing.ExecuteToolAsync("conversion.TableToGeodatabase", valueArray);
                }

                StandaloneTable geoDict = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "GeoMaterialDict");

                if (geoDict != null)
                {
                    IReadOnlyList<string> valueArray = Geoprocessing.MakeValueArray(geoDict, geodatabasePath);

                    await Geoprocessing.ExecuteToolAsync("conversion.TableToGeodatabase", valueArray);
                }

                StandaloneTable glossary = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "Glossary");

                if (glossary != null)
                {
                    IReadOnlyList<string> valueArray = Geoprocessing.MakeValueArray(glossary, geodatabasePath);

                    await Geoprocessing.ExecuteToolAsync("conversion.TableToGeodatabase", valueArray);
                }

                StandaloneTable symbology = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == "Symbology");

                if (symbology != null)
                {
                    IReadOnlyList<string> valueArray = Geoprocessing.MakeValueArray(symbology, geodatabasePath);

                    await Geoprocessing.ExecuteToolAsync("conversion.TableToGeodatabase", valueArray);
                }
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

    internal class ShowExport : ArcGIS.Desktop.Framework.Contracts.Button
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
