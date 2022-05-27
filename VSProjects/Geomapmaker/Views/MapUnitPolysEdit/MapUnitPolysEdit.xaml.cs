using Geomapmaker.ViewModels.MapUnitPolysEdit;

namespace Geomapmaker.Views.MapUnitPolysEdit
{
    /// <summary>
    /// Interaction logic for MapUnitPolysEdit.xaml
    /// </summary>
    public partial class MapUnitPolysEdit : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public MapUnitPolysEditVM mapUnitPolysEditVM = new MapUnitPolysEditVM();

        public MapUnitPolysEdit()
        {
            InitializeComponent();
            DataContext = mapUnitPolysEditVM;
        }
    }
}
