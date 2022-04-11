using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.Validation
{
    public class ValidationViewModel
    {
        public event EventHandler WindowCloseEvent;

        public ICommand CommandCancel => new RelayCommand(() => CloseProwindow());

        public ICommand CommandValidate => new RelayCommand(() => Validate());

        public void Validate()
        {





        }

        public void CloseProwindow()
        {
            WindowCloseEvent(this, new EventArgs());
        }
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
