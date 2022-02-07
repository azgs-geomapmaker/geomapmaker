using Geomapmaker.ViewModels.MapUnitPolys;

namespace Geomapmaker.Views.MapUnitPolys
{
    /// <summary>
    /// Interaction logic for MapUnitPolys.xaml
    /// </summary>
    public partial class MapUnitPolys : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public MapUnitPolysViewModel mapUnitPolysVM = new MapUnitPolysViewModel();

        public MapUnitPolys()
        {
            InitializeComponent();
            DataContext = mapUnitPolysVM;
        }
    }
}
