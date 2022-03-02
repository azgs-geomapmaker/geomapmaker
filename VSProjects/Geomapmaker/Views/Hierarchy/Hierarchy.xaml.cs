using Geomapmaker.ViewModels.Hierarchy;

namespace Geomapmaker.Views.Hierarchy
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
