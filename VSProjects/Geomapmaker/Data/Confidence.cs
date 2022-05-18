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
            "-9",
            "10",
            "25",
            "50",
            "100",
            "250",
        };

        public static ObservableCollection<string> OrientationConfidenceDegrees => new ObservableCollection<string>() {
            "-9 (Unknown)",
            "5",
            "10",
        };

    }
}
