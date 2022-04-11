using Geomapmaker.ViewModels.RebuildRenderers;

namespace Geomapmaker.Views.RebuildRenderers
{
    /// <summary>
    /// Interaction logic for RebuildRenderers.xaml
    /// </summary>
    public partial class RebuildRenderers : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public RebuildRenderersViewModel rebuildRenderersVM = new RebuildRenderersViewModel();

        public RebuildRenderers()
        {
            InitializeComponent();
            DataContext = rebuildRenderersVM;
        }
    }
}
