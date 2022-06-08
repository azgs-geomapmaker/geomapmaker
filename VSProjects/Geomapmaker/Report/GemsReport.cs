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

.tableHeader {
    background-color: #1E5288;
    color: white;
}

.info {
    font-family: ""Courier New"", Courier, monospace;
    margin-left: 20px;
    margin-right: 20px;
}

.passed {
    background-color: #70B865;
}

.failed {
    background-color: #fc3c56;
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
            return new XElement("div",
                new XElement("h2", "Validation"),
                await GetDataSourceErrorsAsync(),
                await GetDescriptionOfMapUnitsErrorsAsync(),
                await GetGlossaryErrorsAsync(),
                await GetGeoMaterialDictErrorsAsync(),
                await GetMapUnitPolysErrorsAsync()
            );
        }

        private async Task<XElement> GetDataSourceErrorsAsync()
        {
            List<ValidationRule> results = await DataSources.GetValidationResultsAsync();

            bool isFailed = results.Any(a => a.Errors.Count != 0);

            return new XElement("div",
                    new XElement("table",
                       new XElement("tr",
                            new XElement("th", new XAttribute("colspan", "2"), new XAttribute("class", $"{(isFailed ? "failed" : "passed")}"),
                            $"DataSources: {(isFailed ? "Failed": "Passed")}")
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

        private async Task<XElement> GetDescriptionOfMapUnitsErrorsAsync()
        {
            List<ValidationRule> results = await DescriptionOfMapUnits.GetValidationResultsAsync();

            bool isFailed = results.Any(a => a.Errors.Count != 0);

            return new XElement("div",
                    new XElement("table",
                       new XElement("tr",
                            new XElement("th", new XAttribute("colspan", "2"), new XAttribute("class", $"{(isFailed ? "failed" : "passed")}"),
                            $"DescriptionOfMapUnits: {(isFailed ? "Failed" : "Passed")}")
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

        private async Task<XElement> GetGlossaryErrorsAsync()
        {
            List<ValidationRule> results = await Glossary.GetValidationResultsAsync();

            bool isFailed = results.Any(a => a.Errors.Count != 0);

            return new XElement("div",
                    new XElement("table",
                       new XElement("tr",
                            new XElement("th", new XAttribute("colspan", "2"), new XAttribute("class", $"{(isFailed ? "failed" : "passed")}"),
                            $"Glossary: {(isFailed ? "Failed" : "Passed")}")
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

        private async Task<XElement> GetGeoMaterialDictErrorsAsync()
        {
            List<ValidationRule> results = await GeoMaterialDict.GetValidationResultsAsync();

            bool isFailed = results.Any(a => a.Errors.Count != 0);

            return new XElement("div",
                    new XElement("table",
                       new XElement("tr",
                            new XElement("th", new XAttribute("colspan", "2"), new XAttribute("class", $"{(isFailed ? "failed" : "passed")}"),
                            $"GeoMaterialDict: {(isFailed ? "Failed" : "Passed")}")
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

        private async Task<XElement> GetMapUnitPolysErrorsAsync()
        {
            List<ValidationRule> results = await MapUnitPolys.GetValidationResultsAsync();

            bool isFailed = results.Any(a => a.Errors.Count != 0);

            return new XElement("div",
                    new XElement("table",
                       new XElement("tr",
                            new XElement("th", new XAttribute("colspan", "2"), new XAttribute("class", $"{(isFailed ? "failed" : "passed")}"),
                            $"MapUnitPolys: {(isFailed ? "Failed" : "Passed")}")
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
                list.Add(new XElement("div", new XAttribute("style", "color: red;"), error));
            }

            return list;
        }
    }
}
