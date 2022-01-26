using ArcGIS.Desktop.Editing.Templates;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public class CFTemplates
    {
        public static async Task<List<EditingTemplate>> GetContactFaultTemplatesAsync()
        {

            List<EditingTemplate> templates = new List<EditingTemplate>();

            // Find the ContactsFaults layer
            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "ContactsAndFaults");

            if (layer == null)
            {
                return templates;
            }

            await QueuedTask.Run(() =>
            {
                // Get templates from CF layer
                templates = new List<EditingTemplate>(layer.GetTemplates());
            });

            return templates;
        }
    }
}