using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using Geomapmaker.Views.DataSources;

namespace Geomapmaker.RibbonElements
{
    internal class ShowDataSources : Button
    {

        private DataSources _datasources = null;

        protected override void OnClick()
        {
            //already open?
            if (_datasources != null)
                return;
            _datasources = new DataSources();
            _datasources.Owner = FrameworkApplication.Current.MainWindow;
            _datasources.Closed += (o, e) => { _datasources = null; };
            _datasources.Show();
            //uncomment for modal
            //_datasources.ShowDialog();
        }

    }
}
