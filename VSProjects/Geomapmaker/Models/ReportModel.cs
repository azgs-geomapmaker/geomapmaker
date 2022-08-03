namespace Geomapmaker.Models
{
    class ReportModel
    {
        public string ProjectName { get; set; }

        public string ReportDate { get; set; }

        public string Wkid { get; set; }
        
        public string SpatialRefName { get; set; }
        
        public string Spheroid { get; set; }
        
        public string Projected { get; set; }
        
        public string Unit { get; set; }
    }


    //string wkid = MapView.Active?.Map?.SpatialReference?.Wkid.ToString();

    //string spatialRefName = MapView.Active?.Map?.SpatialReference?.Name;

    //string spheroid = MapView.Active?.Map?.SpatialReference?.Datum?.SpheroidName;

    //string projected = MapView.Active?.Map?.SpatialReference.IsProjected == true ? "True" : "False";

    //string unit = MapView.Active?.Map?.SpatialReference?.Unit?.Name;
}
