#region namespaces
using Autodesk.DesignScript.Geometry;
using Autodesk.GenerativeToolkit.Utilities.GraphicalGeometry;
using Dynamo.Graph.Nodes;
using GenerativeToolkit.Graphs.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using DSPoint = Autodesk.DesignScript.Geometry.Point;
using GenerativeToolkit.Graphs;
#endregion

namespace Analyse
{
    public static class Isovist
    {
        #region Public Methods

        /// <summary>
        /// Returns a surface representing the Isovist area visible from 
        /// the given point.
        /// </summary>
        /// <param name="baseGraph">Base Graph</param>
        /// <param name="point">Origin point</param>
        /// <returns name="isovist">Surface representing the isovist area</returns>
        [NodeCategory("Actions")]
        public static Surface FromPoint(List<Polygon> boundary, List<Polygon> internals, DSPoint point)
        {
            BaseGraph baseGraph = BaseGraph.ByBoundaryAndInternalPolygons(boundary,internals);

            if (baseGraph == null) { throw new ArgumentNullException("graph"); }
            if (point == null) { throw new ArgumentNullException("point"); }

            GeometryVertex origin = GeometryVertex.ByCoordinates(point.X, point.Y, point.Z);

            List<GeometryVertex> vertices = VisibilityGraph.VertexVisibility(origin, baseGraph.graph);
            List<DSPoint> points = vertices.Select(v => Points.ToPoint(v)).ToList();

            Polygon polygon = Polygon.ByPoints(points);

            if (polygon.SelfIntersections().Length > 0)
            {
                points.Add(point);
                polygon = Polygon.ByPoints(points);

            }

            return Surface.ByPatch(polygon);
        }

        #endregion
    }
}
