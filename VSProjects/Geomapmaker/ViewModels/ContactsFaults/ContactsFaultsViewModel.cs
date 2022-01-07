using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;

namespace Geomapmaker.ViewModels.ContactsFaults
{
    public class ContactsFaultsViewModel : ProWindow /*, INotifyPropertyChanged*/
    {
        public string Foo { get; set; } = "Foo123";
    }

    internal class ShowContactsFaults : Button
    {

        private Views.ContactsFaults.ContactsFaults _contactsfaults = null;

        protected override void OnClick()
        {
            //already open?
            if (_contactsfaults != null)
            {
                return;
            }

            _contactsfaults = new Views.ContactsFaults.ContactsFaults
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            _contactsfaults.Closed += (o, e) => { _contactsfaults = null; };

            _contactsfaults.Show();
        }
    }
}
