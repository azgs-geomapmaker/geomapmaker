using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace Geomapmaker.ViewModels.MapUnitPolysCreate
{
    public class MapUnitPolysCreateVM
    {
    }

    internal class ShowMapUnitPolysCreate : Button
    {

        private Views.MapUnitPolysCreate.MapUnitPolysCreate _mapunitpolyscreate = null;

        protected override void OnClick()
        {
            //already open?
            if (_mapunitpolyscreate != null)
                return;
            _mapunitpolyscreate = new Views.MapUnitPolysCreate.MapUnitPolysCreate();
            _mapunitpolyscreate.Owner = FrameworkApplication.Current.MainWindow;
            _mapunitpolyscreate.Closed += (o, e) => { _mapunitpolyscreate = null; };
            _mapunitpolyscreate.Show();
            //uncomment for modal
            //_mapunitpolyscreate.ShowDialog();
        }

    }
}
