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


        public AZGSVM AZGSVM { get; set; }
        public Stage1VM Stage1VM { get; set; }
        public Stage2VM Stage2VM { get; set; }
        public Stage3VM Stage3VM { get; set; }

        public ValidationViewModel()
        {
            AZGSVM = new AZGSVM(this);
            Stage1VM = new Stage1VM(this);
            Stage2VM = new Stage2VM(this);
            Stage3VM = new Stage3VM(this);
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
