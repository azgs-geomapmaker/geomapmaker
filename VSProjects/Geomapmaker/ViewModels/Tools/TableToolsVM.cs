using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.Tools
{
    public class TableToolsVM
    {
        private const string symbologyCsvUrl = "https://raw.githubusercontent.com/azgs/geomapmaker/master/SetUp/SourceMaterials/Symbology.csv";

        private const string predefinedTermsCsvUrl = "https://raw.githubusercontent.com/azgs/geomapmaker/master/SetUp/SourceMaterials/PredefinedTerms.csv";

        public ICommand CommandSetAllPrimaryKeys => new RelayCommand(() => SetAllPrimaryKeys());

        public ICommand CommandInsertGlossaryTerms => new RelayCommand(() => InsertGlossaryTerms());

        public ICommand CommandSetMapUnit => new RelayCommand(() => SetMapUnit());

        public ICommand CommandZeroPadSymbols => new RelayCommand(() => ZeroPadSymbols());

        public ICommand CommandZeroPadHierarchyKeys => new RelayCommand(() => ZeroPadHierarchyKeys());

        public ICommand CommandGeopackageRename => new RelayCommand(() => GeopackageRename());

        public ICommand CommandSymbologyGithubLink => new RelayCommand(() => Process.Start(symbologyCsvUrl));

        public ICommand CommandInsertSymbologyTable => new RelayCommand(() => InsertSymbologyTable());

        public ICommand CommandPredefinedTermsGithubLink => new RelayCommand(() => Process.Start(predefinedTermsCsvUrl));

        public ICommand CommandInsertPredfinedTermsTable => new RelayCommand(() => InsertPredefinedTermsTable());

        public ToolsViewModel ParentVM { get; set; }

        public TableToolsVM(ToolsViewModel parentVM)
        {
            ParentVM = parentVM;
        }

        public async void SetAllPrimaryKeys()
        {
            ParentVM.CloseProwindow();

            int idCount = 0;

            // Add primary keys 
            idCount += await Data.AnyFeatureLayer.SetPrimaryKeys("ContactsAndFaults", "contactsandfaults_id");

            idCount += await Data.AnyFeatureLayer.SetPrimaryKeys("MapUnitPolys", "mapunitpolys_id");

            idCount += await Data.AnyFeatureLayer.SetPrimaryKeys("Stations", "stations_id");

            idCount += await Data.AnyFeatureLayer.SetPrimaryKeys("OrientationPoints", "orientationpoints_id");

            idCount += await Data.AnyStandaloneTable.SetPrimaryKeys("DescriptionOfMapUnits", "descriptionofmapunits_id");

            idCount += await Data.AnyStandaloneTable.SetPrimaryKeys("Glossary", "glossary_id");

            MessageBox.Show($"Added {idCount} Primary Key{(idCount == 1 ? "" : "s")}", "Set All Primary Keys");
        }

        public async void InsertGlossaryTerms()
        {
            ParentVM.CloseProwindow();

            // Get predefined terms from table
            List<GlossaryTerm> predefinedTerms = await Data.PredefinedTerms.GetPredefinedDictionaryAsync();

            // Get terms from the glossary table
            List<GlossaryTerm> glossaryTerms = await Data.Glossary.GetGlossaryDictionaryAsync();

            // List of terms to add
            List<GlossaryTerm> insertTerms = new List<GlossaryTerm>();

            foreach (GlossaryTerm predefined in predefinedTerms)
            {
                if (!glossaryTerms.Any(a => a.Term == predefined.Term))
                {
                    insertTerms.Add(predefined);
                }
            }

            int count = await Data.Glossary.InsertGlossaryTermsAsync(insertTerms);

            MessageBox.Show($"Added {count} Glossary Term{(count == 1 ? "" : "s")}", "Insert Glossary Terms");
        }

        public async void SetMapUnit()
        {
            ParentVM.CloseProwindow();

            int stationCount = await Data.Stations.UpdateStationsWithMapUnitIntersectionAsync();

            int opCount = await Data.OrientationPoints.UpdateOrientationPointsWithMapUnitIntersectionAsync();

            MessageBox.Show($"Updated {stationCount} Stations row{(stationCount == 1 ? "" : "s")} and {opCount} Orientation Points row{(opCount == 1 ? "" : "s")}", "Find MapUnitPolys Intersections");
        }

        public async void ZeroPadSymbols()
        {
            ParentVM.CloseProwindow();

            int cfCount = await Data.ContactsAndFaults.ZeroPadSymbolValues();

            int opCount = await Data.OrientationPoints.ZeroPadSymbolValues();

            MessageBox.Show($"Updated {cfCount} ContactsAndFaults row{(cfCount == 1 ? "" : "s")} and {opCount} Orientation Point row{(opCount == 1 ? "" : "s")}", "Zero Pad Symbols");
        }

        public async void ZeroPadHierarchyKeys()
        {
            ParentVM.CloseProwindow();

            int count = await Data.DescriptionOfMapUnits.ZeroPadHierarchyKeyValues();

            MessageBox.Show($"Updated {count} DescriptionOfMapUnits row{(count == 1 ? "" : "s")}", "Zero Pad Hierarchy Keys");
        }

        public async void GeopackageRename()
        {
            ParentVM.CloseProwindow();

            int count = 0;

            IEnumerable<Layer> layers = MapView.Active?.Map?.Layers?.Where(a => a.Name.StartsWith("main.")) ?? new List<Layer>();

            IEnumerable<StandaloneTable> tables = MapView.Active?.Map.GetStandaloneTablesAsFlattenedList().Where(b => b.Name.StartsWith("main.")) ?? new List<StandaloneTable>(); ;

            await QueuedTask.Run(() =>
            {
                foreach (Layer layer in layers)
                {
                    layer.SetName(layer.Name.Remove(0, 5));
                    count++;
                }

                foreach (StandaloneTable table in tables)
                {
                    table.SetName(table.Name.Remove(0, 5));
                    count++;
                }
            });

            MessageBox.Show($"Updated {count} dataset name{(count == 1 ? "" : "s")}", "Geopackage Rename");
        }

        public async void InsertSymbologyTable()
        {
            ParentVM.CloseProwindow();

            string savePath = Path.Combine(Project.Current.HomeFolderPath, "Symbology.csv");

            try
            {
                using (WebClient client = new WebClient())
                {
                    await client.DownloadFileTaskAsync(symbologyCsvUrl, savePath);
                }

                await QueuedTask.Run(() =>
                {
                    StandaloneTableFactory.Instance.CreateStandaloneTable(new Uri(savePath), MapView.Active.Map, "Symbology");
                });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Something went wrong.");
            }
        }

        public async void InsertPredefinedTermsTable()
        {
            ParentVM.CloseProwindow();

            string savePath = Path.Combine(Project.Current.HomeFolderPath, "PredefinedTerms.csv");

            try
            {
                using (WebClient client = new WebClient())
                {
                    await client.DownloadFileTaskAsync(predefinedTermsCsvUrl, savePath);
                }

                await QueuedTask.Run(() =>
                {
                    StandaloneTableFactory.Instance.CreateStandaloneTable(new Uri(savePath), MapView.Active.Map, "PredefinedTerms");
                });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Something went wrong.");
            }
        }
    }
}