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
    }
}
