using Geomapmaker.ViewModels.MapUnitPolysCreate;

namespace Geomapmaker.Views.MapUnitPolysCreate
{
    /// <summary>
    /// Interaction logic for MapUnitPolysCreate.xaml
    /// </summary>
    public partial class MapUnitPolysCreate : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public MapUnitPolysCreateVM mapUnitPolysCreateVM = new MapUnitPolysCreateVM();

        public MapUnitPolysCreate()
        {
            InitializeComponent();
            DataContext = mapUnitPolysCreateVM;
        }
    }
}
