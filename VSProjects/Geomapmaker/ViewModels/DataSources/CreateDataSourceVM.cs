using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Controls;
using System;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.DataSources
{
    public class CreateDataSourceVM : ProWindow
    {
        public ICommand CommandSave { get; }
        public ICommand CommandCancel { get; }

        public CreateDataSourceVM()
        {
            CommandSave = new RelayCommand(() => SubmitAsync(), () => CanSave());
            CommandCancel = new RelayCommand(() => ResetAsync());
        }

        private void ResetAsync()
        {
            throw new NotImplementedException();
        }

        private bool CanSave()
        {
            return true;
        }

        private void SubmitAsync()
        {
            throw new NotImplementedException();
        }

        public string Source { get; set; } = "Source";
        public string Notes { get; set; } = "Notes";
        public string Url { get; set; } = "Url";
    }
}
