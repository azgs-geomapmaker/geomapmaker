using ArcGIS.Core.Data;
using ArcGIS.Desktop.Editing;
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
