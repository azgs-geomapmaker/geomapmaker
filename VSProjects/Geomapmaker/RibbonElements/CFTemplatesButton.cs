using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using Geomapmaker.Data;
using Geomapmaker.Report;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Geomapmaker.RibbonElements
{
    internal class CFTemplatesButton : Button
    {
        protected override async void OnClick()
        {
            ProgressDialog progDialog = new ProgressDialog("Building Templates");

            progDialog.Show();

            //await Task.Run(() => ContactsAndFaults.RebuildContactsFaultsSymbology());
            await ContactsAndFaults.GenerateTemplatesAsync();

            progDialog.Hide();
            
        }
    }
}
