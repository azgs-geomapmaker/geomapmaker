using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Templates;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System.Collections.Generic;
using System.Linq;

namespace Geomapmaker.RibbonElements
{
    internal class GeneratePolygons : Button
    {
        protected override async void OnClick()
        {
            Data.MapUnitPolys.RebuildMUPSymbologyAndTemplates();

            // Contacts and Faults layer
            FeatureLayer cfLayer = MapView.Active.Map.GetLayersAsFlattenedList().FirstOrDefault((l) => l.Name == "ContactsAndFaults") as FeatureLayer;

            // Map Unit Poly layer
            FeatureLayer polyLayer = MapView.Active.Map.GetLayersAsFlattenedList().FirstOrDefault((l) => l.Name == "MapUnitPolys") as FeatureLayer;

            if (cfLayer == null || polyLayer == null)
            {
                return;
            }

            EditOperation op = new EditOperation
            {
                Name = "Generate All MapUnitPolys"
            };

            await QueuedTask.Run(() =>
            {
                string templateName = GeomapmakerModule.MUP_UnassignedTemplateName;

                EditingTemplate tmpTemplate = polyLayer.GetTemplate(templateName);

                if (tmpTemplate == null)
                {
                    return;
                }

                Selection cf_Collection = cfLayer.Select();

                IReadOnlyList<long> ContactFaultOids = cf_Collection.GetObjectIDs();

                op.ConstructPolygons(tmpTemplate, cfLayer, ContactFaultOids, null, true);

                op.Execute();

                Data.MapUnitPolys.RebuildMUPSymbologyAndTemplates();
            });
        }
    }
}
