using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using System;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.Tools
{
    public class ToolsViewModel : ProWindow
    {
        public event EventHandler WindowCloseEvent;

        public ICommand CommandCancel => new RelayCommand(() => CloseProwindow());

        public TableToolsVM TableTools { get; set; }

        public SymbologyToolsVM SymbologyTools { get; set; }

        public ToolsViewModel()
        {
            TableTools = new TableToolsVM(this);
            SymbologyTools = new SymbologyToolsVM(this);
        }

        public void CloseProwindow()
        {
            WindowCloseEvent(this, new EventArgs());
        }
    }

    internal class ShowTools : Button
    {
        private Views.Tools.Tools _tools = null;

        protected override void OnClick()
        {
            //already open?
            if (_tools != null)
            {
                _tools.Close();
                return;
            }

            _tools = new Views.Tools.Tools
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            _tools.Closed += (o, e) => { _tools = null; };

            _tools.toolVM.WindowCloseEvent += (s, e) => _tools.Close();

            _tools.ShowDialog();
        }
    }
}
