using Geomapmaker.Data;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Geomapmaker.Report
{
    public class GemsReport
    {
        private ReportModel report;

        private readonly string css =
@"
h1 {font-size: 1.5em; font-weight: bold;}

h1, h2 {background-color: lightgray; padding: 5px; border-radius: 4px; font-family: Century Gothic,CenturyGothic,AppleGothic,sans-serif; , sans-serif; text-align: center;}

table {
    margin-left: auto;
    margin-right: auto;
    width: 80%;
}

table td:first-child {
    text-align: center;
    width: 200px;
}

table th {
    height: 50px;
    background-color: #1E5288;
    color: white;
}

table,
th,
td {
    border: 1px solid gray;
    border-collapse: collapse;
    padding: 3px;
}

table td {
    vertical-align: top;
}

.info {font-family: Courier New, Courier, monospace; margin-left: 20px; margin-right: 20px;}

";

        public void BuildReport()
        {
            report = new ReportModel()
            {
                ProjectName = ArcGisProject.GetName(),
                ReportDate = DateTime.Today.ToString("D"),
            };
        }

        public async void ExportReport(string filePath)
        {
            if (report == null)
            {
                return;
            }

            XDocument reportXml = new XDocument(
               new XElement("html",
                   new XElement("head",
                    new XElement("style", css)
                   ),
                   GetHeader(),
                   GetInformation(),
                   await GetGemsErrorsAsync(),
                   await GetGeomapmakerErrorsAsync()
               )
            );

            File.WriteAllText(filePath, reportXml.ToString());
        }

        private XElement GetHeader()
        {
            return new XElement("h1",
                "GeMS validation of " + report.ProjectName
            );
        }

        private XElement GetInformation()
        {
            return new XElement("div",
                new XAttribute("class", "info"),
                    new XElement("div", "Date: " + report.ReportDate)
            );
        }

        private async Task<XElement> GetGemsErrorsAsync()
        {
            Dictionary<string, List<string>> errors = await GemsValidation.GetErrorsAsync();

            return new XElement("div",
                    new XElement("h2",
                        "GeMs Validation"
                    ),
                    new XElement("table",
                        new XElement("tr",
                            new XElement("th", "Dataset"),
                            new XElement("th", new XAttribute("style", "text-align: right;"), "Errors")
                        ),
                        new XElement("tr",
                            new XElement("td", "Symbology"),
                            new XElement("td", ErrorListToHtml(errors["Symbology"]))
                        ),
                        new XElement("tr",
                            new XElement("td", "DataSources"),
                            new XElement("td", ErrorListToHtml(errors["DataSources"]))
                        ),
                        new XElement("tr",
                            new XElement("td", "DescriptionOfMapUnits"),
                            new XElement("td", ErrorListToHtml(errors["DescriptionOfMapUnits"]))
                        ),
                        new XElement("tr",
                            new XElement("td", "Glossary"),
                            new XElement("td", ErrorListToHtml(errors["Glossary"]))
                        ),
                        new XElement("tr",
                            new XElement("td", "GeoMaterialDict"),
                            new XElement("td", ErrorListToHtml(errors["GeoMaterialDict"]))
                        ),
                        new XElement("tr",
                            new XElement("td", "MapUnitPolys"),
                            new XElement("td", ErrorListToHtml(errors["MapUnitPolys"]))
                        ),
                         new XElement("tr",
                            new XElement("td", "ContactsAndFaults"),
                            new XElement("td", ErrorListToHtml(errors["ContactsAndFaults"]))
                        ),
                        new XElement("tr",
                            new XElement("td", "Stations"),
                            new XElement("td", ErrorListToHtml(errors["Stations"]))
                        ),
                        new XElement("tr",
                            new XElement("td", "OrientationPoints"),
                            new XElement("td", ErrorListToHtml(errors["OrientationPoints"]))
                        )
                    )
            );
        }

        private async Task<XElement> GetGeomapmakerErrorsAsync()
        {
            Dictionary<string, List<string>> errors = await GeomapmakerValidation.GetErrorsAsync();

            return new XElement("div",
                    new XElement("h2",
                        "Geomapmaker Validation"
                    ),
                    new XElement("table",
                        new XElement("tr",
                            new XElement("th", "Dataset"),
                            new XElement("th", new XAttribute("style", "text-align: right;"), "Errors")
                        ),
                        new XElement("tr",
                            new XElement("td", "Symbology"),
                            new XElement("td", ErrorListToHtml(errors["Symbology"]))
                        ),
                        new XElement("tr",
                            new XElement("td", "DataSources"),
                            new XElement("td", ErrorListToHtml(errors["DataSources"]))
                        ),
                        new XElement("tr",
                            new XElement("td", "DescriptionOfMapUnits"),
                            new XElement("td", ErrorListToHtml(errors["DescriptionOfMapUnits"]))
                        ),
                        new XElement("tr",
                            new XElement("td", "Glossary"),
                            new XElement("td", ErrorListToHtml(errors["Glossary"]))
                        ),
                        new XElement("tr",
                            new XElement("td", "GeoMaterialDict"),
                            new XElement("td", ErrorListToHtml(errors["GeoMaterialDict"]))
                        ),
                        new XElement("tr",
                            new XElement("td", "MapUnitPolys"),
                            new XElement("td", ErrorListToHtml(errors["MapUnitPolys"]))
                        ),
                         new XElement("tr",
                            new XElement("td", "ContactsAndFaults"),
                            new XElement("td", ErrorListToHtml(errors["ContactsAndFaults"]))
                        ),
                        new XElement("tr",
                            new XElement("td", "Stations"),
                            new XElement("td", ErrorListToHtml(errors["Stations"]))
                        ),
                        new XElement("tr",
                            new XElement("td", "OrientationPoints"),
                            new XElement("td", ErrorListToHtml(errors["OrientationPoints"]))
                        )
                    )
            );
        }

        private List<XElement> ErrorListToHtml(List<string> errorList)
        {
            List<XElement> list = new List<XElement>();

            if (errorList.Count == 0)
            {
                list.Add(new XElement("div", new XAttribute("style", "font-weight: bold; text-align: right;"), "Passed"));
            }
            else
            {
                foreach (string error in errorList)
                {
                    list.Add(new XElement("div", new XAttribute("style", "color: red; text-align: right;"), error));
                }
            }

            return list;
        }

    }

}
