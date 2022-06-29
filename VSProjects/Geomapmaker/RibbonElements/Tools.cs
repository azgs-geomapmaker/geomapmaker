using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Linq;

namespace Geomapmaker.RibbonElements
{
    internal class SetAllPrimaryKeys : Button
    {
        protected override async void OnClick()
        {
            int idCount = 0;

            // Add primary keys 
            idCount += await Data.AnyFeatureLayer.SetPrimaryKeys("ContactsAndFaults", "contactsandfaults_id");

            idCount += await Data.AnyFeatureLayer.SetPrimaryKeys("MapUnitPolys", "mapunitpolys_id");

            idCount += await Data.AnyFeatureLayer.SetPrimaryKeys("Stations", "stations_id");

            idCount += await Data.AnyFeatureLayer.SetPrimaryKeys("OrientationPoints", "orientationpoints_id");

            MessageBox.Show($"Created {idCount} Primary Keys", "Set All Primary Keys");
        }
    }

    internal class InsertGlossaryTerms : Button
    {
        protected override async void OnClick()
        {
            List<GlossaryTerm> predefinedTerms = await Data.PredefinedTerms.GetPredefinedDictionaryAsync();

            List<GlossaryTerm> glossaryTerms = await Data.Glossary.GetGlossaryDictionaryAsync();

            List<GlossaryTerm> insertTerms = new List<GlossaryTerm>();

            foreach (GlossaryTerm predefined in predefinedTerms)
            {
                if (!glossaryTerms.Any(a => a.Term == predefined.Term))
                {
                    insertTerms.Add(predefined);
                }
            }

            int count = await Data.Glossary.InsertGlossaryTermsAsync(insertTerms);

            MessageBox.Show($"Inserted {count} Glossary Terms", "Insert Glossary Terms");
        }
    }
}
