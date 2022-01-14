using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geomapmaker.MapTools
{
    internal class PopulateCFTool : MapTool
    {
        public PopulateCFTool()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Map;
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            return base.OnToolActivateAsync(active);
        }

        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            if (!(MapView.Active?.Map.Layers.FirstOrDefault(a => a.Name == "ContactsAndFaults") is FeatureLayer cfFeatureLayer))
            {
                return true;
            }

            await QueuedTask.Run(() =>
            {
                MapView mv = MapView.Active;

                if (mv == null)
                {
                    return;
                }

                // Get the features that intersect the sketch geometry. 
                // GetFeatures() returns a dictionary of featurelayer and a list of Object ids for each
                Dictionary<BasicFeatureLayer, List<long>> features = mv.GetFeatures(geometry);

                // Flash features on the map
                MapView.Active.FlashFeature(features);

                // Filter out non-cf features
                var cfFeatures = features.Where(x => x.Key.Name == "ContactsAndFaults");

                if (cfFeatures.Count() > 0 && cfFeatures.First().Value.Count() > 0)
                {
                    var cfID = cfFeatures.First().Value.First();

                    Table enterpriseTable = cfFeatureLayer.GetTable();

                    QueryFilter queryFilter = new QueryFilter
                    {
                        WhereClause = "objectid in (" + cfID + ")"
                    };

                    using (RowCursor rowCursor = enterpriseTable.Search(queryFilter, false))
                    {
                        if (rowCursor.MoveNext())
                        {
                            using (Row row = rowCursor.Current)
                            {
                                //populate a CF from fields
                                var cf = new ContactFault();
                                //cf.Symbol = DataHelper.CFSymbols.Where(cfs => cfs.Key == row["symbol"].ToString()).First();
                                cf.IdentityConfidence = row["identityconfidence"].ToString();
                                cf.ExistenceConfidence = row["existenceconfidence"].ToString();
                                cf.LocationConfidenceMeters = row["locationconfidencemeters"].ToString();
                                cf.IsConcealed = row["isconcealed"].ToString() == "Y";
                                cf.Notes = row["notes"] == null ? "" : row["notes"].ToString();


                            }
                        }
                    }
                }
            });

            // Return if the sketch complete event was handled.
            return true;
        }
    }
}
