using Geomapmaker.ViewModels.Headings;

namespace Geomapmaker.Views.Headings
{
    /// <summary>
    /// Interaction logic for Headings.xaml
    /// </summary>
    public partial class Headings : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public HeadingsViewModel headingsVM = new HeadingsViewModel();

        public Headings()
        {
            InitializeComponent();
            DataContext = headingsVM;
        }
    }
}
