namespace Geomapmaker._helpers
{
    public class Helpers
    {
        // Convert row object to a string. 
        public static string RowValueToString(object value)
        {
            // Nulls are converted to an empty string
            return value == null ? "" : value.ToString();
        }

        // Convert row object to a string. 
        public static long RowValueToLong(object value)
        {
            // 0 if null. 
            return value == null ? 0 : long.Parse(value.ToString());
        }
    }
}
