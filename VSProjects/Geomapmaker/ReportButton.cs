using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Contracts;
using Geomapmaker.Report;
using System.Diagnostics;
using System.IO;

namespace Geomapmaker
{
    internal class ReportButton : Button
    {
        protected override async void OnClick()
        {
            GemsReport report = new GemsReport();

            report.BuildReport();

            string tempFilePath = Path.GetTempPath() + "report.html";

            await report.ExportReportAsync(tempFilePath);

            var process = Process.Start(tempFilePath);

            process.Exited += (s, e) => File.Delete(tempFilePath);
        }
    }
}
