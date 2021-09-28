using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Geomapmaker.Models
{
    public class MapUnitTree
    {
        public string Name { get; set; }

        public IList<MapUnitTree> Children { get; set; } = new List<MapUnitTree>();
    }
}
