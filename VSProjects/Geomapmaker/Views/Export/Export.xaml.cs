using Geomapmaker.ViewModels.Export;

namespace Geomapmaker.Views.Export
{
    /// <summary>
    /// Interaction logic for Export.xaml
    /// </summary>
    public partial class Export : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public ExportVM exportVM = new ExportVM();

        public Export()
        {
            InitializeComponent();
            DataContext = exportVM;
        }
    }
}
