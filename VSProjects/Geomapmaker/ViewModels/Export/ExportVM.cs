using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
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

        public bool CreateGeodatabase { get; set; } = false;

        public bool CreateShapefiles { get; set; } = false;

        public bool CreateGeopackage { get; set; } = false;

        public bool CreateKml { get; set; } = false;

        public bool CreateTextTables { get; set; } = false;

        public bool CreateReport { get; set; } = false;

        public bool CreateOpen { get; set; } = true;

        public bool CreateSimple { get; set; } = false;

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
                string geodatabaseFolder = exportPath + $"\\Geodatabase";

                // Path for shapefiles
                string shapefileFolder = exportPath + "\\Shapefiles";

                // Path for geopackage
                string geopackageFolder = exportPath + "\\Geopackage";

                // Path for kml/kmz
                string kmlFolder = exportPath + "\\KML";

                // Path for text-tables
                string tablesFolder = exportPath + "\\Tables";

                // Path for the report
                string reportPath = exportPath + "\\Report.html";

                string openPath = exportPath + "\\Open";

                string simplePath = exportPath + "\\Simple";

                CloseProwindow();

                ProgressDialog progDialog = new ProgressDialog("Exporting Project");

                progDialog.Show();

                if (CreateGeodatabase)
                {
                    // Create shapefiles folder
                    System.IO.Directory.CreateDirectory(geodatabaseFolder);

                    // Path for the .gdb
                    string gdbPath = $"{geodatabaseFolder}\\{projectName}.gdb";

                    // Path for the FeatureDataset (inside the .gdb)
                    string featureDatasetPath = gdbPath + "\\GeologicMap";

                    // Get the maps spatial reference or default to WGS84
                    SpatialReference spatialReferences = MapView.Active?.Map?.SpatialReference ?? SpatialReferences.WGS84;

                    // Create a FileGeodatabaseConnectionPath with the name of the file geodatabase you wish to create
                    FileGeodatabaseConnectionPath fileGeodatabaseConnectionPath = new FileGeodatabaseConnectionPath(new Uri(gdbPath));

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
                        await Geoprocessing.ExecuteToolAsync("conversion.TableToGeodatabase", new List<string> { "DataSources", gdbPath });
                        await Geoprocessing.ExecuteToolAsync("conversion.TableToGeodatabase", new List<string> { "DescriptionOfMapUnits", gdbPath });
                        await Geoprocessing.ExecuteToolAsync("conversion.TableToGeodatabase", new List<string> { "GeoMaterialDict", gdbPath });
                        await Geoprocessing.ExecuteToolAsync("conversion.TableToGeodatabase", new List<string> { "Glossary", gdbPath });
                        await Geoprocessing.ExecuteToolAsync("conversion.TableToGeodatabase", new List<string> { "Symbology", gdbPath });
                    }
                }

                if (CreateShapefiles)
                {
                    // Create shapefiles folder
                    System.IO.Directory.CreateDirectory(shapefileFolder);

                    // FeatureClasses
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToShapefile", new List<string> { "ContactsAndFaults", shapefileFolder });
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToShapefile", new List<string> { "MapUnitPolys", shapefileFolder });
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToShapefile", new List<string> { "Stations", shapefileFolder });
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToShapefile", new List<string> { "OrientationPoints", shapefileFolder });
                }

                if (CreateGeopackage)
                {
                    // Create geopackage folder
                    System.IO.Directory.CreateDirectory(geopackageFolder);

                    // Path of the .gpkg file
                    string geopackagePath = $"{geopackageFolder}\\{projectName}.gpkg";

                    // Create geopackage
                    await Geoprocessing.ExecuteToolAsync("management.CreateSQLiteDatabase", new List<string> { geopackagePath, "GEOPACKAGE" });

                    // FeatureClasses
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToGeodatabase", new List<string> { "ContactsAndFaults", geopackagePath });
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToGeodatabase", new List<string> { "MapUnitPolys", geopackagePath });
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToGeodatabase", new List<string> { "Stations", geopackagePath });
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToGeodatabase", new List<string> { "OrientationPoints", geopackagePath });

                    // Tables
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToGeodatabase", new List<string> { "DataSources", geopackagePath });
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToGeodatabase", new List<string> { "DescriptionOfMapUnits", geopackagePath });
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToGeodatabase", new List<string> { "GeoMaterialDict", geopackagePath });
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToGeodatabase", new List<string> { "Glossary", geopackagePath });
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToGeodatabase", new List<string> { "Symbology", geopackagePath });
                }

                if (CreateKml)
                {
                    // Create kml folder
                    System.IO.Directory.CreateDirectory(kmlFolder);

                    // Path of the .kmz file
                    string kmzPath = $"{kmlFolder}\\{projectName}.kmz";

                    string mapName = MapView.Active?.Map?.Name;

                    await Geoprocessing.ExecuteToolAsync("conversion.MapToKML", new List<string> { mapName, kmzPath });
                }

                if (CreateTextTables)
                {
                    // Create tables folder
                    System.IO.Directory.CreateDirectory(tablesFolder);

                    // Tables
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "DataSources", tablesFolder, "DataSources.csv" }, null, null, null, GPExecuteToolFlags.None);
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "DescriptionOfMapUnits", tablesFolder, "DescriptionOfMapUnits.csv" }, null, null, null, GPExecuteToolFlags.None);
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "GeoMaterialDict", tablesFolder, "GeoMaterialDict.csv" }, null, null, null, GPExecuteToolFlags.None);
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "Glossary", tablesFolder, "Glossary.csv" }, null, null, null, GPExecuteToolFlags.None);
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "Symbology", tablesFolder, "Symbology.csv" }, null, null, null, GPExecuteToolFlags.None);
                }

                if (CreateReport)
                {
                    GemsReport report = new GemsReport();

                    report.BuildReport();

                    await report.ExportReportAsync(reportPath);
                }

                if (CreateOpen)
                {
                    // OPEN-- Consists of shapefiles, additional.dbf files, and pipe-delimited text files.
                    // Field renaming is documented in output file logfile.txt.
                    // This package will be a complete transcription of the geodatabase without loss of any information.

                    // Create open folder
                    System.IO.Directory.CreateDirectory(openPath);

                    // FeatureClasses shapefiles
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToShapefile", new List<string> { "ContactsAndFaults", openPath });
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToShapefile", new List<string> { "MapUnitPolys", openPath });
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToShapefile", new List<string> { "Stations", openPath });
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToShapefile", new List<string> { "OrientationPoints", openPath });

                    // dbf tables
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "DataSources", openPath, "DataSources.dbf" }, null, null, null, GPExecuteToolFlags.None);
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "DescriptionOfMapUnits", openPath, "DescriptionOfMapUnits.dbf" }, null, null, null, GPExecuteToolFlags.None);
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "GeoMaterialDict", openPath, "GeoMaterialDict.dbf" }, null, null, null, GPExecuteToolFlags.None);
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "Glossary", openPath, "Glossary.dbf" }, null, null, null, GPExecuteToolFlags.None);
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "Symbology", openPath, "Symbology.dbf" }, null, null, null, GPExecuteToolFlags.None);

                    // Pipe-delimited tables
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "DataSources", openPath, "DataSources.psv" }, null, null, null, GPExecuteToolFlags.None);
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "DescriptionOfMapUnits", openPath, "DescriptionOfMapUnits.psv" }, null, null, null, GPExecuteToolFlags.None);
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "GeoMaterialDict", openPath, "GeoMaterialDict.psv" }, null, null, null, GPExecuteToolFlags.None);
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "Glossary", openPath, "Glossary.psv" }, null, null, null, GPExecuteToolFlags.None);
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "Symbology", openPath, "Symbology.psv" }, null, null, null, GPExecuteToolFlags.None);
                }

                if (CreateSimple)
                {
                    // SIMPLE-- Consists of shapefiles alone. Tables Glossary, DataSources, and DescriptionOfMapUnits are joined to selected feature classes within feature dataset GeologicMap,
                    // long fields are truncated, and these feature classes are written to shapefiles.
                    // Field renaming is documented in output file logfile.txt.This package is a partial (incomplete)transcription of the geodatabase, but will be easier to use than the OPEN package. 

                    // Create simple folder
                    System.IO.Directory.CreateDirectory(simplePath);

                    // FeatureClasses shapefiles
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToShapefile", new List<string> { "ContactsAndFaults", simplePath });
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToShapefile", new List<string> { "MapUnitPolys", simplePath });
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToShapefile", new List<string> { "Stations", simplePath });
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToShapefile", new List<string> { "OrientationPoints", simplePath });

                }

                progDialog.Hide();
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
