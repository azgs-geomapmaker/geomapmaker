using ArcGIS.Desktop.Framework;
using Geomapmaker.Data;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.Tools
{
    public class SymbologyToolsVM
    {
        public ICommand CommandMups => new RelayCommand(() => RebuildMUPSymbologyAndTemplates());

        public ICommand CommandCf => new RelayCommand(() => RebuildContactsFaultsSymbology());

        public ICommand CommandOp => new RelayCommand(() => RebuildOrientationPointsSymbology());

        public ToolsViewModel ParentVM { get; set; }

        public SymbologyToolsVM(ToolsViewModel parentVM)
        {
            ParentVM = parentVM;
        }

        public void RebuildMUPSymbologyAndTemplates()
        {
            ParentVM.CloseProwindow();
            MapUnitPolys.RebuildMUPSymbologyAndTemplates();
        }

        public async void RebuildContactsFaultsSymbology()
        {
            ParentVM.CloseProwindow();
            await Symbology.RefreshCFSymbolOptionsAsync();
            ContactsAndFaults.RebuildContactsFaultsSymbology();
        }

        public async void RebuildOrientationPointsSymbology()
        {
            ParentVM.CloseProwindow();
            await Symbology.RefreshOPSymbolOptionsAsync();
            Data.OrientationPoints.RebuildOrientationPointsSymbology();
        }
    }
}
