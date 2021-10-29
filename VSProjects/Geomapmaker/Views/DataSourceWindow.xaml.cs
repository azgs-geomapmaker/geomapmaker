using Geomapmaker.ViewModels;

namespace Geomapmaker
{
    /// <summary>
    /// Interaction logic for DataSourceDialogProWindow.xaml
    /// </summary>
    public partial class DataSourceWindow : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public DataSourceWindow()
        {
            InitializeComponent();

            DataContext = new DataSourceViewModel();
        }
    }
}
