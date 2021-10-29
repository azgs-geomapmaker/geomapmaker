using Geomapmaker.Models;
using System.Collections.ObjectModel;

namespace Geomapmaker.ViewModels
{
    public class DataSourceViewModel : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        // TODO Tooltips

        public DataSourceViewModel()
        {
            var foo = Data.DataSources.DataSourcesList;
        }

        ObservableCollection<DataSource> DataSources { get; set; } = new ObservableCollection<DataSource>(Data.DataSources.DataSourcesList);

    }
}
