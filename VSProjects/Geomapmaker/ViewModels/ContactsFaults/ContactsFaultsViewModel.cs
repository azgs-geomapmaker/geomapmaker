using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.ContactsFaults
{
    public class ContactsFaultsViewModel : ProWindow, INotifyPropertyChanged
    {
        public ICommand CommandClose => new RelayCommand((proWindow) =>
        {
            if (proWindow != null)
            {
                (proWindow as ProWindow).Close();
            }

        }, () => true);

        public CreateContactFaultVM Create { get; set; }
        public EditContactFaultVM Edit { get; set; }
        public DeleteContactFault Delete { get; set; }

        public ContactsFaultsViewModel()
        {
            Create = new CreateContactFaultVM(this);
            Edit = new EditContactFaultVM(this);
            Delete = new DeleteContactFault(this);
        }

        private List<CFSymbol> _symbolOptions { get; set; }
        public List<CFSymbol> SymbolOptions
        {
            get => _symbolOptions;
            set
            {
                _symbolOptions = value;
                NotifyPropertyChanged();
            }
        }

        //Update collection of CF symbols
        public async void RefreshCFSymbolsAsync()
        {
            // Get symbology options if the list is null
            if (Data.CFSymbology.CFSymbolOptionsList == null)
            {
                await Data.CFSymbology.RefreshCFSymbolOptions();
            }

            SymbolOptions = Data.CFSymbology.CFSymbolOptionsList;
            Create.SymbolOptions = SymbolOptions;
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

        protected override async void OnClick()
        {
            //already open?
            if (_contactsfaults != null)
            {
                _contactsfaults.Close();
                return;
            }

            _contactsfaults = new Views.ContactsFaults.ContactsFaults
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            await QueuedTask.Run(() =>
            {
                _contactsfaults.contactsFaultsVM.RefreshCFSymbolsAsync();
            });

            _contactsfaults.Closed += (o, e) =>
            {
                ResetMapTool(o, e);
                _contactsfaults = null;
            };

            _contactsfaults.Show();
        }

        private void ResetMapTool(object sender, EventArgs e)
        {
            FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
        }
    }
}