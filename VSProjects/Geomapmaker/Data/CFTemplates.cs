using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Editing.Templates;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.Data
{
    public class CFTemplates
    {
        public static async Task<List<ContactFaultTemplate>> GetContactFaultTemplatesAsync()
        {
            // List of templates to return
            List<ContactFaultTemplate> contactFaultTemplates = new List<ContactFaultTemplate>();

            IEnumerable<EditingTemplate> layerTemplates = new List<EditingTemplate>();

            // Find the ContactsFaults layer
            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "ContactsAndFaults");

            if (layer == null)
            {
                return contactFaultTemplates;
            }

            await QueuedTask.Run(() =>
            {
                // Get templates from CF layer
                layerTemplates = layer.GetTemplates();

                foreach (EditingTemplate template in layerTemplates)
                {
                    if (template.Name != "ContactsAndFaults" && template.Name != "Sketch")
                    {
                        // Get CIMFeatureTemplate
                        CIMFeatureTemplate templateDef = template.GetDefinition() as CIMFeatureTemplate;

                        ContactFaultTemplate tmpTemplate = new ContactFaultTemplate()
                        {
                            Type = templateDef.DefaultValues["type"].ToString(),
                            Label = templateDef.DefaultValues["label"].ToString(),
                            Symbol = templateDef.DefaultValues["symbol"].ToString(),
                            IdentityConfidence = templateDef.DefaultValues["identityconfidence"].ToString(),
                            ExistenceConfidence = templateDef.DefaultValues["existenceconfidence"].ToString(),
                            LocationConfidenceMeters = templateDef.DefaultValues["locationconfidencemeters"].ToString(),
                            IsConcealed = templateDef.DefaultValues["isconcealed"].ToString() == "Y",
                            DataSource = templateDef.DefaultValues["datasourceid"].ToString(),
                            Template = template
                        };

                        // Notes is an optional field
                        if (templateDef.DefaultValues.ContainsKey("notes"))
                        {
                            tmpTemplate.Notes = templateDef.DefaultValues["notes"].ToString();
                        }

                        contactFaultTemplates.Add(tmpTemplate);
                    }
                }

            });

            return contactFaultTemplates;
        }
    }
}