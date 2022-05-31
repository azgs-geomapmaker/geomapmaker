using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace Geomapmaker.ViewModels.Export
{
    public class ExportVM
    {
    }

    internal class ShowExport : Button
    {

        private Views.Export.Export _export = null;

        protected override void OnClick()
        {
            //already open?
            if (_export != null)
                return;
            _export = new Views.Export.Export();
            _export.Owner = FrameworkApplication.Current.MainWindow;
            _export.Closed += (o, e) => { _export = null; };
            _export.Show();
            //uncomment for modal
            //_export.ShowDialog();
        }

    }
}
