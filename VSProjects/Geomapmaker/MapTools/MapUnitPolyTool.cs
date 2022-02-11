using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.ViewModels.MapUnitPolys;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Geomapmaker.MapTools
{
    internal class MapUnitPolyTool : MapTool
    {
        public MapUnitPolyTool()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Map;

            // Reflection
            Assembly asm = Assembly.GetExecutingAssembly();

            // Path to custom cursor
            string uri = Path.GetDirectoryName(Uri.UnescapeDataString(new Uri(asm.CodeBase).LocalPath)) + "\\Cursors\\ContactsFaults.cur";

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
            FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "ContactsAndFaults");

            IEnumerable<MapUnitPolysViewModel> mapUnitPolyVMs = FrameworkApplication.Current.Windows.OfType<MapUnitPolysViewModel>(); ;

            // Get the most recent window. GC takes some time to clean up the closed prowindows.
            MapUnitPolysViewModel mapUnitPolyVM = mapUnitPolyVMs.Last();

            await QueuedTask.Run(() =>
            {
                // Get features that intersect the point

                Dictionary<BasicFeatureLayer, List<long>> selection = MapView.Active.GetFeatures(geometry);

                // Filter anything not CF
                FeatureLayer cfLayer = selection.Where(f => f.Key.Name == "ContactsAndFaults").FirstOrDefault().Key as FeatureLayer;

                // Select the oids
                List<long> oidsCF = selection[cfLayer];

                if (oidsCF.Count > 0)
                {
                    mapUnitPolyVM.Set_CF_Oids(oidsCF);
                }
            });

            return true;
        }

        //private List<long> lineOids = new List<long>();

        //protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        //{
        //    FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "ContactsAndFaults");

        //    await QueuedTask.Run(() =>
        //    {
        //        // Get features that intersect the point
        //        var selection = MapView.Active.GetFeatures(geometry);

        //        // Filter anything not CF
        //        var cfLayer = selection.Where(f => f.Key.Name == "ContactsAndFaults").FirstOrDefault().Key as FeatureLayer;

        //        // Select the oids
        //        var oidsCF = selection[cfLayer];

        //        foreach (long oid in oidsCF)
        //        {
        //            if (lineOids.Contains(oid))
        //            {
        //                lineOids.Remove(oid);
        //            }
        //            else
        //            {
        //                lineOids.Add(oid);
        //            }
        //        }

        //        if (lineOids.Count == 0)
        //        {
        //            cfLayer.ClearSelection();
        //        }
        //        else
        //        {
        //            QueryFilter queryFilter = new QueryFilter
        //            {
        //                ObjectIDs = lineOids
        //            };

        //            var foo = cfLayer.Select(queryFilter);

        //            var op = new EditOperation
        //            {
        //                Name = string.Format("Create {0}", "MapUnitPolys")
        //            };

        //            var polyLayer = MapView.Active.Map.GetLayersAsFlattenedList().First((l) => l.Name == "MapUnitPolys") as FeatureLayer;

        //            var tmpTemplate = polyLayer.GetTemplates().FirstOrDefault();

        //            op.ConstructPolygons(tmpTemplate, cfLayer, lineOids, null, false);

        //            try
        //            {
        //                op.Execute();

        //                if (op.IsSucceeded)
        //                {
        //                    lineOids = new List<long>();
        //                    cfLayer.ClearSelection();
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                // Edit op failed
        //            }

        //        }

        //    });

        //    return true;
        //}







        //private Geometry shape;

        //protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        //{
        //    IEnumerable<MapUnitPolysViewModel> cfWindowVMs = Application.Current.Windows.OfType<MapUnitPolysViewModel>(); ;

        //    if (cfWindowVMs.Count() < 1)
        //    {
        //        return false;
        //    }

        //    // Get the most recent window. GC takes some time to clean up the closed prowindows.
        //    MapUnitPolysViewModel viewModel = cfWindowVMs.Last();

        //    var mv = MapView.Active;

        //    await QueuedTask.Run(() =>
        //    {
        //        // Get the features that intersect the sketch geometry.
        //        var features = mv.GetFeatures(geometry);

        //        // Only interested in ContactsAndFaults
        //        var cfFeatures = features.Where(x => x.Key.Name == "ContactsAndFaults");

        //        if (cfFeatures.Count() > 0)
        //        {
        //            //TODO: I am only pulling the first from the list. Might need to present some sort of selector to the user. 
        //            if (cfFeatures.First().Value.Count() > 0)
        //            {
        //                var cfID = cfFeatures.First().Value.First();

        //                FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "ContactsAndFaults");

        //                if (layer == null)
        //                {
        //                    return;
        //                }

        //                using (Table cfTable = layer.GetTable())
        //                {
        //                    QueryFilter queryFilter = new QueryFilter
        //                    {
        //                        WhereClause = "objectid in (" + cfID + ")"
        //                    };

        //                    using (RowCursor rowCursor = cfTable.Search(queryFilter, false))
        //                    {
        //                        PolylineBuilder polylineBuilder = new PolylineBuilder(); //TODO: spatial ref?

        //                        while (rowCursor.MoveNext())
        //                        {
        //                            using (var lineFeature = rowCursor.Current as Feature)
        //                            {
        //                                // add the coordinate collection of the current geometry into our overall list of collections
        //                                var polylineGeometry = lineFeature.GetShape() as Polyline;
        //                                polylineBuilder.AddParts(polylineGeometry.Parts);
        //                            }
        //                        }

        //                        Polyline polyline = GeometryEngine.Instance.SimplifyPolyline(polylineBuilder.ToGeometry(), SimplifyType.Network, true);

        //                        shape = GeometryEngine.Instance.SimplifyAsFeature(PolygonBuilder.CreatePolygon(polyline), true);
        //                    }
        //                }
        //            }
        //        }
        //    });

        //    if (shape == null || shape.IsEmpty)
        //    {
        //        return false;
        //    }

        //    var polyLayer = MapView.Active.Map.GetLayersAsFlattenedList().First((l) => l.Name == "MapUnitPolys") as FeatureLayer;

        //    //Define some default attribute values
        //    Dictionary<string, object> attributes = new Dictionary<string, object>
        //    {
        //        ["SHAPE"] = shape, //Geometry
        //        ["Symbol"] = viewModel.Selected.Symbol,
        //        ["MapUnit"] = viewModel.Selected.MU,
        //        ["IdentityConfidence"] = viewModel.IdentityConfidence,
        //        ["Notes"] = viewModel.Notes,
        //        ["DataSourceID"] = viewModel.DataSource
        //    };

        //    var op = new EditOperation
        //    {
        //        Name = string.Format("Create {0}", "MapUnitPolys")
        //    };
        //    op.Create(polyLayer, attributes);

        //    await op.ExecuteAsync();

        //    if (!op.IsSucceeded)
        //    {
        //        MessageBox.Show("Hogan's goat!");
        //    }

        //    await QueuedTask.Run(() =>
        //    {
        //        MapView.Active.Map.SetSelection(null);
        //    });

        //    shape = null;

        //    //return base.OnSketchCompleteAsync(geometry);
        //    return true;
        //}
    }
}
