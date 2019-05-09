using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.GenerativeToolkit.Utilities.GraphicalGeometry;
using GenerativeToolkit.Graphs;
using GenerativeToolkit.Graphs.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Autodesk.GenerativeToolkit.Analyze
{
    public static class VisiblePoints
    {
        private const string scoreOutputPort = "score";
        private const string geometryOutputPort = "visiblePoints";

        /// <summary>
        /// Calculates the visible points out of a list of sample points from a given origin.
        /// Returns a number from 0-1 where 0 indicates no points are visible and 1 indicates all points are visible.
        /// </summary>
        /// <param name="origin">Origin point to measure from</param>
        /// <param name="points">Sample points</param>
        /// <param name="boundary">Polygon(s) enclosing all obstacle Polygons</param>
        /// <param name="obstacles">List of Polygons representing internal obstructions</param>
        /// <returns>precentages of the amount of visible points</returns>
        [MultiReturn(new[] { scoreOutputPort, geometryOutputPort })]
        public static Dictionary<string, object> FromOrigin(
            Point origin,
            List<Point> points,
            List<Polygon> boundary,
            [DefaultArgument("[]")] List<Polygon> obstacles)
        {
            Polygon isovist = IsovistPolygon(boundary, obstacles, origin);
            GeometryPolygon gPol = GeometryPolygon.ByVertices(isovist.Points.Select(p => GeometryVertex.ByCoordinates(p.X, p.Y, p.Z)).ToList());

            List<Point> visPoints = new List<Point>();
            double totalPoints = points.Count;
            double visibilityAmount = 0;

            foreach (Point point in points)
            {
                GeometryVertex vertex = GeometryVertex.ByCoordinates(point.X, point.Y, point.Z);

                if (gPol.ContainsVertex(vertex))
                {
                    ++visibilityAmount;
                    visPoints.Add(point);
                }
            }
            isovist.Dispose();

            return new Dictionary<string, object>()
            {
                {scoreOutputPort, (1/totalPoints) * visibilityAmount},
                {geometryOutputPort, visPoints }
            };
        }

        private static Polygon IsovistPolygon(
            List<Polygon> boundary,
            List<Polygon> obstacles,
            Point point)
        {
            BaseGraph baseGraph = BaseGraph.ByBoundaryAndInternalPolygons(boundary, obstacles);

            if (baseGraph == null)
            {
                throw new ArgumentNullException("graph");
            }

            if (point == null)
            {
                throw new ArgumentNullException("point");
            }

            GeometryVertex origin = GeometryVertex.ByCoordinates(point.X, point.Y, point.Z);

            List<GeometryVertex> vertices = VisibilityGraph.VertexVisibility(origin, baseGraph.graph);
            List<Point> points = vertices.Select(v => Points.ToPoint(v)).ToList();

            var polygon = Polygon.ByPoints(points);

            // if polygon is self intersecting, make new polygon
            if (polygon.SelfIntersections().Length > 0)
            {
                points.Add(point);
                polygon = Polygon.ByPoints(points);
            }
            return polygon;
        }
    }
}
