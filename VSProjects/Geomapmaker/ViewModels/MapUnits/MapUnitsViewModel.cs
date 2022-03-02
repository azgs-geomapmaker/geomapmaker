using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.MapUnits
{
    public class MapUnitsViewModel : ProWindow, INotifyPropertyChanged
    {
        public event EventHandler WindowCloseEvent;

        public ICommand CommandCancel => new RelayCommand(() => CloseProwindow());

        public void CloseProwindow()
        {
            WindowCloseEvent(this, new EventArgs());
        }

        public CreateMapUnitVM Create { get; set; }
        public EditMapUnitVM Edit { get; set; }
        public DeleteMapUnitVM Delete { get; set; }

        public MapUnitsViewModel()
        {
            Create = new CreateMapUnitVM(this);
            Edit = new EditMapUnitVM(this);
            Delete = new DeleteMapUnitVM(this);
        }

        // Tooltips dictionary
        public Dictionary<string, string> Tooltips => new Dictionary<string, string>
        {
            // Dockpane Headings
            {"CreateHeading", "TODO CreateHeading" },
            {"EditHeading", "TODO EditHeading" },
            {"DeleteHeading", "TODO DeleteHeading" },

            // Control Labels
            {"MapUnit", "TODO MapUnit" },
            {"Name", "TODO Name" },
            {"FullName", "TODO FullName" },
            {"Age", "TODO Age" },
            {"OlderInterval", "TODO OlderInterval" },
            {"YoungerInterval", "TODO YoungerInterval" },
            {"RelativeAge", "TODO RelativeAge" },
            {"Description", "TODO Description" },
            {"Label", "TODO Label" },
            {"Color", "TODO Color" },
            {"GeoMaterial", "TODO GeoMaterial" },
            {"GeoMaterialConfidence", "TODO GeoMaterialConfidence" },

            // Map Unit Selection Comboboxes 
            {"Edit", "TODO Edit" },
            {"Delete", "TODO Delete" },

            // Buttons
            {"ClearButton", "TODO ClearButton" },
            {"SaveButton", "TODO SaveButton" },
            {"UpdateButton", "TODO UpdateButton" },
            {"DeleteButton", "TODO DeleteButton" },
        };

        // Max length of the field's string
        public Dictionary<string, int> MaxLength => new Dictionary<string, int>
        {
            {"MapUnit", 10 },
            {"Name", 254 },
            {"FullName", 254 },
            {"RelativeAge", 254 },
            {"Description", 3000 },
            {"Label", 30 },
        };

        private List<MapUnit> _mapUnits { get; set; }
        public List<MapUnit> MapUnits
        {
            get => _mapUnits;
            set
            {
                _mapUnits = value;
                NotifyPropertyChanged();
            }
        }

        private List<MapUnit> _standardDMUs { get; set; }
        public List<MapUnit> StandardDMUs
        {
            get => _standardDMUs;
            set
            {
                _standardDMUs = value;
                NotifyPropertyChanged();
            }
        }

        //Update collection of dmu
        public async void RefreshMapUnitsAsync()
        {
            MapUnits = await Data.DescriptionOfMapUnits.GetMapUnitsAsync();
            StandardDMUs = MapUnits.Where(a => a.ParagraphStyle == "Standard").OrderBy(a => a.Name).ToList();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    internal class ShowMapUnits : Button
    {
        private Views.MapUnits.MapUnits _mapunits = null;

        protected override void OnClick()
        {
            if (_mapunits != null)
            {
                _mapunits.Close();
                return;
            }

            _mapunits = new Views.MapUnits.MapUnits
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            _mapunits.mapUnitsVM.RefreshMapUnitsAsync();

            _mapunits.Closed += (o, e) => { _mapunits = null; };

            _mapunits.mapUnitsVM.WindowCloseEvent += (s, e) => _mapunits.Close();

            _mapunits.Show();

        }

    }
}