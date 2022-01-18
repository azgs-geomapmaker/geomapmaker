using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using Geomapmaker.ViewModels.ContactsFaults;
using System.Collections.Generic;
using System.Linq;
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

            IEnumerable<ContactsFaultsViewModel> cfWindowVMs = FrameworkApplication.Current.Windows.OfType<ContactsFaultsViewModel>(); ;

            if (cfWindowVMs.Count() < 1)
            {
                return true;
            }

            // Get the most recent window. GC takes some time to clean up the closed prowindows.
            ContactsFaultsViewModel cfViewModel = cfWindowVMs.Last();

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
                IEnumerable<KeyValuePair<BasicFeatureLayer, List<long>>> cfFeatures = features.Where(x => x.Key.Name == "ContactsAndFaults");

                if (cfFeatures.Count() > 0 && cfFeatures.First().Value.Count() > 0)
                {
                    long cfID = cfFeatures.First().Value.First();

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
                                ContactFault cf = new ContactFault
                                {
                                    Type = row["type"].ToString(),
                                    IdentityConfidence = row["identityconfidence"].ToString(),
                                    ExistenceConfidence = row["existenceconfidence"].ToString(),
                                    LocationConfidenceMeters = row["locationconfidencemeters"].ToString(),
                                    IsConcealed = row["isconcealed"].ToString() == "Y",
                                    Notes = row["notes"] == null ? "" : row["notes"].ToString()
                                };

                                // Pass values back to the ViewModel to prepop
                                cfViewModel.Create.PrepopulateCF(cf);
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
