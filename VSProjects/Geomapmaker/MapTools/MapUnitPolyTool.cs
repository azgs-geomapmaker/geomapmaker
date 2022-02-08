﻿using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.Models;
using Geomapmaker.ViewModels.MapUnitPolys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Geomapmaker.MapTools
{
    internal class MapUnitPolyTool : MapTool
    {
        public MapUnitPolyTool()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Map;
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            if (MapView.Active != null && MapView.Active.Map != null)
            {
                // Clear selection
                QueuedTask.Run(() =>
                {
                    MapView.Active.Map.SetSelection(null);
                });
            }

            return base.OnToolActivateAsync(active);
        }

        private Geometry shape;

        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            IEnumerable<MapUnitPolysViewModel> cfWindowVMs = Application.Current.Windows.OfType<MapUnitPolysViewModel>(); ;

            if (cfWindowVMs.Count() < 1)
            {
                return false;
            }

            // Get the most recent window. GC takes some time to clean up the closed prowindows.
            MapUnitPolysViewModel viewModel = cfWindowVMs.Last();

            var mv = MapView.Active;

            await QueuedTask.Run(() =>
            {
                // Get the features that intersect the sketch geometry.
                var features = mv.GetFeatures(geometry);

                // Only interested in ContactsAndFaults
                var cfFeatures = features.Where(x => x.Key.Name == "ContactsAndFaults");

                if (cfFeatures.Count() > 0)
                {
                    //TODO: I am only pulling the first from the list. Might need to present some sort of selector to the user. 
                    if (cfFeatures.First().Value.Count() > 0)
                    {
                        var cfID = cfFeatures.First().Value.First();

                        FeatureLayer layer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault(l => l.Name == "ContactsAndFaults");

                        if (layer == null)
                        {
                            return;
                        }

                        using (Table cfTable = layer.GetTable())
                        {
                            QueryFilter queryFilter = new QueryFilter
                            {
                                WhereClause = "objectid in (" + cfID + ")"
                            };

                            using (RowCursor rowCursor = cfTable.Search(queryFilter, false))
                            {
                                PolylineBuilder polylineBuilder = new PolylineBuilder(); //TODO: spatial ref?

                                while (rowCursor.MoveNext())
                                {
                                    using (var lineFeature = rowCursor.Current as Feature)
                                    {
                                        // add the coordinate collection of the current geometry into our overall list of collections
                                        var polylineGeometry = lineFeature.GetShape() as Polyline;
                                        polylineBuilder.AddParts(polylineGeometry.Parts);
                                    }
                                }

                                Polyline polyline = GeometryEngine.Instance.SimplifyPolyline(polylineBuilder.ToGeometry(), SimplifyType.Network, true);

                                shape = GeometryEngine.Instance.SimplifyAsFeature(PolygonBuilder.CreatePolygon(polyline), true);
                            }
                        }
                    }
                }
            });

            if (shape == null || shape.IsEmpty)
            {
                return false;
            }

            var polyLayer = MapView.Active.Map.GetLayersAsFlattenedList().First((l) => l.Name == "MapUnitPolys") as FeatureLayer;

            //Define some default attribute values
            Dictionary<string, object> attributes = new Dictionary<string, object>
            {
                ["SHAPE"] = shape, //Geometry
                ["Symbol"] = viewModel.Selected.Symbol,
                ["MapUnit"] = viewModel.Selected.MU,
                ["IdentityConfidence"] = viewModel.IdentityConfidence,
                ["Notes"] = viewModel.Notes,
                ["DataSourceID"] = viewModel.DataSource
            };

            var op = new EditOperation
            {
                Name = string.Format("Create {0}", "MapUnitPolys")
            };
            op.Create(polyLayer, attributes);

            await op.ExecuteAsync();

            if (!op.IsSucceeded)
            {
                MessageBox.Show("Hogan's goat!");
            }

            await QueuedTask.Run(() =>
            {
                MapView.Active.Map.SetSelection(null);
            });

            shape = null;

            //return base.OnSketchCompleteAsync(geometry);
            return true;
        }
    }
}
