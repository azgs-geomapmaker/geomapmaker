using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;

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
}
