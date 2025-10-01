using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using Geomapmaker.ViewModels.ContactsFaults;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

            // Reflection
            Assembly asm = Assembly.GetExecutingAssembly();

            // Path to custom cursor
            string uri = Path.GetDirectoryName(Uri.UnescapeDataString(new Uri(asm.CodeBase).LocalPath)) + "\\Cursors\\EyeDropper.cur";

            if (File.Exists(uri))
            {
                // Create custom cursor from file
                Cursor = new System.Windows.Input.Cursor(uri);
            }
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            return base.OnToolActivateAsync(active);
        }

        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            FeatureLayer cfFeatureLayer = (FeatureLayer)(MapView.Active?.Map.Layers.FirstOrDefault(a => a.Name == "ContactsAndFaults"));

            IEnumerable<ContactsFaultsViewModel> cfWindowVMs = System.Windows.Application.Current.Windows.OfType<ContactsFaultsViewModel>(); ;

            // Get the most recent window. GC takes some time to clean up the closed prowindows.
            ContactsFaultsViewModel cfViewModel = cfWindowVMs.LastOrDefault();

            MapView mv = MapView.Active;
            
            if (mv == null || cfFeatureLayer == null || cfWindowVMs == null)
            {
                return false;
            }

            await QueuedTask.Run(() =>
            {
                // Get the features that intersect the sketch geometry. 
                SelectionSet features = mv.GetFeatures(geometry);

                // Flash features on the map
                MapView.Active.FlashFeature(features);

                // Check if the ContactsAndFaults layer has any selected features
                if (features.Contains(cfFeatureLayer))
                {
                    // Get the ObjectIDs for the ContactsAndFaults layer
                    IList<long> cfObjectIDs = features[cfFeatureLayer];

                    if (cfObjectIDs.Count > 0)
                    {
                        long cfID = cfObjectIDs.First();

                        using (Table enterpriseTable = cfFeatureLayer.GetTable())
                        {
                            if (enterpriseTable != null)
                            {
                                QueryFilter queryFilter = new QueryFilter
                                {
                                    ObjectIDs = new List<long> { cfID }
                                };

                                using (RowCursor rowCursor = enterpriseTable.Search(queryFilter, false))
                                {
                                    if (rowCursor.MoveNext())
                                    {
                                        using (Row row = rowCursor.Current)
                                        {
                                            //populate a CF from fields
                                            ContactFaultTemplate cf = new ContactFaultTemplate
                                            {
                                                Label = row["label"]?.ToString(),
                                                Type = row["type"]?.ToString(),
                                                Symbol = row["symbol"]?.ToString(),
                                                IdentityConfidence = row["identityconfidence"]?.ToString(),
                                                ExistenceConfidence = row["existenceconfidence"]?.ToString(),
                                                LocationConfidenceMeters = row["locationconfidencemeters"]?.ToString(),
                                                IsConcealed = row["isconcealed"]?.ToString() == "Y",
                                                Notes = row["notes"]?.ToString()
                                            };

                                            // Pass values back to the ViewModel to prepop
                                            cfViewModel.Create.PrepopulateCF(cf);
                                        }
                                    }
                                }
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
