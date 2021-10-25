using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Contracts;

namespace Geomapmaker
{
	internal class GeomapmakerConfiguration : ConfigurationManager
	{
		#region Override App Name and Icon

		/// <summary>
		/// Replaces the default ArcGIS Pro application name
		/// </summary>
		protected override string ApplicationName => "GeomapMaker";

        #endregion Override App Name and Icon

        #region Override DAML Database

        protected override void OnUpdateDatabase(XDocument database)
        {
            Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!OnUpdateDatabase enter");
            try
            {
                if (database?.Root != null)
                {
                    var nsp = database.Root.Name.Namespace;
                    var tabElements = from seg in database.Root.Descendants(nsp + "tab") select seg;
                    var elements = new HashSet<XElement>();
                    foreach (var tabElement in tabElements)
                    {
                        if (tabElement.Parent == null
                            || tabElement.Parent.Name.LocalName.StartsWith("backstage"))
                            continue;
                        var id = tabElement.Attribute("id");
                        if (id == null) continue;

                        if (!id.Value.StartsWith("AZGS") && !id.Value.StartsWith("Geomapmaker"))
                            elements.Add(tabElement);
                        else
                        {
                            Debug.WriteLine($@"Keep: {id}");
                        }
                    }
                    foreach (var element in elements)
                    {
                        element.Remove();
                    }
                }
             }
            catch (Exception ex)
            {
                Debug.WriteLine($@"Error in update database: {ex}");
            }
        }

        #endregion


    }
}
