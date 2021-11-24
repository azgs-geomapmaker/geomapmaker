using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Controls;
using System;
using System.Windows.Input;

namespace Geomapmaker.ViewModels.DataSources
{
    public class CreateDataSourceVM : ProWindow
    {
        public string Source { get; set; }
        public string Notes { get; set; }
        public string Url { get; set; }

        public ICommand CommandSave { get; }

        public CreateDataSourceVM()
        {
            CommandSave = new RelayCommand(() => SubmitAsync(), () => CanSave());
        }

        private bool CanSave()
        {
            return true;
        }

        private void SubmitAsync()
        {
            throw new NotImplementedException();
        }
    }
}
