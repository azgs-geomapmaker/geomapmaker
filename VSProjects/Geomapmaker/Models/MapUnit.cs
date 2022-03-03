using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Geomapmaker.Models
{
    public class MapUnit
    {
        public int ID { get; set; }

        public string MU { get; set; }

        public string Name { get; set; }

        public string FullName { get; set; }

        public string Age { get; set; }

        public string RelativeAge { get; set; }

        public string Description { get; set; }

        public string HierarchyKey { get; set; }

        public string ParagraphStyle { get; set; }

        public string Label { get; set; }

        public string Symbol { get; set; }

        public string AreaFillRGB { get; set; }

        public (double, double, double) RGB
        {
            get
            {
                // If the RGB values are seperated by commas
                if (!string.IsNullOrEmpty(AreaFillRGB) && AreaFillRGB.Count(a => a == ',') == 2)
                {
                    return (Convert.ToDouble(AreaFillRGB.Split(',')[0]), Convert.ToDouble(AreaFillRGB.Split(',')[1]), Convert.ToDouble(AreaFillRGB.Split(',')[2]));
                }
                // If the RGB values are seperated by semi-colons
                else if (!string.IsNullOrEmpty(AreaFillRGB) && AreaFillRGB.Count(a => a == ';') == 2)
                {
                    return (Convert.ToDouble(AreaFillRGB.Split(';')[0]), Convert.ToDouble(AreaFillRGB.Split(';')[1]), Convert.ToDouble(AreaFillRGB.Split(';')[2]));
                }

                // black
                return (0, 0, 0);
            }
        }

        public string HexColor { get; set; }

        public string DescriptionSourceID { get; set; }

        public string GeoMaterial { get; set; }

        public string GeoMaterialConfidence { get; set; }

        public string Tooltip
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                sb.Append($"<b>ID: </b>{ID}<br>");

                if (!string.IsNullOrEmpty(MU))
                {
                    sb.Append($"<b>MapUnit: </b>{MU}<br>");
                }

                sb.Append($"<b>Name: </b>{Name}<br>");

                if (!string.IsNullOrEmpty(FullName))
                {
                    sb.Append($"<b>FullName: </b>{FullName}<br>");
                }
                if (!string.IsNullOrEmpty(Age))
                {
                    sb.Append($"<b>Age: </b>{Age}<br>");
                }
                if (!string.IsNullOrEmpty(RelativeAge))
                {
                    sb.Append($"<b>RelativeAge: </b>{RelativeAge}<br>");
                }
                if (!string.IsNullOrEmpty(Description))
                {
                    sb.Append($"<b>Description: </b>{Description}<br>");
                }
                if (!string.IsNullOrEmpty(Label))
                {
                    sb.Append($"<b>Label: </b>{Label}<br>");
                }
                if (!string.IsNullOrEmpty(AreaFillRGB))
                {
                    sb.Append($"<b>AreaFillRGB: </b>{AreaFillRGB}<br>");
                }
                if (!string.IsNullOrEmpty(HexColor))
                {
                    sb.Append($"<b>HexColor: </b>{HexColor}<br>");
                }
                if (!string.IsNullOrEmpty(GeoMaterial))
                {
                    sb.Append($"<b>GeoMaterial: </b>{GeoMaterial}<br>");
                }
                if (!string.IsNullOrEmpty(GeoMaterialConfidence))
                {
                    sb.Append($"<b>GeoMaterialConfidence: </b>{GeoMaterialConfidence}<br>");
                }

                sb.Append($"<b>DescriptionSourceID: </b>{DescriptionSourceID}<br>");

                return sb.ToString();
            }
        }
    }

    public class MapUnitTreeItem : MapUnit
    {
        public bool IsExpanded { get; set; }

        public bool IsSelected { get; set; }

        public ObservableCollection<MapUnitTreeItem> Children { get; set; } = new ObservableCollection<MapUnitTreeItem>();

        public bool CanAcceptChildren => string.IsNullOrEmpty(ParagraphStyle) || ParagraphStyle == "Heading";

        public string ColorVisibility => ParagraphStyle == "Heading" ? "Collapsed" : "Visible";
    }

}
