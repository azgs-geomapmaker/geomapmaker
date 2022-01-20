using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Editing.Templates;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.MapTools
{
    internal class ContactFaultTool : MapTool
    {
        public ContactFaultTool()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Line;
            SketchOutputMode = SketchOutputMode.Map;
        }

        protected override async Task OnToolActivateAsync(bool active)
        {
            await QueuedTask.Run(async () =>
            {
                var map = MapView.Active?.Map;
                if (map == null)
                {
                    return;
                }

                var myTemplate = map.FindLayers("ContactsAndFaults").FirstOrDefault()?.GetTemplate("1.1.1");

                if (myTemplate == null)
                    return;

                // activate the template
                await myTemplate.ActivateDefaultToolAsync();

                // retrieve the inspector
                var insp = myTemplate.Inspector;

                // modify fields
                insp["type"] = "typeTestINSPECTOR";
            });

            return;
        }

        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            return base.OnSketchCompleteAsync(geometry);
        }
    }
}
