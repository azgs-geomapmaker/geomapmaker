using ArcGIS.Desktop.Framework.Contracts;

namespace Geomapmaker.ViewModels.OrientationPoints
{
    public class OrientationPointsViewModel
    {
    }

    internal class ShowOrientationPoints : Button
    {
        private Views.OrientationPoints.OrientationPoints _orientationpoints = null;

        protected override void OnClick()
        {
            //already open?
            if (_orientationpoints != null)
            {
                _orientationpoints.Close();
                return;
            }

            _orientationpoints = new Views.OrientationPoints.OrientationPoints
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            _orientationpoints.Closed += (o, e) => { _orientationpoints = null; };

            //_orientationpoints.stationsVM.WindowCloseEvent += (s, e) => _stations.Close();

            _orientationpoints.Show();
        }
    }

}
