using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.ContactsFaults
{
    public class ContactsFaultsViewModel : ProWindow, INotifyPropertyChanged
    {
        public ICommand CommandCancel => new RelayCommand((proWindow) =>
        {
            if (proWindow != null)
            {
                (proWindow as ProWindow).Close();
            }

        }, () => true);

        private List<CFSymbol> _cfSymbols { get; set; }
        public List<CFSymbol> CfSymbols
        {
            get => _cfSymbols;
            set
            {
                _cfSymbols = value;
                NotifyPropertyChanged();
            }
        }

        //Update collection of CF symbols
        public async void RefreshCFSymbols()
        {
            CfSymbols = await Data.CFSymbolOptions.GetCFSymbolOptions();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
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

            _contactsfaults.contactsFaultsVM.RefreshCFSymbols();

            _contactsfaults.Closed += (o, e) => { _contactsfaults = null; };

            _contactsfaults.Show();
        }
    }
}
