using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;

namespace Geomapmaker.RibbonElements
{
    internal class SetAllPrimaryKeys : Button
    {
        protected override void OnClick()
        {
            int idCount = 0;

            // Add primary keys 

            MessageBox.Show($"Created {idCount} Primary Keys", "Set All Primary Keys");
        }
    }
}
