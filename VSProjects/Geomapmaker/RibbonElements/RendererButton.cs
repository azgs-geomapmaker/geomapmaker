using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Data;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Geomapmaker.RibbonElements
{
    internal class RendererButton : Button
    {
        protected override void OnClick()
        {
            ContactsAndFaults.ResetContactsFaultsSymbology();
            MapUnitPolys.RebuildMUPSymbologyAndTemplates();
        }
    }
}
