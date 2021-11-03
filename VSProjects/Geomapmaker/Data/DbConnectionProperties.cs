using ArcGIS.Core.Data;

namespace Geomapmaker.Data
{
    public static class DbConnectionProperties
    {
        private static DatabaseConnectionProperties connectionProperties;

        public static void SetProperties(DatabaseConnectionProperties props)
        {
            connectionProperties = props;
        }

        public static DatabaseConnectionProperties GetProperties()
        {
            return connectionProperties;
        }
    }
}
