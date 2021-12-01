using Geomapmaker.ViewModels;

namespace Geomapmaker.Views
{
    /// <summary>
    /// Interaction logic for Hierarchy.xaml
    /// </summary>
    public partial class Hierarchy : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public HierarchyViewModel hierarchyVM = new HierarchyViewModel();

        public Hierarchy()
        {
            InitializeComponent();
            DataContext = hierarchyVM;
        }
    }
}
