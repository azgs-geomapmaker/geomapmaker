namespace Geomapmaker.Models
{
    public class Station
    {
        // Specifies type of point feature represented by this data point
        public string Type { get; set; }

        // References a point symbol
        public string Symbol { get; set; }

        // Describes text label to accompany this point symbol
        public string Label { get; set; }

        // Radius (in meters) of positional-uncertainty envelope around this point feature
        public string LocationConfidenceMeters { get; set; }

        // Denominator of smallest map scale at which this point feature should be plotted on map
        // (that is, it should not be plotted at smaller map scales)
        public string PlotAtScale { get; set; }

        // Foreign key to Stations point feature class
        public string StationsID { get; set; }

        // Records map unit to which this analysis or observation pertains.
        // Foreign key to DescriptionOfMapUnits table
        public string MapUnit { get; set; }

        // Optional field
        public string Notes { get; set; }

        // Primary key
        public string Stations_ID { get; set; }
    }

}
