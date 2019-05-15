#region namespaces
using DSGeom = Autodesk.DesignScript.Geometry;
using Autodesk.GenerativeToolkit.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using GenerativeToolkit.Graphs;
using Autodesk.DesignScript.Runtime;
#endregion

namespace Autodesk.GenerativeToolkit.Analyse
{
    public static class VisiblePoints
    {
        private const string output1 = "score";
        private const string output2 = "visiblePoints";

        /// <summary>
        /// Calculates the visible points out of a list of sample points from a given origin.
        /// Returns a number from 0-1 where 0 indicates no points are visible and 1 indicates all points are visible.
        /// </summary>
        /// <param name="origin">Origin point to measure from</param>
        /// <param name="points">Sample points</param>
        /// <param name="boundary">Polygon(s) enclosing all obstacle Polygons</param>
        /// <param name="obstacles">List of Polygons representing internal obstructions</param>
        /// <returns>precentages of the amount of visible points</returns>
        [MultiReturn(new[] { output1, output2 })]
        public static Dictionary<string, object> FromOrigin(DSGeom.Point origin, List<DSGeom.Point> points, List<DSGeom.Polygon> boundary, [DefaultArgument("[]")] List<DSGeom.Polygon> obstacles)
        {
            DSGeom.Polygon isovist = IsovistPolygon(boundary, obstacles, origin);
            Polygon gPol = Polygon.ByVertices(isovist.Points.Select(p => Vertex.ByCoordinates(p.X, p.Y, p.Z)).ToList());

            List<Point> visPoints = new List<Point>();
            double totalPoints = points.Count;
            double visibilityAmount = 0;
 
            foreach (Point point in points)
            {
                Vertex vertex = Vertex.ByCoordinates(point.X, point.Y, point.Z);
                
                if (gPol.ContainsVertex(vertex))
                {
                    ++visibilityAmount;
                    visPoints.Add(point);
                }
            }
            isovist.Dispose();

            return new Dictionary<string, object>()
            {
                {output1, (1/totalPoints) * visibilityAmount},
                {output2, visPoints }
            };
        }

        private static Polygon IsovistPolygon(List<Polygon> boundary, List<Polygon> obstacles, Point point)
        {
            BaseGraph baseGraph = BaseGraph.ByBoundaryAndInternalPolygons(boundary, obstacles);

            if (baseGraph == null) { throw new ArgumentNullException("graph"); }
            if (point == null) { throw new ArgumentNullException("point"); }

            Vertex origin = Vertex.ByCoordinates(point.X, point.Y, point.Z);

            List<Vertex> vertices = VisibilityGraph.VertexVisibility(origin, baseGraph.graph);
            List<Point> points = vertices.Select(v => Points.ToPoint(v)).ToList();
            // TODO: Implement better way of checking if polygon is self intersectingç

            Polygon polygon = Polygon.ByPoints(points);

            if (polygon.SelfIntersections().Length > 0)
            {
                points.Add(point);
                polygon = Polygon.ByPoints(points);
            }
            return polygon;
        }
    }
}
