using System.Collections.ObjectModel;

namespace Geomapmaker.Data
{
    public class Confidence
    {
        public static ObservableCollection<string> ConfidenceOptions => new ObservableCollection<string>()
        {
            "High",
            "Medium",
            "Low"
        };
    }
}
