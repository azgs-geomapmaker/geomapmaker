namespace Geomapmaker.Data
{
    public class ArcGisProject
    {
        static public string GetName()
        {
            // Get the project name
            string projectName = ArcGIS.Desktop.Core.Project.Current?.Name ?? "Geomapmaker";

            // Remove extension
            return projectName.Replace(".aprx", "");
        }
    }
}
