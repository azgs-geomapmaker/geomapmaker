using Geomapmaker.ViewModels.GeneratePolygons;

namespace Geomapmaker.Views.GeneratePolygons
{
    /// <summary>
    /// Interaction logic for GeneratePolygons.xaml
    /// </summary>
    public partial class GeneratePolygons : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public GeneratePolygonsViewModel generatePolygonsVM = new GeneratePolygonsViewModel();

        public GeneratePolygons()
        {
            InitializeComponent();
            DataContext = generatePolygonsVM;
        }
    }
}
