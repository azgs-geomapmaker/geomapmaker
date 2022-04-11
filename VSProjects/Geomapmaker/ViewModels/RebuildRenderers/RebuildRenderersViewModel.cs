using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using Geomapmaker.Data;
using System;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.RebuildRenderers
{
    public class RebuildRenderersViewModel
    {
        public event EventHandler WindowCloseEvent;

        public ICommand CommandCancel => new RelayCommand(() => CloseProwindow());

        public ICommand CommandAll => new RelayCommand(() => RebuildAll());

        public ICommand CommandMups => new RelayCommand(() => RebuildMUPSymbologyAndTemplates());

        public ICommand CommandCf => new RelayCommand(() => RebuildContactsFaultsSymbology());

        public ICommand CommandOp => new RelayCommand(() => RebuildOrientationPointsSymbology());

        public void RebuildAll()
        {
            Data.MapUnitPolys.RebuildMUPSymbologyAndTemplates();
            ContactsAndFaults.RebuildContactsFaultsSymbology();
            Data.OrientationPoints.RebuildOrientationPointsSymbology();
            CloseProwindow();
        }

        public void RebuildMUPSymbologyAndTemplates()
        {
            Data.MapUnitPolys.RebuildMUPSymbologyAndTemplates();
            CloseProwindow();
        }

        public void RebuildContactsFaultsSymbology()
        {
            ContactsAndFaults.RebuildContactsFaultsSymbology();
            CloseProwindow();
        }

        public void RebuildOrientationPointsSymbology()
        {
            Data.OrientationPoints.RebuildOrientationPointsSymbology();
            CloseProwindow();
        }

        public void CloseProwindow()
        {
            WindowCloseEvent(this, new EventArgs());
        }
    }

    internal class ShowRebuildRenderers : Button
    {

        private Views.RebuildRenderers.RebuildRenderers _rebuildrenderers = null;

        protected override void OnClick()
        {
            // already open?
            if (_rebuildrenderers != null)
            {
                _rebuildrenderers.Close();
                return;
            }

            _rebuildrenderers = new Views.RebuildRenderers.RebuildRenderers
            {
                Owner = FrameworkApplication.Current.MainWindow
            };
            _rebuildrenderers.Closed += (o, e) => { _rebuildrenderers = null; };

            _rebuildrenderers.rebuildRenderersVM.WindowCloseEvent += (s, e) => _rebuildrenderers.Close();

            _rebuildrenderers.Show();

        }

    }
}
