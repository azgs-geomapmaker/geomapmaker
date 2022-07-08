using Geomapmaker.ViewModels.Tools;

namespace Geomapmaker.Views.Tools
{
    /// <summary>
    /// Interaction logic for Tools.xaml
    /// </summary>
    public partial class Tools : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public ToolsViewModel toolVM = new ToolsViewModel();

        public Tools()
        {
            InitializeComponent();
            DataContext = toolVM;
        }
    }
}
