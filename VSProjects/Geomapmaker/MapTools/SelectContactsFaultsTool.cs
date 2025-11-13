using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.ViewModels.ContactsFaultsEdit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Geomapmaker.MapTools
{
    internal class SelectContactsFaultsTool : MapTool
    {
        public SelectContactsFaultsTool()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Rectangle;
            SketchOutputMode = SketchOutputMode.Map;

            // Reflection
            Assembly asm = Assembly.GetExecutingAssembly();

            // Path to custom cursor
            string uri = Path.GetDirectoryName(Uri.UnescapeDataString(new Uri(asm.CodeBase).LocalPath)) + "\\Cursors\\ContactsFaults.cur";

            if (File.Exists(uri))
            {
                // Create custom cursor from file
                Cursor = new System.Windows.Input.Cursor(uri);
            }
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            return base.OnToolActivateAsync(active);
        }

        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            FeatureLayer layer = MapView.Active?.Map?.GetLayersAsFlattenedList()?.OfType<FeatureLayer>()?.FirstOrDefault(l => l.Name == "ContactsAndFaults");

            IEnumerable<ContactsFaultsEditVM> viewModels = System.Windows.Application.Current.Windows.OfType<ContactsFaultsEditVM>();

            // Get the most recent window. GC takes some time to clean up the closed prowindows.
            ContactsFaultsEditVM contactsFaultsEditVM = viewModels.LastOrDefault();

            if (contactsFaultsEditVM == null)
            {
                return false;
            }

            await QueuedTask.Run(() =>
            {
                // Get features that intersect the point
                SelectionSet selection = MapView.Active.GetFeatures(geometry);

                // Get the CF layer from the selection
                FeatureLayer cfLayer = MapView.Active?.Map?.GetLayersAsFlattenedList()?.OfType<FeatureLayer>()?.FirstOrDefault(l => l.Name == "ContactsAndFaults");

                if (cfLayer != null && selection.Contains(cfLayer))
                {
                    // Get the ObjectIDs for the MapUnitPolys layer
                    List<long> oidsCF = selection[cfLayer].ToList();

                    if (oidsCF.Count > 0)
                    {
                        contactsFaultsEditVM.Set_CF_Oids(oidsCF);
                    }
                }

            });

            return true;
        }
    }
}
