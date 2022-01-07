using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.ContactsFaults
{
    public class ContactsFaultsViewModel : ProWindow /*, INotifyPropertyChanged*/
    {

    }

    internal class ShowContactsFaults : Button
    {

        private Views.ContactsFaults.ContactsFaults _contactsfaults = null;

        protected override void OnClick()
        {
            //already open?
            if (_contactsfaults != null)
                return;
            _contactsfaults = new Views.ContactsFaults.ContactsFaults();
            _contactsfaults.Owner = FrameworkApplication.Current.MainWindow;
            _contactsfaults.Closed += (o, e) => { _contactsfaults = null; };
            _contactsfaults.Show();
            //uncomment for modal
            //_contactsfaults.ShowDialog();
        }

    }
}
