using Autodesk.DesignScript.Runtime;
using Autodesk.RefineryToolkits.SpacePlanning.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using DSGeom = Autodesk.DesignScript.Geometry;
using GTGeom = Autodesk.RefineryToolkits.Core.Geometry;

namespace Autodesk.RefineryToolkits.SpacePlanning.Analyze
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
            DSGeom.Point origin,
            List<DSGeom.Point> points,
            List<DSGeom.Polygon> boundary,
            [DefaultArgument("[]")] List<DSGeom.Polygon> obstacles)
        {
            DSGeom.Polygon isovist = IsovistPolygon(boundary, obstacles, origin);
            GTGeom.Polygon gPol = GTGeom.Polygon.ByVertices(isovist.Points.Select(p => GTGeom.Vertex.ByCoordinates(p.X, p.Y, p.Z)).ToList());

            List<DSGeom.Point> visPoints = new List<DSGeom.Point>();
            double totalPoints = points.Count;
            double visibilityAmount = 0;

            foreach (DSGeom.Point point in points)
            {
                GTGeom.Vertex vertex = GTGeom.Vertex.ByCoordinates(point.X, point.Y, point.Z);

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

        private static DSGeom.Polygon IsovistPolygon(
            List<DSGeom.Polygon> boundary,
            List<DSGeom.Polygon> obstacles,
            DSGeom.Point point)
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

            GTGeom.Vertex origin = GTGeom.Vertex.ByCoordinates(point.X, point.Y, point.Z);

            List<GTGeom.Vertex> vertices = VisibilityGraph.VertexVisibility(origin, baseGraph.graph);
            List<DSGeom.Point> points = vertices.Select(v => GTGeom.Points.ToPoint(v)).ToList();

            var polygon = DSGeom.Polygon.ByPoints(points);

            // if polygon is self intersecting, make new polygon
            if (polygon.SelfIntersections().Length > 0)
            {
                points.Add(point);
                polygon = DSGeom.Polygon.ByPoints(points);
            }
            return polygon;
        }
    }
}
