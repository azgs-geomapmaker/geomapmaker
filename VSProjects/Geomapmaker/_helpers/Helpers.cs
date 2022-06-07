using System.Collections.Generic;
using System.Text;

namespace Geomapmaker._helpers
{
    public class Helpers
    {
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
