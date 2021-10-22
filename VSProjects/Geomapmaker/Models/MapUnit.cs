using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public string HexColor { get; set; }

        public string DescriptionSourceID { get; set; }

        public string GeoMaterial { get; set; }

        public string GeoMaterialConfidence { get; set; }
    }

    public class MapUnitTreeItem : MapUnit
    {
        public ObservableCollection<MapUnitTreeItem> Children { get; set; } = new ObservableCollection<MapUnitTreeItem>();

        public bool CanAcceptChildren => string.IsNullOrEmpty(ParagraphStyle) || ParagraphStyle == "Heading" ;
    }
}
