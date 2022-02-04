using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace Geomapmaker.ViewModels.MapUnitPolys
{
    public class MapUnitPolysViewModel
    {

    }

    internal class ShowMapUnitPolys : Button
    {
        private Views.MapUnitPolys.MapUnitPolys _mapunitpolys = null;

        protected override void OnClick()
        {
            //already open?
            if (_mapunitpolys != null)
                return;
            _mapunitpolys = new Views.MapUnitPolys.MapUnitPolys();
            _mapunitpolys.Owner = FrameworkApplication.Current.MainWindow;
            _mapunitpolys.Closed += (o, e) => { _mapunitpolys = null; };
            _mapunitpolys.Show();
            //uncomment for modal
            //_mapunitpolys.ShowDialog();
        }

    }
}
