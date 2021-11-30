using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using Geomapmaker.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.Headings
{
    public class HeadingsViewModel : ProWindow, INotifyPropertyChanged
    {
        public ICommand CommandCancel => new RelayCommand((proWindow) =>
        {
            if (proWindow != null)
            {
                (proWindow as ProWindow).Close();
            }

        }, () => true);

        public CreateHeadingVM Create { get; set; }
        public EditHeadingVM Edit { get; set; }
        public DeleteHeadingVM Delete { get; set; }

        public HeadingsViewModel()
        {
            Create = new CreateHeadingVM(this);
            Edit = new EditHeadingVM(this);
            Delete = new DeleteHeadingVM(this);
        }

        // Tooltips dictionary
        public Dictionary<string, string> Tooltips => new Dictionary<string, string>
        {
            // Dockpane Headings
            {"CreateHeading", "TODO CreateHeading" },
            {"EditHeading", "TODO EditHeading" },
            {"DeleteHeading", "TODO DeleteHeading" },

            // Control Labels
            {"Name", "TODO Name" },
            {"Description", "TODO Description" },
            
            // Heading Selection Comboboxes
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
            {"Name", 254 },
            {"Description", 3000 },
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

        private List<MapUnit> _headings { get; set; }
        public List<MapUnit> Headings
        {
            get => _headings;
            set
            {
                _headings = value;
                NotifyPropertyChanged();
            }
        }

        //Update collection of dmu
        public async Task RefreshMapUnitsAsync()
        {
            MapUnits = await Data.DescriptionOfMapUnits.GetMapUnitsAsync();
            Headings = MapUnits.Where(a => a.ParagraphStyle == "Heading").OrderBy(a => a.Name).ToList();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    internal class ShowHeadings : Button
    {
        private Views.Headings.Headings _headings = null;

        protected override async void OnClick()
        {
            if (_headings != null)
            {
                return;
            }

            _headings = new Views.Headings.Headings
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            await _headings.headingsVM.RefreshMapUnitsAsync();

            _headings.Closed += (o, e) => { _headings = null; };

            _headings.Show();
        }
    }
}