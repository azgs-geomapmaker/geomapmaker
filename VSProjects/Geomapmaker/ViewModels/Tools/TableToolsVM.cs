using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Dialogs;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.Tools
{
    public class TableToolsVM
    {
        public ICommand CommandSetAllPrimaryKeys => new RelayCommand(() => SetAllPrimaryKeys());

        public ICommand CommandInsertGlossaryTerms => new RelayCommand(() => InsertGlossaryTerms());

        public ICommand CommandSetMapUnit => new RelayCommand(() => SetMapUnit());

        public ICommand CommandZeroPadSymbols => new RelayCommand(() => ZeroPadSymbols());

        public ICommand CommandZeroPadHierarchyKeys => new RelayCommand(() => ZeroPadHierarchyKeys());

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
            int cfCount = await Data.ContactsAndFaults.ZeroPadSymbolValues();

            int opCount = await Data.OrientationPoints.ZeroPadSymbolValues();

            MessageBox.Show($"Updated {cfCount} ContactsAndFaults row{(cfCount == 1 ? "" : "s")} and {opCount} Orientation Point row{(opCount == 1 ? "" : "s")}", "Zero Pad Symbols");
        }

        public async void ZeroPadHierarchyKeys()
        {
            int count = await Data.DescriptionOfMapUnits.ZeroPadHierarchyKeyValues();

            MessageBox.Show($"Updated {count} DescriptionOfMapUnits row{(count == 1 ? "" : "s")}", "Zero Pad Hierarchy Keys");
        }
    }
}
