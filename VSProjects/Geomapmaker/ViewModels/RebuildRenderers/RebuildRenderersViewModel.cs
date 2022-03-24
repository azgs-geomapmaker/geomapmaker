using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace Geomapmaker.ViewModels.RebuildRenderers
{
    public class RebuildRenderersViewModel
    {
    }

    internal class ShowRebuildRenderers : Button
    {

        private Geomapmaker.RebuildRenderers _rebuildrenderers = null;

        protected override void OnClick()
        {
            //already open?
            if (_rebuildrenderers != null)
                return;
            _rebuildrenderers = new Geomapmaker.RebuildRenderers();
            _rebuildrenderers.Owner = FrameworkApplication.Current.MainWindow;
            _rebuildrenderers.Closed += (o, e) => { _rebuildrenderers = null; };
            _rebuildrenderers.Show();
            //uncomment for modal
            //_rebuildrenderers.ShowDialog();
        }

    }
}
