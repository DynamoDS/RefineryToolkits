using Autodesk.DesignScript.Runtime;
using Autodesk.GenerativeToolkit.Graphs;
using Dynamo.Graph.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using DSGeom = Autodesk.DesignScript.Geometry;
using GTGeom = Autodesk.GenerativeToolkit.Core.Geometry;

namespace Autodesk.GenerativeToolkit.Analyze
{
    public static class Isovist
    {
        /***************************************************************************************
        * Title: Graphical
        * Author: Alvaro Ortega Pickmans
        * Date: 2017
        * Availability: https://github.com/alvpickmans/Graphical
        *
        ***************************************************************************************/

        /// <summary>
        /// Returns a surface representing the Isovist area visible from 
        /// the given point.
        /// </summary>
        /// <param name="boundary">Polygon(s) enclosing all internal Polygons</param>
        /// <param name="internals">List of Polygons representing internal obstructions</param>
        /// <param name="point">Origin point</param>
        /// <returns name="isovist">Surface representing the isovist area</returns>
        [NodeCategory("Actions")]
        public static DSGeom.Surface FromPoint(
            List<DSGeom.Polygon> boundary,
            [DefaultArgument("[]")] List<DSGeom.Polygon> internals,
            DSGeom.Point point)
        {
            BaseGraph baseGraph = BaseGraph.ByBoundaryAndInternalPolygons(boundary, internals);

            if (baseGraph == null) throw new ArgumentNullException("graph");
            if (point == null) throw new ArgumentNullException("point");

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
            DSGeom.Surface surface = DSGeom.Surface.ByPatch(polygon);
            polygon.Dispose();
            points.ForEach(p => p.Dispose());

            return surface;
        }
    }
}
