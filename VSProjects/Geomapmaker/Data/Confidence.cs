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

        // The USGS GeMS template has Domain values set for the ContactsAndFaults LocationConfidenceMeters values.
        // TODO: Remove the domain during database creation so we can use the -9 option
        public static ObservableCollection<string> ContactsFaultsLocationConfidenceMeters => new ObservableCollection<string>() {
            "10",
            "25",
            "50",
            "100",
            "250",
        };

        public static ObservableCollection<string> LocationConfidenceMeters => new ObservableCollection<string>() {
            "-9 (Unknown)",
            "10",
            "25",
            "50",
            "100",
            "250",
        };

        public static ObservableCollection<string> OrientationConfidenceDegrees => new ObservableCollection<string>() {
            "-9 (Unknown)"
        };
    }
}
