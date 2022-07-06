using ArcGIS.Desktop.Framework;
using Geomapmaker.Data;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.Tools
{
    public class SymbologyToolsVM
    {
        public ICommand CommandAll => new RelayCommand(() => RebuildAll());

        public ICommand CommandMups => new RelayCommand(() => RebuildMUPSymbologyAndTemplates());

        public ICommand CommandCIMSymbols => new RelayCommand(() => RebuildCIMSymbols());

        public ICommand CommandCf => new RelayCommand(() => RebuildContactsFaultsSymbology());

        public ICommand CommandOp => new RelayCommand(() => RebuildOrientationPointsSymbology());

        ToolsViewModel ParentVM { get; set; }

        public SymbologyToolsVM(ToolsViewModel parentVM)
        {
            ParentVM = parentVM;
        }

        public async void RebuildAll()
        {
            await Data.Symbology.RefreshCFSymbolOptionsAsync();
            await Data.Symbology.RefreshOPSymbolOptionsAsync();
            Data.MapUnitPolys.RebuildMUPSymbologyAndTemplates();
            Data.ContactsAndFaults.RebuildContactsFaultsSymbology();
            Data.OrientationPoints.RebuildOrientationPointsSymbology();
            ParentVM.CloseProwindow();
        }

        public async void RebuildCIMSymbols()
        {
            await Data.Symbology.RefreshCFSymbolOptionsAsync();
            await Data.Symbology.RefreshOPSymbolOptionsAsync();
            ParentVM.CloseProwindow();
        }

        public void RebuildMUPSymbologyAndTemplates()
        {
            Data.MapUnitPolys.RebuildMUPSymbologyAndTemplates();
            ParentVM.CloseProwindow();
        }

        public void RebuildContactsFaultsSymbology()
        {
            ContactsAndFaults.RebuildContactsFaultsSymbology();
            ParentVM.CloseProwindow();
        }

        public void RebuildOrientationPointsSymbology()
        {
            Data.OrientationPoints.RebuildOrientationPointsSymbology();
            ParentVM.CloseProwindow();
        }
    }
}
