using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.Validation
{
    public class ValidationViewModel : ProWindow, INotifyPropertyChanged
    {
        public event EventHandler WindowCloseEvent;

        public ICommand CommandCancel => new RelayCommand(() => CloseProwindow());

        public OverviewVM Overview { get; set; }
        public Level1VM Level1 { get; set; }
        public Level2VM Level2 { get; set; }
        public Level3VM Level3 { get; set; }
        public GeomapmakerVM AZGS { get; set; }

        public ValidationViewModel()
        {
            Overview = new OverviewVM(this);
            Level1 = new Level1VM(this);
            Level2 = new Level2VM(this);
            Level3 = new Level3VM(this);
            AZGS = new GeomapmakerVM(this);
        }

        public void UpdateLevel1Results(int errorCount)
        {
            Overview.UpdateLevel1Results(errorCount);
        }

        public void UpdateLevel2Results(int errorCount)
        {
            Overview.UpdateLevel2Results(errorCount);
        }

        public void UpdateLevel3Results(int errorCount)
        {
            Overview.UpdateLevel3Results(errorCount);
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

        protected override void OnClick()
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

            _validation.Closed += (o, e) => { _validation = null; };

            _validation.validationVM.WindowCloseEvent += (s, e) => _validation.Close();

            _validation.Show();
        }
    }
}
