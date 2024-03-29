﻿using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.ViewModels.MapUnitPolysCreate;
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
            SketchType = SketchGeometryType.Point;
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

            IEnumerable<MapUnitPolysCreateVM> viewModels = System.Windows.Application.Current.Windows.OfType<MapUnitPolysCreateVM>(); ;

            // Get the most recent window. GC takes some time to clean up the closed prowindows.
            MapUnitPolysCreateVM mapUnitPolysCreateVM = viewModels.LastOrDefault();

            if (mapUnitPolysCreateVM == null)
            {
                return false;
            }

            await QueuedTask.Run(() =>
            {
                // Get features that intersect the point
                Dictionary<BasicFeatureLayer, List<long>> selection = MapView.Active.GetFeatures(geometry);

                // Filter anything not CF
                FeatureLayer cfLayer = selection.Where(f => f.Key.Name == "ContactsAndFaults").FirstOrDefault().Key as FeatureLayer;

                // Select the oids
                List<long> oidsCF = selection[cfLayer];

                if (oidsCF.Count > 0)
                {
                    mapUnitPolysCreateVM.Set_CF_Oids(oidsCF);
                }
            });

            return true;
        }
    }
}
