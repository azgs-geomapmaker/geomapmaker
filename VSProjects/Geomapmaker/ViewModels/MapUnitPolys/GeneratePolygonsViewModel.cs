using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Editing.Templates;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.MapUnitPolys
{
    public class GeneratePolygonsViewModel
    {
        public event EventHandler WindowCloseEvent;

        public ICommand CommandGeneratePolygons => new RelayCommand(() => GenerateAllPolygons());

        public ICommand CommandCancel => new RelayCommand(() => CloseProwindow());

        public async void GenerateAllPolygons()
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
                string tempTemplateName = GeomapmakerModule.MUP_UnassignedTemplateName;

                // Get the temp template
                EditingTemplate tmpTemplate = polyLayer.GetTemplate(tempTemplateName);

                // Check if null
                if (tmpTemplate == null)
                {
                    Inspector insp = new Inspector();
                    insp.LoadSchema(polyLayer);

                    insp["MapUnit"] = tempTemplateName;
                    insp["DataSourceID"] = tempTemplateName;
                    insp["IdentityConfidence"] = tempTemplateName;
                    insp["Label"] = null;
                    insp["Symbol"] = null;
                    insp["Notes"] = tempTemplateName;

                    // Create an empty template 
                    EditingTemplate newTemplate = polyLayer.CreateTemplate(tempTemplateName, tempTemplateName, insp);
                    
                    // Get the empty template
                    tmpTemplate = polyLayer.GetTemplate(tempTemplateName);
                }

                using (Selection cf_Collection = cfLayer.Select())
                {
                    IReadOnlyList<long> ContactFaultOids = cf_Collection.GetObjectIDs();

                    op.ConstructPolygons(tmpTemplate, cfLayer, ContactFaultOids, null, true);

                    op.Execute();
                }

                // Remove the temp template
                polyLayer.RemoveTemplate(tempTemplateName);

                // Clear selection
                cfLayer.ClearSelection();
                polyLayer.ClearSelection();

                // Rebuild symbology
                Data.MapUnitPolys.RebuildMUPSymbologyAndTemplates();
            });

            CloseProwindow();
        }

        public void CloseProwindow()
        {
            WindowCloseEvent(this, new EventArgs());
        }
    }

    internal class ShowGeneratePolygons : Button
    {
        private Views.MapUnitPolys.GeneratePolygons _generatepolygons = null;

        protected override void OnClick()
        {
            // already open?
            if (_generatepolygons != null)
            {
                _generatepolygons.Close();
                return;
            }

            _generatepolygons = new Views.MapUnitPolys.GeneratePolygons
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            
            _generatepolygons.Closed += (o, e) => { _generatepolygons = null; };

            _generatepolygons.generatePolygonsVM.WindowCloseEvent += (s, e) => _generatepolygons.Close();

            _generatepolygons.Show();

        }
    }
}
