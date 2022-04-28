using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.Validation
{
    public class ValidationViewModel : ProWindow, INotifyPropertyChanged
    {
        public event EventHandler WindowCloseEvent;

        public ICommand CommandCancel => new RelayCommand(() => CloseProwindow());

        public OverviewVM Overview { get; set; }
        public GemsVM Gems { get; set; }
        public TopoVM Topo { get; set; }
        public GeomapmakerVM Geomapmaker { get; set; }

        public ValidationViewModel()
        {
            Overview = new OverviewVM(this);
            Gems = new GemsVM(this);
            Topo = new TopoVM(this);
            Geomapmaker = new GeomapmakerVM(this);
        }

        // Async validation
        public async void ValidateAsync(){
            await Gems.Validate();
            await Geomapmaker.Validate();
        }

        // Pass errors down to overview viewmodel
        public void UpdateGemsResults(int errorCount)
        {
            Overview.UpdateGemsResults(errorCount);
        }

        // Pass errors down to overview viewmodel
        public void UpdateTopoResults(int errorCount)
        {
            Overview.UpdateTopoResults(errorCount);
        }

        // Pass errors down to overview viewmodel
        public void UpdateGeomapmakerResults(int errorCount)
        {
            Overview.UpdateGeomapmakerResults(errorCount);
        }


        public void CloseProwindow()
        {
            WindowCloseEvent(this, new EventArgs());
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    internal class ShowValidation : Button
    {

        private Views.Validation.Validation _validation = null;

        protected override async void OnClick()
        {
            if (_validation != null)
            {
                _validation.Close();
                return;
            }

            _validation = new Views.Validation.Validation
            {
                Owner = FrameworkApplication.Current.MainWindow
            };

            // Progress dialog
            ProgressorSource ps = new ProgressorSource("Validating GeMS Project");

            await QueuedTask.Run(() =>
            {
                _validation.validationVM.ValidateAsync();
            }, ps.Progressor);

            _validation.Closed += (o, e) => { _validation = null; };

            _validation.validationVM.WindowCloseEvent += (s, e) => _validation.Close();

            _validation.Show();
        }
    }
}
