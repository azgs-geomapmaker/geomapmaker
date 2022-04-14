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

        // Convert a list of strings into an HTML string in a list
        // Used to display errors in a HTML tooltip
        public static List<string> ErrorListTooltip(List<string> errorList)
        {
            StringBuilder sb = new StringBuilder();

            foreach (string error in errorList)
            {
                // append on new line
                sb.Append($"<br />{error}");
            }

            return new List<string>() { sb.ToString() };
        }
    }
}
