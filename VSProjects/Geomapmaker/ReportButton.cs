using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Contracts;
using Geomapmaker.Report;

namespace Geomapmaker
{
    internal class ReportButton : Button
    {
        protected override void OnClick()
        {
            GemsReport report = new GemsReport();

            report.BuildReport();

            report.ExportReport(@"C:\Users\mcamp\Desktop\Export_Test\Report.html");
        }
    }
}
