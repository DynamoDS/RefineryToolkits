using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using DSGeom = Autodesk.DesignScript.Geometry;
using GTGeom = Autodesk.RefineryToolkits.Core.Geometry;

namespace Autodesk.RefineryToolkits.SpacePlanning.Analyze
{
    public static class Visibility
    {
        private const string percentageVisibleOutputPort = "Percentage visible";
        private const string visibleItemsOutputPort = "Visible items";

        /// <summary>
        /// Calculates the visibility of a set of points, from a given origin point.
        /// Returns the percentage of points that are visible and the visible points themselves.
        /// </summary>
        /// <param name="origin">Origin point to measure visibility from.</param>
        /// <param name="points">The points to measure visibility to.</param>
        /// <param name="boundary">Polygon(s) enclosing all obstacle Polygons</param>
        /// <param name="obstacles">List of Polygons representing internal obstructions</param>
        /// <returns>precentages of the amount of visible points</returns>
        [MultiReturn(new[] { percentageVisibleOutputPort, visibleItemsOutputPort })]
        public static Dictionary<string, object> OfPointsFromOrigin(
            Point origin,
            List<Point> points,
            List<Polygon> boundary,
            [DefaultArgument("[]")] List<Polygon> obstacles = null)
        {
            if (origin is null)
                throw new ArgumentNullException(nameof(origin));
            if (points is null || points.Count == 0)
                throw new ArgumentNullException(nameof(points));
            if (obstacles == null) obstacles = new List<Polygon>();

            Polygon isovist = IsovistPolygon(origin, obstacles, boundary);
            GTGeom.Polygon isovistPolygon = GTGeom.Polygon.ByVertices(isovist.Points.Select(p => GTGeom.Vertex.ByCoordinates(p.X, p.Y, p.Z)).ToList());

            var visiblePoints = new List<Point>();
            double totalPoints = points.Count;
            double visibilityAmount = 0;

            for (var i = 0; i < points.Count; i++)
            {
                var point = points[i];
                GTGeom.Vertex vertex = GTGeom.Vertex.ByCoordinates(point.X, point.Y, point.Z);

                if (isovistPolygon.ContainsVertex(vertex))
                {
                    ++visibilityAmount;
                    visiblePoints.Add(point);
                }
            }
            isovist.Dispose();

            var visibilityPercentageScore = (1 / totalPoints) * visibilityAmount * 100;

            return new Dictionary<string, object>()
            {
                {percentageVisibleOutputPort, visibilityPercentageScore},
                {visibleItemsOutputPort, visiblePoints }
            };
        }


        /// <summary>
        /// Calculates the view to outside from a given point based on a 360 degree ratio.
        /// Returns a number from 0-1 where 0 indicates no points are visible and 1 indicates all points are visible.
        /// </summary>
        /// <param name="boundary">Polygon(s) enclosing all internal Polygons</param>
        /// <param name="obstacles">List of Polygons representing internal obstructions</param>
        /// <param name="targetLines">Line segments representing the views to outside</param>
        /// <param name="origin">Origin point to measure from</param>
        /// <returns>precentage of 360 view that is to the outside</returns>
        [MultiReturn(new[] { percentageVisibleOutputPort, visibleItemsOutputPort })]
        public static Dictionary<string, object> OfLinesFromOrigin(
            Point origin,
            List<Curve> targetLines,
            List<Polygon> boundary,
            [DefaultArgument("[]")] List<Polygon> obstacles)
        {
            Surface isovist = Isovist.FromPoint(boundary, obstacles, origin);

            List<Curve> lines = new List<Curve>();
            double outsideViewAngles = 0;
            foreach (Curve segment in targetLines)
            {
                Geometry[] intersectSegment = isovist.Intersect(segment);
                if (intersectSegment != null)
                {
                    foreach (Curve seg in intersectSegment)
                    {
                        lines.Add(seg);
                        var vec1 = Vector.ByTwoPoints(origin, seg.StartPoint);
                        var vec2 = Vector.ByTwoPoints(origin, seg.EndPoint);
                        outsideViewAngles += vec1.AngleWithVector(vec2);
                        vec1.Dispose();
                        vec2.Dispose();
                    }
                }
                else
                {
                    continue;
                }
            }
            isovist.Dispose();
            double visibilityPercentageScore = outsideViewAngles / 360 * 100;

            return new Dictionary<string, object>()
            {
                {percentageVisibleOutputPort, visibilityPercentageScore },
                {visibleItemsOutputPort, lines }
            };
        }

        #region Helpers
        private static Polygon IsovistPolygon(
            Point originPoint,
            List<Polygon> obstacles,
            List<Polygon> boundary)
        {
            if (obstacles is null)
                throw new ArgumentNullException(nameof(obstacles));
            if (boundary is null)
                throw new ArgumentNullException(nameof(boundary));

            var originVertex = GTGeom.Vertex.ByCoordinates(originPoint.X, originPoint.Y, originPoint.Z);
            var baseGraph = BaseGraph.ByBoundaryAndInternalPolygons(boundary, obstacles);

            List<GTGeom.Vertex> vertices = Graphs.VisibilityGraph.VertexVisibility(originVertex, baseGraph.graph);
            List<DSGeom.Point> points = vertices.Select(v => GTGeom.Points.ToPoint(v)).ToList();

            var polygon = DSGeom.Polygon.ByPoints(points);

            // if polygon is self intersecting, make new polygon
            if (polygon.SelfIntersections().Length > 0)
            {
                points.Add(originPoint);
                polygon = DSGeom.Polygon.ByPoints(points);
            }
            return polygon;
        }

        #endregion
    }
}
