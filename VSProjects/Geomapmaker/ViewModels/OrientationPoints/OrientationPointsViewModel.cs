using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.OrientationPoints
{
    public class OrientationPointsViewModel
    {
        public event EventHandler WindowCloseEvent;

        public ICommand CommandCancel => new RelayCommand(() => CloseProwindow());

        public void CloseProwindow()
        {
            WindowCloseEvent(this, new EventArgs());
        }

        public CreateOrientationPointVM Create { get; set; }

        public OrientationPointsViewModel()
        {
            Create = new CreateOrientationPointVM(this);
        }
    }

    internal class ShowOrientationPoints : Button
    {
        private Views.OrientationPoints.OrientationPoints _orientationpoints = null;

        protected override void OnClick()
        {
            //already open?
            if (_orientationpoints != null)
            {
                _orientationpoints.Close();
                return;
            }

            _orientationpoints = new Views.OrientationPoints.OrientationPoints
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            _orientationpoints.Closed += (o, e) => { _orientationpoints = null; };

            _orientationpoints.orientationPointsViewModelVM.WindowCloseEvent += (s, e) => _orientationpoints.Close();

            _orientationpoints.Show();
        }
    }

}
