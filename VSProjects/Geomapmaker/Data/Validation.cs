using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public class Validation
    {
        public static bool CheckFeatureLayerExists(string layerName)
        {
            if (MapView.Active == null)
            {
                return false;
            }

            Layer layer = MapView.Active?.Map.FindLayers(layerName).FirstOrDefault();

            return layer != null;
        }

        public static async Task<bool> CheckStandaloneTableExistsAsync(string tableName)
        {
            // Check for active map
            if (MapView.Active == null)
            {
                return false;
            }

            StandaloneTable standaloneTable = MapView.Active?.Map.StandaloneTables.FirstOrDefault(a => a.Name == tableName);

            // Check if table was null
            if (standaloneTable == null)
            {
                return false;
            }

            // Check for the underyling table
            bool underlyingTableExists = false;

            await QueuedTask.Run(() =>
            {
                using (Table table = standaloneTable.GetTable())
                {
                    if (table != null)
                    {
                        underlyingTableExists = true;
                    }
                }
            });

            return underlyingTableExists;
        }
    }
}
