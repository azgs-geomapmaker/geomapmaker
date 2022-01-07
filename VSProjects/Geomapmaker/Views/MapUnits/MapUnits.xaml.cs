using Geomapmaker.ViewModels.MapUnits;

namespace Geomapmaker.Views.MapUnits
{
    /// <summary>
    /// Interaction logic for MapUnits.xaml
    /// </summary>
    public partial class MapUnits : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public MapUnitsViewModel mapUnitsVM = new MapUnitsViewModel();

        public MapUnits()
        {
            InitializeComponent();
            DataContext = mapUnitsVM;
        }
    }
}
