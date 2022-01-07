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

        public static ObservableCollection<string> LocationConfidenceMeters => new ObservableCollection<string>() {
            "10",
            "25",
            "50",
            "100",
            "250",
        };
    }
}
