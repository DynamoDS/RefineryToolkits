/***************************************************************************************
* This code was originally created by Alvaro Ortega Pickmans
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
using GTGeom = Autodesk.RefineryToolkits.Core.Geometry;

namespace Autodesk.RefineryToolkits.SpacePlanning.Analyze
{
    public static class Isovist
    {
        /// <summary>
        /// Returns a surface representing the area visible from the given point.
        /// </summary>
        /// <param name="point">Origin or observation point</param>
        /// <param name="boundary">Polygon(s) enclosing all internal Polygons</param>
        /// <param name="obstructions">List of Polygons representing internal obstructions</param>
        /// <returns name="Isovist">Surface representing the isovist area, meaning the area visible from observation point.</returns>
        [NodeCategory("Actions")]
        public static Surface FromPoint(
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
    }
}
