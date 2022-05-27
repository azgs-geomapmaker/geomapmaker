using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace Geomapmaker.ViewModels.MapUnitPolysEdit
{
    public class MapUnitPolysEditVM
    {
    }

    internal class ShowMapUnitPolysEdit : Button
    {
        private Views.MapUnitPolysEdit.MapUnitPolysEdit _mapunitpolysedit = null;

        protected override void OnClick()
        {
            //already open?
            if (_mapunitpolysedit != null)
                return;
            _mapunitpolysedit = new Views.MapUnitPolysEdit.MapUnitPolysEdit();
            _mapunitpolysedit.Owner = FrameworkApplication.Current.MainWindow;
            _mapunitpolysedit.Closed += (o, e) => { _mapunitpolysedit = null; };
            _mapunitpolysedit.Show();
            //uncomment for modal
            //_mapunitpolysedit.ShowDialog();
        }

    }
}
