using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Editing.Templates;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Geomapmaker.MapTools {
    internal class ApplyCFTemplateTool : MapTool {
        public ApplyCFTemplateTool() {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Map;
        }

        protected override Task OnToolActivateAsync(bool active) {
            return base.OnToolActivateAsync(active);
        }

        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry) {
            ProgressDialog progDialog = new ProgressDialog("Getting selected template");

            progDialog.Show();

            var mv = MapView.Active;

            await QueuedTask.Run(() => {

                //Get the currently selected template in the Create Features pane
                EditingTemplate currentTemplate = EditingTemplate.Current;

                if (currentTemplate == null || currentTemplate.Layer?.Name != "ContactsAndFaults") {
                    MessageBox.Show("Please select a Contacts and Faults template before applying.");
                    return;
                }

                // Get default values from the template
                CIMEditingTemplate templateDef = currentTemplate.GetDefinition();
                IDictionary<string, object> defaultValues = null;
                if (templateDef is CIMBasicRowTemplate rowTemplate) {
                    defaultValues = rowTemplate.DefaultValues;
                }

                // Find the CF feature at the sketched location
                var features = mv.GetFeatures(geometry);
                FeatureLayer layer = mv.Map.FindLayers("ContactsAndFaults").FirstOrDefault() as FeatureLayer;
                var objectID = features.ToDictionary().ContainsKey(layer) ? features[layer] : null;

                if (objectID != null && defaultValues != null) {
                    EditOperation editOperation = new EditOperation();
                    editOperation.Name = "Modify CF Feature Attributes";

                    // Load the feature into an Inspector
                    Inspector inspector = new Inspector();
                    inspector.Load(layer, objectID);

                    // Apply the new default values to the Inspector
                    foreach (var kvp in defaultValues) {
                        inspector[kvp.Key] = kvp.Value;
                    }

                    // Execute the edit operation
                    editOperation.Modify(inspector);
                    editOperation.ExecuteAsync();
                }    
            });

            progDialog.Hide();

            return true;
        }
    }
}
