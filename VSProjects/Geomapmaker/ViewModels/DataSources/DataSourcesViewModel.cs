using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Contracts;

namespace Geomapmaker.ViewModels.DataSources
{
    public class DataSourcesViewModel : ProWindow
    {
        public CreateDataSourceVM Create { get; set; } = new CreateDataSourceVM();
    }

    internal class ShowDataSources : Button
    {
        private Views.DataSources.DataSources _datasources = null;

        protected override void OnClick()
        {
            //already open?
            if (_datasources != null)
            {
                return;
            }

            _datasources = new Views.DataSources.DataSources
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            _datasources.Closed += (o, e) => { _datasources = null; };
            _datasources.Show();
        }
    }
}
