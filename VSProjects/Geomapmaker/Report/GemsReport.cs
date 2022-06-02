using Geomapmaker.Models;
using System;
using System.IO;
using System.Xml.Linq;

namespace Geomapmaker.Report
{
    public class GemsReport
    {
        private ReportModel report;

        public void BuildReport()
        {
            report = new ReportModel()
            {
                ProjectName = Data.ArcGisProject.GetName(),
                ReportDate = DateTime.Today.ToString("D"),
            };
        }

        public void ExportReport(string filePath)
        {
            if (report == null)
            {
                return;
            }

            XDocument reportXml = new XDocument(
               new XElement("html",
                   GetHeader(),
                   GetInformation()
               )
            );

            File.WriteAllText(filePath, reportXml.ToString());
        }

        private XElement GetHeader()
        {
            return new XElement("h1",
                new XAttribute("style", "background-color: lightgray; padding: 5px; border-radius: 4px; font-family: \"Century Gothic\", CenturyGothic, AppleGothic, sans-serif; text-align: center;"),
                    "GeMS validation of " + report.ProjectName
            );
        }

        private XElement GetInformation()
        {
            return new XElement("div",
                new XAttribute("style", "font-family: Courier New, Courier, monospace; margin-left: 20px; margin-right: 20px;"),
                    new XElement("div", "Date: " + report.ReportDate)
                    //new XElement("div", "lorem ipsum"),
            );
        }
    }
}
