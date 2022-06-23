using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using Geomapmaker.Report;
using System.Diagnostics;
using System.IO;

namespace Geomapmaker.RibbonElements
{
    internal class ReportButton : Button
    {
        protected override async void OnClick()
        {
            ProgressDialog progDialog = new ProgressDialog("Building Report");

            progDialog.Show();

            GemsReport report = new GemsReport();

            report.BuildReport();

            string tempFilePath = Path.GetTempPath() + "report.html";

            await report.ExportReportAsync(tempFilePath);

            progDialog.Hide();

            Process.Start(tempFilePath);
        }
    }
}
