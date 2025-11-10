using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Geomapmaker.ViewModels.OrientationPoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geomapmaker.MapTools {
    internal class AzimuthMeasureTool : MapTool {
        public AzimuthMeasureTool() {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Line;
            SketchOutputMode = SketchOutputMode.Map;
        }

        /*
        //To snap first line to north-south direction
        protected override async Task<bool> OnSketchModifiedAsync() {
            // Get the current sketch geometry
            var currentSketch = await MapView.Active.GetCurrentSketchAsync();

            // Force the first line segment to be vertical (due North)
            if (currentSketch is Polyline polyline) {
                var points = polyline.Points.ToList();

                // If we have exactly 2 points (first segment being drawn)
                if (points.Count == 2 && points[0].X != points[1].X) {
                    await QueuedTask.Run(() => {
                        MapPoint startPoint = points[0];
                        MapPoint endPoint = points[1];

                        // Create a new end point with the same X as start point (forces vertical line)
                        MapPoint constrainedEndPoint = MapPointBuilderEx.CreateMapPoint(
                            startPoint.X,  // Same X coordinate (forces vertical)
                            endPoint.Y,    // Keep the Y coordinate from mouse position
                            startPoint.SpatialReference);

                        // Create a new polyline with the constrained points
                        List<MapPoint> constrainedPoints = new List<MapPoint> { startPoint, constrainedEndPoint };
                        Polyline constrainedLine = PolylineBuilderEx.CreatePolyline(constrainedPoints, startPoint.SpatialReference);

                        // Update the sketch - use SetCurrentSketchAsync
                        MapView.Active.SetCurrentSketchAsync(constrainedLine);
                    });
                }
            }

            // Call base to ensure proper sketch handling for all cases
            return await base.OnSketchModifiedAsync();
        }
        */

        //To assume north-south line (accept only one line)
        protected override async Task<bool> OnSketchModifiedAsync() {
            // Get the current sketch geometry
            var currentSketch = await MapView.Active.GetCurrentSketchAsync();

            // If we have a line with 2 points, complete the sketch automatically
            if (currentSketch is Polyline polyline) {
                var points = polyline.Points.ToList();

                // If we have exactly 2 points, finish the sketch
                if (points.Count >= 2) {
                    await FinishSketchAsync();
                    return true;
                }
            }

            return await base.OnSketchModifiedAsync();
        }


        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            if (geometry is Polyline line)
            {
                double azimuth = await QueuedTask.Run(() =>
                {
                    var points = line.Points;
                    if (points.Count >= 2)
                    {
                        MapPoint startPoint = points[0];
                        MapPoint endPoint = points[points.Count - 1];

                        // Calculate azimuth using simple trigonometry
                        double deltaX = endPoint.X - startPoint.X;
                        double deltaY = endPoint.Y - startPoint.Y;
                        
                        // Calculate angle in radians, then convert to degrees
                        double angleRadians = Math.Atan2(deltaX, deltaY);
                        double azimuth = angleRadians * (180.0 / Math.PI);

                        // Normalize to 0-360 range
                        if (azimuth < 0)
                            azimuth += 360.0;

                        return azimuth;
                    }
                    return 0;
                });

                //MessageBox.Show($"Azimuth: {azimuth:F2}°", "Azimuth Measurement");

                IEnumerable<OrientationPointsViewModel> opWindowVMs = System.Windows.Application.Current.Windows.OfType<OrientationPointsViewModel>(); ;

                // Get the most recent window. GC takes some time to clean up the closed prowindows.
                OrientationPointsViewModel opViewModel = opWindowVMs.LastOrDefault();

                opViewModel.Create.PopulateAzimuth(azimuth);

            }

            return true;
        }
    }
}