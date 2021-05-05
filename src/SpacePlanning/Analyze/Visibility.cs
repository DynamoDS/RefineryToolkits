/***************************************************************************************
* Portions of this code was originally created by Alvaro Ortega Pickmans
* Title: Graphical
* Author: Alvaro Ortega Pickmans
* Date: 2017
* Availability: https://github.com/alvpickmans/Graphical
*
***************************************************************************************/

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.RefineryToolkits.SpacePlanning.Graphs;
using Dynamo.Graph.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using DSGeom = Autodesk.DesignScript.Geometry;
using GTGeom = Autodesk.RefineryToolkits.Core.Geometry;

namespace Autodesk.RefineryToolkits.SpacePlanning.Analyze
{
    public static class Visibility
    {
        private const string percentageVisibleOutputPort = "percentageVisible";
        private const string visibilityScoresOutputPort = "visibilityPercentages";
        private const string visibleItemsOutputPort = "visibleItems";

        /// <summary>
        /// Calculates the visibility of a set of points, from a given origin point.
        /// Returns the percentage of points that are visible and the visible points themselves.
        /// </summary>
        /// <param name="origin">Origin point to measure visibility from.</param>
        /// <param name="points">The points to measure visibility to.</param>
        /// <param name="boundary">Polygon(s) enclosing all obstacle Polygons</param>
        /// <param name="obstructions">List of Polygons representing internal obstructions</param>
        /// <returns name="percentageVisible">The percentage of target Points that are visible from the origin point.</returns>
        /// <returns name="visibleItems">The specific Points that are visible from the origin point.</returns>
        [MultiReturn(new[] { percentageVisibleOutputPort, visibleItemsOutputPort })]
        public static Dictionary<string, object> OfPointsFromOrigin(
            Point origin,
            List<Point> points,
            List<Polygon> boundary,
            [DefaultArgument("[]")] List<Polygon> obstructions = null)
        {
            if (origin is null)
                throw new ArgumentNullException(nameof(origin));
            if (points is null || points.Count == 0)
                throw new ArgumentNullException(nameof(points));
            if (obstructions == null) obstructions = new List<Polygon>();

            Polygon isovist = IsovistPolygon(origin, obstructions, boundary);
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
        /// Calculates the visibility of target Lines from a given point based on a 360 degree view range.
        /// Returns the percentage of 360 view from origin point that target lines are visible from and the target lines that are visible.
        /// </summary>
        /// <param name="boundary">Polygon(s) enclosing all internal Polygons</param>
        /// <param name="obstructions">List of Polygons representing internal obstructions</param>
        /// <param name="targetLines">Line segments representing the views to outside</param>
        /// <param name="origin">Origin point to measure from</param>
        /// <returns name="percentageVisible">The total percentage of 360 view from origin point that target lines are visible from.</returns>
        /// <returns name="visibilityPercentages">The percentage of 360 view from origin point that each target lines are visible from.</returns>
        /// <returns name="visibleItems">The specific Lines that are visible from the origin point.</returns>
        [MultiReturn(new[] { percentageVisibleOutputPort, visibilityScoresOutputPort, visibleItemsOutputPort })]
        public static Dictionary<string, object> OfLinesFromOrigin(
            Point origin,
            List<Curve> targetLines,
            List<Polygon> boundary,
            [DefaultArgument("[]")] List<Polygon> obstructions)
        {
            double[] visibilityPercentages = new double[targetLines.Count];
            Surface isovist = Visibility.IsovistFromPoint(origin, boundary, obstructions);

            List<Curve> lines = new List<Curve>();
            double outsideViewAngles = 0;
            for (var i = 0; i < targetLines.Count; i++)
            {
                var segment = targetLines[i];
                Geometry[] intersectSegment = isovist.Intersect(segment);
                if (intersectSegment == null) continue;

                double segmentAngle = 0;
                for (var j = 0; j < intersectSegment.Length; j++)
                {
                    var seg = (Curve)intersectSegment[j];
                    lines.Add(seg);
                    var vec1 = Vector.ByTwoPoints(origin, seg.StartPoint);
                    var vec2 = Vector.ByTwoPoints(origin, seg.EndPoint);
                    segmentAngle += vec1.AngleWithVector(vec2);
                    vec1.Dispose();
                    vec2.Dispose();
                }
                outsideViewAngles += segmentAngle;
                visibilityPercentages[i] = segmentAngle / 360 * 100;
                // TODO surface individual scores
            }
            isovist.Dispose();
            double visibilityPercentageScore = outsideViewAngles / 360 * 100;

            return new Dictionary<string, object>()
            {
                {percentageVisibleOutputPort, visibilityPercentageScore },
                {visibilityScoresOutputPort, visibilityPercentages },
                {visibleItemsOutputPort, lines }
            };
        }

        /// <summary>
        /// Returns a surface representing the area visible from the given point.
        /// </summary>
        /// <param name="point">Origin or observation point</param>
        /// <param name="boundary">Polygon(s) enclosing all internal Polygons</param>
        /// <param name="obstructions">List of Polygons representing internal obstructions</param>
        /// <returns name="Isovist">Surface representing the isovist area, meaning the area visible from observation point.</returns>
        [NodeCategory("Actions")]
        public static Surface IsovistFromPoint(
            Point point,
            List<Polygon> boundary,
            [DefaultArgument("[]")] List<Polygon> obstructions)
        {
            var baseGraph = BaseGraph.ByBoundaryAndInternalPolygons(boundary, obstructions);

            if (baseGraph == null) throw new ArgumentNullException("graph");
            if (point == null) throw new ArgumentNullException("point");

            GTGeom.Vertex origin = GTGeom.Vertex.ByCoordinates(point.X, point.Y, point.Z);

            List<GTGeom.Vertex> vertices = VisibilityGraph.VertexVisibility(origin, baseGraph.graph);
            var points = vertices.Select(v => GTGeom.Points.ToPoint(v)).ToList();

            var polygon = Polygon.ByPoints(points);

            // if polygon is self intersecting, make new polygon
            if (polygon.SelfIntersections().Length > 0)
            {
                points.Add(point);
                polygon = Polygon.ByPoints(points);
            }

            var surface = Surface.ByPatch(polygon);
            polygon.Dispose();
            points.ForEach(p => p.Dispose());

            return surface;
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
            points.ForEach(x => x.Dispose());

            return polygon;
        }

        #endregion
    }
}
