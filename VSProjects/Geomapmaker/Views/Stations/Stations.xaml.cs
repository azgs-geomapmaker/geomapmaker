using Geomapmaker.ViewModels.Stations;

namespace Geomapmaker.Views.Stations
{
    /// <summary>
    /// Interaction logic for Stations.xaml
    /// </summary>
    public partial class Stations : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public StationsViewModel stationsVM = new StationsViewModel();

        public Stations()
        {
            InitializeComponent();
            DataContext = stationsVM;
        }
    }
}
