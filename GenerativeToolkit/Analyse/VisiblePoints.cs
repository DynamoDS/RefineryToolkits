#region namespaces
using Autodesk.DesignScript.Geometry;
using Autodesk.GenerativeToolkit.Utilities.GraphicalGeometry;
using GenerativeToolkit.Graphs.Geometry;
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
        /// <summary>
        /// Calculates the visible points out of a list of sample points from a given origin.
        /// Returns a number from 0-1 where 0 indicates no points are visible and 1 indicates all points are visible.
        /// </summary>
        /// <param name="origin">Origin point to measure from</param>
        /// <param name="points">Sample points</param>
        /// <param name="boundary">Polygon(s) enclosing all obstacle Polygons</param>
        /// <param name="obstacles">List of Polygons representing internal obstructions</param>
        /// <returns>precentages of the amount of visible points</returns>
        public static double FromOrigin(Point origin, List<Point> points, List<Polygon> boundary, [DefaultArgument("[]")] List<Polygon> obstacles)
        {
            Polygon isovist = IsovistPolygon(boundary, obstacles, origin);
            GeometryPolygon gPol = GeometryPolygon.ByVertices(isovist.Points.Select(p => GeometryVertex.ByCoordinates(p.X, p.Y, p.Z)).ToList());

            double totalPoints = points.Count;
            double visibilityAmount = 0;
 
            foreach (Point point in points)
            {
                GeometryVertex vertex = GeometryVertex.ByCoordinates(point.X, point.Y, point.Z);
                
                if (gPol.ContainsVertex(vertex))
                {
                    ++visibilityAmount;
                    //point.Dispose();
                }
            }
            isovist.Dispose();
            return (1/totalPoints) * visibilityAmount;
        }

        private static Polygon IsovistPolygon(List<Polygon> boundary, List<Polygon> obstacles, Point point)
        {
            BaseGraph baseGraph = BaseGraph.ByBoundaryAndInternalPolygons(boundary, obstacles);

            if (baseGraph == null) { throw new ArgumentNullException("graph"); }
            if (point == null) { throw new ArgumentNullException("point"); }

            GeometryVertex origin = GeometryVertex.ByCoordinates(point.X, point.Y, point.Z);

            List<GeometryVertex> vertices = VisibilityGraph.VertexVisibility(origin, baseGraph.graph);
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
