using ArcGIS.Desktop.Framework;
using Geomapmaker.Data;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.Tools
{
    public class SymbologyToolsVM
    {
        public ICommand CommandCIMSymbols => new RelayCommand(() => RebuildCIMSymbols());

        public ICommand CommandMups => new RelayCommand(() => RebuildMUPSymbologyAndTemplates());

        public ICommand CommandCf => new RelayCommand(() => RebuildContactsFaultsSymbology());

        public ICommand CommandOp => new RelayCommand(() => RebuildOrientationPointsSymbology());

        public ToolsViewModel ParentVM { get; set; }

        public SymbologyToolsVM(ToolsViewModel parentVM)
        {
            ParentVM = parentVM;
        }

        public async void RebuildCIMSymbols()
        {
            ParentVM.CloseProwindow();
            await Symbology.RefreshCFSymbolOptionsAsync();
            await Symbology.RefreshOPSymbolOptionsAsync();
        }

        public void RebuildMUPSymbologyAndTemplates()
        {
            ParentVM.CloseProwindow();
            MapUnitPolys.RebuildMUPSymbologyAndTemplates();
        }

        public void RebuildContactsFaultsSymbology()
        {
            ParentVM.CloseProwindow();
            ContactsAndFaults.RebuildContactsFaultsSymbology();
        }

        public void RebuildOrientationPointsSymbology()
        {
            ParentVM.CloseProwindow();
            Data.OrientationPoints.RebuildOrientationPointsSymbology();
        }
    }
}
