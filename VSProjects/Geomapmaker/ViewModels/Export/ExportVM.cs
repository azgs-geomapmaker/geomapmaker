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

        public bool CreateShapefiles { get; set; } = true;

        public bool CreateTextTables { get; set; } = true;

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

                // Path for the FeatureDataset (inside the GDB)
                string featureDatasetPath = geodatabasePath + "\\GeologicMap";

                // Path for shapefiles
                string shapefilePath = exportPath + "\\Shapefiles";

                // Path for text-tables
                string tablesPath = exportPath + "\\Tables";

                // Path for the report
                string reportPath = exportPath + "\\Report.html";

                CloseProwindow();

                ProgressDialog progDialog = new ProgressDialog("Exporting Geodatabase");

                progDialog.Show();

                if (CreateGeodatabase)
                {
                    ExportGeodatabase(geodatabasePath, featureDatasetPath);
                }

                if (CreateShapefiles)
                {
                    // Create shapefiles folder
                    System.IO.Directory.CreateDirectory(shapefilePath);

                    // FeatureClasses
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToShapefile", new List<string> { "ContactsAndFaults", shapefilePath });
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToShapefile", new List<string> { "MapUnitPolys", shapefilePath });
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToShapefile", new List<string> { "Stations", shapefilePath });
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToShapefile", new List<string> { "OrientationPoints", shapefilePath });

                    // Tables
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToDBASE", new List<string> { "DataSources", shapefilePath });
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToDBASE", new List<string> { "DescriptionOfMapUnits", shapefilePath });
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToDBASE", new List<string> { "GeoMaterialDict", shapefilePath });
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToDBASE", new List<string> { "Glossary", shapefilePath });
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToDBASE", new List<string> { "Symbology", shapefilePath });
                }

                if (CreateTextTables)
                {
                    // Create tables folder
                    System.IO.Directory.CreateDirectory(tablesPath);

                    // Tables
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "DataSources", tablesPath, "DataSources.psv" });
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "DescriptionOfMapUnits", tablesPath, "DescriptionOfMapUnits.psv" });
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "GeoMaterialDict", tablesPath, "GeoMaterialDict.psv" });
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "Glossary", tablesPath, "Glossary.psv" });
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "Symbology", tablesPath, "Symbology.psv" });
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
                schemaBuilder.Build();

                // FeatureClasses
                await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToGeodatabase", new List<string> { "ContactsAndFaults", featureDatasetPath });
                await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToGeodatabase", new List<string> { "MapUnitPolys", featureDatasetPath });
                await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToGeodatabase", new List<string> { "Stations", featureDatasetPath });
                await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToGeodatabase", new List<string> { "OrientationPoints", featureDatasetPath });

                // Tables
                await Geoprocessing.ExecuteToolAsync("conversion.TableToGeodatabase", new List<string> { "DataSources", geodatabasePath });
                await Geoprocessing.ExecuteToolAsync("conversion.TableToGeodatabase", new List<string> { "DescriptionOfMapUnits", geodatabasePath });
                await Geoprocessing.ExecuteToolAsync("conversion.TableToGeodatabase", new List<string> { "GeoMaterialDict", geodatabasePath });
                await Geoprocessing.ExecuteToolAsync("conversion.TableToGeodatabase", new List<string> { "Glossary", geodatabasePath });
                await Geoprocessing.ExecuteToolAsync("conversion.TableToGeodatabase", new List<string> { "Symbology", geodatabasePath });
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
