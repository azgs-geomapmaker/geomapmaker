using ArcGIS.Core.Data;
using Newtonsoft.Json.Linq;

namespace Geomapmaker.Data
{
    public static class DbConnectionProperties
    {
        private static DatabaseConnectionProperties connectionProperties;

        public static void SetProperties(JObject props)
        {
            connectionProperties = new DatabaseConnectionProperties(EnterpriseDatabaseType.PostgreSQL)
            {
                AuthenticationMode = AuthenticationMode.DBMS,
                Instance = props["instance"].ToString(),
                Database = props["database"].ToString(),
                User = props["user"].ToString(),
                Password = props["password"].ToString(),
            };
        }

        public static DatabaseConnectionProperties GetProperties()
        {
            return connectionProperties;
        }
    }
}
