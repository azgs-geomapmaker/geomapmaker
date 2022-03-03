using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace Geomapmaker.ViewModels.Stations
{
    internal class ShowStations : Button
    {

        private Views.Stations.Stations _stations = null;

        protected override void OnClick()
        {
            //already open?
            if (_stations != null)
                return;
            _stations = new Views.Stations.Stations();
            _stations.Owner = FrameworkApplication.Current.MainWindow;
            _stations.Closed += (o, e) => { _stations = null; };
            _stations.Show();
            //uncomment for modal
            //_stations.ShowDialog();
        }

    }
}
