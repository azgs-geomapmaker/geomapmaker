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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using FieldDescription = ArcGIS.Desktop.Mapping.FieldDescription;

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

        public bool CreateCsv { get; set; } = false;

        public bool CreateReport { get; set; } = false;

        public bool CreateOpen { get; set; } = false;

        public bool CreateSimple { get; set; } = true;

        public void CloseProwindow()
        {
            WindowCloseEvent(this, new EventArgs());
        }

        public async void Export()
        {
            FolderBrowserDialog folderPrompt = new FolderBrowserDialog();

            if (folderPrompt.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                await QueuedTask.Run(() =>
                {
                    // Clear selections so everything is exported
                    MapView.Active?.Map?.SetSelection(null);
                });

                // Export folder path from user
                string exportPath = folderPrompt.SelectedPath;

                // Get the project name
                string projectName = _helpers.Helpers.GetProjectName();

                CloseProwindow();

                ProgressDialog progDialog = new ProgressDialog("Exporting Project");

                progDialog.Show();

                if (CreateGeodatabase)
                {
                    // Path for the new GDB file
                    string geodatabaseFolder = Path.Combine(exportPath, "Geodatabase");

                    // Create shapefiles folder
                    Directory.CreateDirectory(geodatabaseFolder);

                    // Path for the .gdb
                    string gdbPath = Path.Combine(geodatabaseFolder, $"{projectName}.gdb");

                    // Path for the FeatureDataset (inside the .gdb)
                    string featureDatasetPath = Path.Combine(gdbPath, "GeologicMap");

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
                    // Path for shapefiles
                    string shapefileFolder = Path.Combine(exportPath, "Shapefiles");

                    // Create shapefiles folder
                    Directory.CreateDirectory(shapefileFolder);

                    // FeatureClasses
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToShapefile", new List<string> { "ContactsAndFaults", shapefileFolder });
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToShapefile", new List<string> { "MapUnitPolys", shapefileFolder });
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToShapefile", new List<string> { "Stations", shapefileFolder });
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToShapefile", new List<string> { "OrientationPoints", shapefileFolder });
                }

                if (CreateGeopackage)
                {
                    // Path for geopackage
                    string geopackageFolder = Path.Combine(exportPath, "Geopackage");

                    // Create geopackage folder
                    Directory.CreateDirectory(geopackageFolder);

                    // Path of the .gpkg file
                    string geopackagePath = Path.Combine(geopackageFolder, $"{projectName}.gpkg");

                    // Create geopackage
                    await Geoprocessing.ExecuteToolAsync("management.CreateSQLiteDatabase", new List<string> { geopackagePath, "GEOPACKAGE" });

                    // Features
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
                    // Path for kml/kmz
                    string kmlFolder = Path.Combine(exportPath, "KML");

                    // Create kml folder
                    Directory.CreateDirectory(kmlFolder);

                    // Path of the .kmz file
                    string kmzPath = Path.Combine(kmlFolder, $"{projectName}.kmz");

                    string mapName = MapView.Active?.Map?.Name;

                    await Geoprocessing.ExecuteToolAsync("conversion.MapToKML", new List<string> { mapName, kmzPath });
                }

                if (CreateCsv)
                {
                    // Path for text-tables
                    string csvFolder = Path.Combine(exportPath, "CSV");

                    // Create tables folder
                    Directory.CreateDirectory(csvFolder);

                    // Features 
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "ContactsAndFaults", csvFolder, "ContactsAndFaults.csv" }, null, null, null, GPExecuteToolFlags.None);
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "MapUnitPolys", csvFolder, "MapUnitPolys.csv" }, null, null, null, GPExecuteToolFlags.None);
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "Stations", csvFolder, "Stations.csv" }, null, null, null, GPExecuteToolFlags.None);
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "OrientationPoints", csvFolder, "OrientationPoints.csv" }, null, null, null, GPExecuteToolFlags.None);

                    // Tables
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "DataSources", csvFolder, "DataSources.csv" }, null, null, null, GPExecuteToolFlags.None);
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "DescriptionOfMapUnits", csvFolder, "DescriptionOfMapUnits.csv" }, null, null, null, GPExecuteToolFlags.None);
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "GeoMaterialDict", csvFolder, "GeoMaterialDict.csv" }, null, null, null, GPExecuteToolFlags.None);
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "Glossary", csvFolder, "Glossary.csv" }, null, null, null, GPExecuteToolFlags.None);
                    await Geoprocessing.ExecuteToolAsync("conversion.TableToTable", new List<string> { "Symbology", csvFolder, "Symbology.csv" }, null, null, null, GPExecuteToolFlags.None);
                }

                if (CreateReport)
                {
                    // Path for the report
                    string reportPath = Path.Combine(exportPath, "Report.html");

                    GemsReport report = new GemsReport();

                    report.BuildReport();

                    await report.ExportReportAsync(reportPath);
                }

                if (CreateOpen)
                {
                    // OPEN-- Consists of shapefiles, additional.dbf files, and pipe-delimited text files.
                    // Field renaming is documented in output file logfile.txt.
                    // This package will be a complete transcription of the geodatabase without loss of any information.

                    // Path for "Open" export
                    string openPath = Path.Combine(exportPath, "Open");

                    // Create open folder
                    Directory.CreateDirectory(openPath);

                    string logFile = Path.Combine(openPath, "logfile.txt");

                    File.WriteAllText(logFile, $"Geomapmaker Open Export: {DateTime.Today:d}" + Environment.NewLine + Environment.NewLine);

                    File.AppendAllLines(logFile, new string[] { "Field Remapping", "Original_Field => Shapefile_Field" });

                    // FeatureClasses shapefiles
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToShapefile", new List<string> { "ContactsAndFaults", openPath });
                    await WriteShapefileLog("ContactsAndFaults", openPath, logFile);

                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToShapefile", new List<string> { "MapUnitPolys", openPath });
                    await WriteShapefileLog("MapUnitPolys", openPath, logFile);

                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToShapefile", new List<string> { "OrientationPoints", openPath });
                    await WriteShapefileLog("OrientationPoints", openPath, logFile);

                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToShapefile", new List<string> { "Stations", openPath });
                    await WriteShapefileLog("Stations", openPath, logFile);

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

                    // Path for "Simple" export
                    string simplePath = Path.Combine(exportPath, "Simple");

                    // Create simple folder
                    Directory.CreateDirectory(simplePath);

                    string logFile = Path.Combine(simplePath, "logfile.txt");

                    File.WriteAllText(logFile, $"Geomapmaker Simple Export: {DateTime.Today:d}" + Environment.NewLine + Environment.NewLine);

                    File.AppendAllLines(logFile, new string[] { "Field Remapping", "Original_Field => Shapefile_Field" });

                    // https://community.esri.com/t5/arcgis-pro-ideas/remove-all-joins-programatically/idi-p/974557
                    // In the 2.9 SDK, calling RemoveJoin without specifying the name of the join  removes the last join added.
                    // This has supposedly been fixed in 3.0 so we won't have to call RemoveJoin for every join added. 

                    // ContactsAndFaults
                    await Geoprocessing.ExecuteToolAsync("management.AddJoin", new List<string> { "ContactsAndFaults", "datasourceid", "DataSources", "datasources_id" });
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToFeatureClass", new List<string> { "ContactsAndFaults", simplePath, "ContactsAndFaults" }, null, null, null, GPExecuteToolFlags.None);
                    await WriteShapefileLog("ContactsAndFaults", simplePath, logFile);
                    await Geoprocessing.ExecuteToolAsync("management.RemoveJoin", new List<string> { "ContactsAndFaults" });

                    // MapUnitPolys
                    await Geoprocessing.ExecuteToolAsync("management.AddJoin", new List<string> { "MapUnitPolys", "mapunit", "DescriptionOfMapUnits", "mapunit" });
                    await Geoprocessing.ExecuteToolAsync("management.AddJoin", new List<string> { "MapUnitPolys", "geomaterial", "GeoMaterialDict", "indentedname" });
                    await Geoprocessing.ExecuteToolAsync("management.AddJoin", new List<string> { "MapUnitPolys", "datasourceid", "DataSources", "datasources_id" });
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToFeatureClass", new List<string> { "MapUnitPolys", simplePath, "MapUnitPolys" }, null, null, null, GPExecuteToolFlags.None);
                    await WriteShapefileLog("MapUnitPolys", simplePath, logFile);
                    await Geoprocessing.ExecuteToolAsync("management.RemoveJoin", new List<string> { "MapUnitPolys" });
                    await Geoprocessing.ExecuteToolAsync("management.RemoveJoin", new List<string> { "MapUnitPolys" });
                    await Geoprocessing.ExecuteToolAsync("management.RemoveJoin", new List<string> { "MapUnitPolys" });

                    // OrientationPoints
                    await Geoprocessing.ExecuteToolAsync("management.AddJoin", new List<string> { "OrientationPoints", "datasourceid", "DataSources", "datasources_id" });
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToFeatureClass", new List<string> { "OrientationPoints", simplePath, "OrientationPoints" }, null, null, null, GPExecuteToolFlags.None);
                    await WriteShapefileLog("OrientationPoints", simplePath, logFile);
                    await Geoprocessing.ExecuteToolAsync("management.RemoveJoin", new List<string> { "OrientationPoints" });

                    // Stations
                    await Geoprocessing.ExecuteToolAsync("management.AddJoin", new List<string> { "Stations", "datasourceid", "DataSources", "datasources_id" });
                    await Geoprocessing.ExecuteToolAsync("conversion.FeatureClassToFeatureClass", new List<string> { "Stations", simplePath, "Stations" }, null, null, null, GPExecuteToolFlags.None);
                    await WriteShapefileLog("Stations", simplePath, logFile);
                    await Geoprocessing.ExecuteToolAsync("management.RemoveJoin", new List<string> { "Stations" });
                }

                progDialog.Hide();
            }
        }

        private async Task WriteShapefileLog(string datasetName, string exportPath, string logfilePath)
        {
            List<FieldDescription> oldFields = new List<FieldDescription>();
            List<Field> newFields = new List<Field>();

            // Find the original feature class
            FeatureLayer feature = MapView.Active?.Map.FindLayers(datasetName).FirstOrDefault() as FeatureLayer;

            await QueuedTask.Run(() =>
            {
                // Get the original fields
                oldFields = feature.GetFieldDescriptions();

                // Exporting to shapefile always seems to move the shape field to the second position
                FieldDescription shapeField = oldFields.FirstOrDefault(a => a.Alias.ToLower() == "shape");
                if (shapeField != null)
                {
                    oldFields.Remove(shapeField);
                    oldFields.Insert(1, shapeField);
                }

                // Read the shapefile
                FileSystemConnectionPath fileConnection = new FileSystemConnectionPath(new Uri(exportPath), FileSystemDatastoreType.Shapefile);
                using (FileSystemDatastore shapefile = new FileSystemDatastore(fileConnection))
                {
                    FeatureClassDefinition featureClassDef = shapefile.GetDefinition<FeatureClassDefinition>(datasetName);

                    // Get the new fields
                    newFields = featureClassDef?.GetFields()?.ToList();
                }
            });

            using (StreamWriter file = File.AppendText(logfilePath))
            {
                file.WriteLine(Environment.NewLine + datasetName.ToUpper());

                // Loop over both sets of fields
                for (int i = 0; i < oldFields.Count && i < newFields.Count; i++)
                {
                    // Append to textfile
                    file.WriteLine($"{oldFields[i].Alias} => {newFields[i].Name}");
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

            _export.ShowDialog();
        }
    }
}
