using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace Geomapmaker
{
    internal class DataSourceButton : Button
    {
        private DataSourceWindow _dswindow = null;

        protected override async void OnClick()
        {
            //already open?
            if (_dswindow != null)
            {
                return;
            }

            await Data.DataSources.RefreshAsync();

            _dswindow = new DataSourceWindow();
            _dswindow.Owner = FrameworkApplication.Current.MainWindow;
            _dswindow.Closed += (o, e) => { _dswindow = null; };
            //_datasourcedialogprowindow.Show();
            //uncomment for modal
            _dswindow.ShowDialog();
        }

    }
}
