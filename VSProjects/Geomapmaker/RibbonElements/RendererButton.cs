using ArcGIS.Desktop.Framework.Contracts;
using Geomapmaker.Data;

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
