using System.Collections.Generic;

namespace Geomapmaker._helpers
{
    public class Helpers
    {
        public static string GetProjectName()
        {
            // Get the project name
            string projectName = ArcGIS.Desktop.Core.Project.Current?.Name ?? "Geomapmaker";

            // Remove extension
            return projectName.Replace(".aprx", "");
        }

        // Convert row object to a string
        public static string RowValueToString(object value)
        {
            // Nulls are converted to an empty string
            return value == null ? "" : value.ToString();
        }

        // Convert row object to a long
        public static long RowValueToLong(object value)
        {
            // 0 if null. 
            return value == null ? 0 : long.Parse(value.ToString());
        }

        // Convert a list of strings into HTML
        public static List<string> ErrorListToTooltip(List<string> errorList)
        {
            return new List<string>() { string.Join("<br />", errorList) };
        }
    }
}
