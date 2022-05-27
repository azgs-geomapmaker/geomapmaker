using Geomapmaker.ViewModels.OrientationPoints;

namespace Geomapmaker.Views.OrientationPoints
{
    /// <summary>
    /// Interaction logic for OrientationPoints.xaml
    /// </summary>
    public partial class OrientationPoints : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public OrientationPointsViewModel orientationPointsViewModelVM = new OrientationPointsViewModel();

        public OrientationPoints()
        {
            InitializeComponent();
            DataContext = orientationPointsViewModelVM;
        }
    }
}
