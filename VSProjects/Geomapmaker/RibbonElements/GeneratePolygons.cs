using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Editing.Templates;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System.Linq;

namespace Geomapmaker.RibbonElements
{
    internal class GeneratePolygons : Button
    {
        protected override async void OnClick()
        {
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
                    Inspector insp = new Inspector();
                    insp.LoadSchema(polyLayer);

                    // Set attributes
                    insp["Symbol"] = templateName;
                    insp["MapUnit"] = templateName;
                    insp["IdentityConfidence"] = templateName;
                    insp["Notes"] = templateName;
                    insp["DataSourceID"] = templateName;

                    // Create the temporary template
                    tmpTemplate = polyLayer.CreateTemplate(templateName, templateName, insp);
                }

                //CIMFeatureTemplate templateDef = tmpTemplate.GetDefinition() as CIMFeatureTemplate;

                //templateDef.DefaultValues["IdentityConfidence"] = IdentityConfidence;
                //templateDef.DefaultValues["Notes"] = Notes;

                //tmpTemplate.SetDefinition(templateDef);

                //EditingTemplate updatedTemplate = polyLayer.GetTemplate(Selected.MapUnit);



                var foo = cfLayer.Select();



                //// Contruct polygons from cf lines
                //op.ConstructPolygons(tmpTemplate, cfLayer, ContactFaultOids.Keys, null, false);

                //op.Execute();

                //// Check if the polygon create was a success
                //if (op.IsSucceeded)
                //{

                //}
                //else
                //{

                //}

            });
        }
    }
}
