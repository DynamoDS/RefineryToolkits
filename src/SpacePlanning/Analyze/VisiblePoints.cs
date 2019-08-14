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
        private const string scoreOutputPort = "score";
        private const string geometryOutputPort = "visiblePoints";
        private const string geometryOutputPortViews = "segments";

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


        /// <summary>
        /// Calculates the view to outside from a given point based on a 360 degree ratio.
        /// Returns a number from 0-1 where 0 indicates no points are visible and 1 indicates all points are visible.
        /// </summary>
        /// <param name="boundary">Polygon(s) enclosing all internal Polygons</param>
        /// <param name="internals">List of Polygons representing internal obstructions</param>
        /// <param name="viewSegments">Line segments representing the views to outside</param>
        /// <param name="origin">Origin point to measure from</param>
        /// <returns>precentage of 360 view that is to the outside</returns>
        [MultiReturn(new[] { scoreOutputPort, geometryOutputPortViews })]
        public static Dictionary<string, object> ByLineSegments(
            List<Curve> viewSegments,
            Point origin,
            List<Polygon> boundary,
            [DefaultArgument("[]")] List<Polygon> internals)
        {
            Surface isovist = Isovist.FromPoint(boundary, internals, origin);

            List<Curve> lines = new List<Curve>();
            double outsideViewAngles = 0;
            foreach (Curve segment in viewSegments)
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
            double score = outsideViewAngles / 360;

            return new Dictionary<string, object>()
            {
                {scoreOutputPort, score },
                {geometryOutputPort, lines }
            };
        }

        #region Helpers
        private static DSGeom.Polygon IsovistPolygon(
            List<DSGeom.Polygon> boundary,
            List<DSGeom.Polygon> obstacles,
            DSGeom.Point point)
        {
            var baseGraph = BaseGraph.ByBoundaryAndInternalPolygons(boundary, obstacles);

            if (baseGraph == null)
            {
                throw new ArgumentNullException("graph");
            }

            if (point == null)
            {
                throw new ArgumentNullException("point");
            }

            var origin = GTGeom.Vertex.ByCoordinates(point.X, point.Y, point.Z);

            List<GTGeom.Vertex> vertices = Graphs.VisibilityGraph.VertexVisibility(origin, baseGraph.graph);
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

        #endregion
    }
}
