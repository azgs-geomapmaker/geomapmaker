using Geomapmaker.Models;
using System.Collections.ObjectModel;

namespace Geomapmaker.ViewModels
{
    public class DataSourceViewModel : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        // TODO Tooltips

        public DataSourceViewModel()
        {
            DataSources = new ObservableCollection<DataSource>(Data.DataSources.DataSourcesList);
        }

        public ObservableCollection<DataSource> DataSources { get; set; }

        public DataSource SelectedDataSource { get; set; }

    }
}
