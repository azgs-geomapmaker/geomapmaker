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

        public string AreaFillRGB
        {
            //hexcolor is the primary color holder, and is bound to the view. AreaFilleRGB just reformats that into xxx;xxx;xxx.
            get
            {
                if (HexColor != null)
                {
                    int r = Convert.ToInt32(HexColor.Substring(1, 2), 16);
                    int g = Convert.ToInt32(HexColor.Substring(3, 2), 16);
                    int b = Convert.ToInt32(HexColor.Substring(5, 2), 16);
                    return r + ";" + g + ";" + b;
                }
                else
                {
                    return null;
                }
            }
        }

        public string HexColor { get; set; }

        public string DescriptionSourceID { get; set; }

        public string GeoMaterial { get; set; }

        public string GeoMaterialConfidence { get; set; }

        // Only used to hold the tree-view. Not saved in db
        public ObservableCollection<MapUnit> Children { get; set; } = new ObservableCollection<MapUnit>();

        public bool CanAcceptChildren { get; set; }

    }
}
