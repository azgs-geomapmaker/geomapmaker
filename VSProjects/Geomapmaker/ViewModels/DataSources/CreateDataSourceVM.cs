using ArcGIS.Desktop.Framework.Controls;

namespace Geomapmaker.ViewModels.DataSources
{
    public class CreateDataSourceVM : ProWindow
    {
        public string Source { get; set; }
        public string Notes { get; set; }
        public string Url { get; set; }
    }
}
