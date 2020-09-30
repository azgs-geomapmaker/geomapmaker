using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace Geomapmaker {
	internal class Button1 : Button {
		protected override void OnClick() {
            //Testing how to activate map tool on button click. Looks like the ICommand route is the one
            var plugin = FrameworkApplication.GetPlugInWrapper("Geomapmaker_MapUnitPolyConstructionTool");
            if (plugin.Enabled) {
                if (plugin is Tool) {
                    FrameworkApplication.SetCurrentToolAsync("Geomapmaker_MapUnitPolyConstructionTool");
                } else {
                    var mv = MapView.Active;
                    var selectedLayers = mv.GetSelectedLayers();
                    IReadOnlyList<Layer> mapunitLayer = mv.Map.FindLayers("MapUnitPolys", true);
                    mv.SelectLayers(mapunitLayer);
                    selectedLayers = mv.GetSelectedLayers();
                    ((ICommand)plugin).Execute(null);
                }
            }
        }
	}
}
