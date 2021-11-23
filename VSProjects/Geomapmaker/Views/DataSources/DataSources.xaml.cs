using Geomapmaker.ViewModels.DataSources;

namespace Geomapmaker.Views.DataSources
{
    /// <summary>
    /// Interaction logic for DataSources.xaml
    /// </summary>
    public partial class DataSources : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        DataSourcesViewModel dataSourcesVM = new DataSourcesViewModel();

        public DataSources()
        {
            InitializeComponent();
            DataContext = dataSourcesVM;
        }
    }
}
