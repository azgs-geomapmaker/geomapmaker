using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.ContactsFaults
{
    public class ContactsFaultsViewModel : ProWindow, INotifyPropertyChanged
    {
        public event EventHandler WindowCloseEvent;

        public ICommand CommandClose => new RelayCommand(() => CloseProwindow());

        public void CloseProwindow()
        {
            WindowCloseEvent(this, new EventArgs());
        }

        public CreateContactFaultVM Create { get; set; }
        public EditContactFaultVM Edit { get; set; }
        public DeleteContactFaultVM Delete { get; set; }

        public ContactsFaultsViewModel()
        {
            Create = new CreateContactFaultVM(this);
            Edit = new EditContactFaultVM(this);
            Delete = new DeleteContactFaultVM(this);
        }

        private List<GemsSymbol> _symbolOptions { get; set; }
        public List<GemsSymbol> SymbolOptions
        {
            get => _symbolOptions;
            set
            {
                _symbolOptions = value;
                NotifyPropertyChanged();
            }
        }

        private List<ContactFaultTemplate> _templates { get; set; }
        public List<ContactFaultTemplate> Templates
        {
            get => _templates;
            set
            {
                _templates = value;
                NotifyPropertyChanged();
            }
        }

        //Update collection of CF symbols
        public async void RefreshCFSymbolsAsync()
        {
            // Get symbology options if the list is null
            if (Data.Symbology.ContactsAndFaultsSymbols == null)
            {
                await Data.Symbology.RefreshCFSymbolOptions();
            }

            // ParentVM keeps a copy of the master list
            SymbolOptions = Data.Symbology.ContactsAndFaultsSymbols;

            // Push options to create vm
            Create.SymbolOptions = SymbolOptions;
            Create.SymbolsFilteredMessage = $"{SymbolOptions.Count()} symbols";
        }

        public async void RefreshTemplates()
        {
            // Get templates except for default and sketch
            Templates = await Data.ContactsAndFaults.GetContactFaultTemplatesAsync(true);
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

            // Symbology takes a few seconds to load. Display progress dialog
            ProgressorSource ps = new ProgressorSource("Generating symbology for Contact Faults");

            await QueuedTask.Run(() =>
            {
                _contactsfaults.contactsFaultsVM.RefreshCFSymbolsAsync();
                _contactsfaults.contactsFaultsVM.RefreshTemplates();
            }, ps.Progressor);

            _contactsfaults.Closed += (o, e) =>
            {
                // Switch back to map explore tool
                FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");

                _contactsfaults = null;
            };

            _contactsfaults.contactsFaultsVM.WindowCloseEvent += (s, e) => _contactsfaults.Close();

            _contactsfaults.Show();
        }
    }
}