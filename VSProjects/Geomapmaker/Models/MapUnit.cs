using System;
using System.Collections.Generic;
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

        public Interval OlderInterval { get; set; }

        public Interval YoungerInterval { get; set; }

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
                if (hexcolor != null)
                {
                    int r = Convert.ToInt32(hexcolor.Substring(1, 2), 16);
                    int g = Convert.ToInt32(hexcolor.Substring(3, 2), 16);
                    int b = Convert.ToInt32(hexcolor.Substring(5, 2), 16);
                    return r + ";" + g + ";" + b;
                }
                else
                {
                    return null;
                }
            }
        }

        private string _hexcolor;
        public string hexcolor
        {
            //Depending on whether hexcolor is assigned from the database or from the view, it may have an alpha channel.
            //We don't want that. So, if it exists, we get rid of it in the get.
            get
            {
                return _hexcolor == null ? null :
                    _hexcolor.Length == 9 ? "#" + _hexcolor.Substring(3, 6) :
                                            "#" + _hexcolor.Substring(1, 6);
            }
            set
            {
                _hexcolor = value;
            }
        }

        public string DescriptionSourceID { get; set; }

        public string GeoMaterial { get; set; }

        public string GeoMaterialConfidence { get; set; }

        public int? ParentId { get; set; }

        // Only used to hold the tree-view. Not saved in db
        public List<MapUnit> Children { get; set; } = new List<MapUnit>();
    }
}
