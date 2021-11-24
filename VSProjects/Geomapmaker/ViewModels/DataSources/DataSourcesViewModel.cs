using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Contracts;
using System.Windows.Input;
using System;
using ArcGIS.Desktop.Framework;

namespace Geomapmaker.ViewModels.DataSources
{
    public class DataSourcesViewModel : ProWindow
    {
        public ICommand CommandCancel => new RelayCommand((proWindow) =>
        {
            if (proWindow != null)
            {
                (proWindow as ProWindow).Close();
            }

        }, () => true);

        public CreateDataSourceVM Create { get; set; } = new CreateDataSourceVM();
    }

    internal class ShowDataSources : Button
    {
        private Views.DataSources.DataSources _datasources = null;

        protected override void OnClick()
        {
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
