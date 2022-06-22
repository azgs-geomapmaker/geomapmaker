using Geomapmaker.Data;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Geomapmaker.Report
{
    public class GemsReport
    {
        private ReportModel report;

        private readonly string css =
@"

h1, h2 {
    font-family: ""Times New Roman"", Times, serif;
    background-color: lightgray;
    padding: 5px;
    border-radius: 4px;
    text-align: center;
}

h1 {
    font-size: 1.5em;
    font-weight: bold;
}

table {
    margin-left: auto;
    margin-right: auto;
    width: 80%;
}

table,
th,
td {
    margin-top: 20px;
    border: 1px solid gray;
    border-collapse: collapse;
    padding: 3px;
}

table td {
    vertical-align: top;
}

td:last-child {
    font-weight: bold;
    text-align: right;
}

.info {
    font-family: ""Courier New"", Courier, monospace;
    margin-left: 20px;
    margin-right: 20px;
}

.tableHeader {
    background-color: #1E5288;
    color: white;
}

.passed {
    height: 40px;
    font-size: 1.25em;
    background-color: #70B865;
}

.failed {
    color: #fff;
    height: 40px;
    font-size: 1.25em;
    background-color: #b2162c;
}

";

        public void BuildReport()
        {
            report = new ReportModel()
            {
                ProjectName = _helpers.Helpers.GetProjectName(),
                ReportDate = DateTime.Today.ToString("D"),
            };
        }

        public async Task<bool> ExportReportAsync(string filePath)
        {
            if (report == null)
            {
                return false;
            }

            XDocument reportXml = new XDocument(
               new XElement("html",
                   new XElement("head",
                    new XElement("style", css)
                   ),
                   GetHeader(),
                   GetInformation(),
                   await GetValidationAsync()
               )
            );

            File.WriteAllText(filePath, reportXml.ToString());

            return true;
        }

        private XElement GetHeader()
        {
            return new XElement("h1",
                "GeMS report of " + report.ProjectName
            );
        }

        private XElement GetInformation()
        {
            return new XElement("div",
                new XAttribute("class", "info"),
                    new XElement("div", "Date: " + report.ReportDate)
            );
        }

        private async Task<XElement> GetValidationAsync()
        {
            List<ValidationRule> SymbologyResults = await Symbology.GetValidationResultsAsync();
            List<ValidationRule> DataSourcesResults = await DataSources.GetValidationResultsAsync();
            List<ValidationRule> DescriptionOfMapUnitsResults = await DescriptionOfMapUnits.GetValidationResultsAsync();
            List<ValidationRule> GlossaryResults = await Glossary.GetValidationResultsAsync();
            List<ValidationRule> GeoMaterialDictResults = await GeoMaterialDict.GetValidationResultsAsync();
            List<ValidationRule> MapUnitPolysResults = await MapUnitPolys.GetValidationResultsAsync();
            List<ValidationRule> ContactsAndFaultsResults = await ContactsAndFaults.GetValidationResultsAsync();
            List<ValidationRule> StationsResults = await Stations.GetValidationResultsAsync();
            List<ValidationRule> OrientationPointsResults = await OrientationPoints.GetValidationResultsAsync();

            return new XElement("div",
                new XElement("h2", "Validation"),

                new XElement("table",
                    new XAttribute("style", "width: 25%;"),
                        new XElement("tr",
                         new XAttribute("class", "tableHeader"),
                            new XElement("th", new XAttribute("style", "text-align: left;"), "Dataset"),
                            new XElement("th", new XAttribute("style", "text-align: right;"), "Result")
                        ),

                        new XElement("tr",
                            new XElement("td", new XAttribute("style", "text-align: left;"), "Symbology"), GetValidationResult(SymbologyResults)
                        ),

                        new XElement("tr",
                            new XElement("td", new XAttribute("style", "text-align: left;"), "DataSources"), GetValidationResult(DataSourcesResults)
                        ),

                        new XElement("tr",
                            new XElement("td", new XAttribute("style", "text-align: left;"), "DescriptionOfMapUnits"), GetValidationResult(DescriptionOfMapUnitsResults)
                        ),

                        new XElement("tr",
                            new XElement("td", new XAttribute("style", "text-align: left;"), "Glossary"), GetValidationResult(GlossaryResults)
                        ),

                        new XElement("tr",
                            new XElement("td", new XAttribute("style", "text-align: left;"), "GeoMaterialDict"), GetValidationResult(GeoMaterialDictResults)
                        ),

                        new XElement("tr",
                            new XElement("td", new XAttribute("style", "text-align: left;"), "MapUnitPolys"), GetValidationResult(MapUnitPolysResults)
                        ),

                        new XElement("tr",
                            new XElement("td", new XAttribute("style", "text-align: left;"), "ContactsAndFaultsResults"), GetValidationResult(ContactsAndFaultsResults)
                        ),

                        new XElement("tr",
                            new XElement("td", new XAttribute("style", "text-align: left;"), "Stations"), GetValidationResult(StationsResults)
                        ),

                        new XElement("tr",
                            new XElement("td", new XAttribute("style", "text-align: left;"), "Symbology"), GetValidationResult(OrientationPointsResults)
                        )
                ),

                // Required datsets
                GetErrorTable("Symbology", SymbologyResults, true),
                GetErrorTable("DataSources", DataSourcesResults, true),
                GetErrorTable("DescriptionOfMapUnits", DescriptionOfMapUnitsResults, true),
                GetErrorTable("Glossary", GlossaryResults, true),
                GetErrorTable("GeoMaterialDict", GeoMaterialDictResults, true),
                GetErrorTable("MapUnitPolys", MapUnitPolysResults, true),
                GetErrorTable("ContactsAndFaults", ContactsAndFaultsResults, true),

                // Optional datasets
                GetErrorTable("Stations", StationsResults, false),
                GetErrorTable("OrientationPoints", OrientationPointsResults, false)
            );
        }

        private XElement GetValidationResult(List<ValidationRule> results)
        {
            if (results.Any(a => a.Status == ValidationStatus.Failed))
            {
                return new XElement("td", new XAttribute("style", "text-align: right; color: red;"), "Failed");
            }
            else if (results.All(a => a.Status == ValidationStatus.Skipped))
            {
                return new XElement("td", new XAttribute("style", "text-align: right;"), "Skipped");
            }

            return new XElement("td", new XAttribute("style", "text-align: right;"), "Passed");
        }

        private XElement GetErrorTable(string DatsetName, List<ValidationRule> results, bool isRequiredDataset)
        {
            string result = "Passed";

            if (results.Any(a => a.Status == ValidationStatus.Failed))
            {
                result = "Failed";
            }
            else if (results.All(a => a.Status == ValidationStatus.Skipped))
            {
                result = "Skipped";
            }

            return new XElement("div",
                    new XElement("table",
                       new XElement("tr",
                            new XElement("th", new XAttribute("colspan", "2"), new XAttribute("class", $"{(result == "Failed" ? "failed" : "passed")}"),
                            $"{DatsetName}: {result}")
                        ),
                        new XElement("tr",
                         new XAttribute("class", "tableHeader"),
                            new XElement("th", new XAttribute("style", "text-align: left;"), "Rule"),
                            new XElement("th", new XAttribute("style", "text-align: right;"), "Result")
                        ),
                        ValidationRuleListToHtml(results)
                    )
            );
        }

        private List<XElement> ValidationRuleListToHtml(List<ValidationRule> results)
        {
            List<XElement> list = new List<XElement>();

            foreach (ValidationRule result in results)
            {
                if (result.Status == ValidationStatus.Skipped)
                {
                    list.Add(new XElement("tr",
                        new XElement("td", result.Description),
                        new XElement("td", "Skipped")
                    ));
                }
                else if (result.Status == ValidationStatus.Passed)
                {
                    list.Add(new XElement("tr",
                        new XElement("td", result.Description),
                        new XElement("td", "Passed")
                    ));
                }
                else
                {
                    list.Add(new XElement("tr",
                        new XElement("td", result.Description),
                            new XElement("td", ErrorListToHtml(result.Errors))
                        )
                    );
                }
            }

            return list;
        }

        private List<XElement> ErrorListToHtml(List<string> errorList)
        {
            List<XElement> list = new List<XElement>();

            foreach (string error in errorList)
            {
                list.Add(new XElement("div", new XAttribute("style", "color: red; margin-bottom: 6px;"), error));
            }

            return list;
        }
    }
}
