using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.GenerativeToolkit.Utilities.GraphicalGeometry;
using Dynamo.Graph.Nodes;
using GenerativeToolkit.Graphs;
using GenerativeToolkit.Graphs.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using DSPoint = Autodesk.DesignScript.Geometry.Point;
using DSPolygon = Autodesk.DesignScript.Geometry.Polygon;

namespace Autodesk.GenerativeToolkit.Analyze
{
    public static class PathFinding
    {
        private const string graphOutputPort = "path";
        private const string lengthOutputPort = "length";


        /***************************************************************************************
        * Title: Graphical
        * Author: Alvaro Ortega Pickmans
        * Date: 2017
        * Availability: https://github.com/alvpickmans/Graphical
        *
        ***************************************************************************************/

        /// <summary>
        /// Returns a graph representing the shortest path 
        /// between two points on a given Visibility Graph.
        /// </summary>
        /// <param name="visGraph">Visibility Graph</param>
        /// <param name="origin">Origin point</param>
        /// <param name="destination">Destination point</param>
        /// <returns name="path">Graph representing the shortest path</returns>
        /// <returns name="length">Length of path</returns>
        [MultiReturn(new[] { graphOutputPort, lengthOutputPort })]
        public static Dictionary<string, object> ShortestPath(
            Visibility visGraph,
            DSPoint origin,
            DSPoint destination)
        {

            if (visGraph == null) throw new ArgumentNullException("visibility");
            if (origin == null) throw new ArgumentNullException("origin");
            if (destination == null) throw new ArgumentNullException("destination");

            var gOrigin = GeometryVertex.ByCoordinates(origin.X, origin.Y, origin.Z);
            var gDestination = GeometryVertex.ByCoordinates(destination.X, destination.Y, destination.Z);

            var visibilityGraph = visGraph.graph as VisibilityGraph;

            var baseGraph = new BaseGraph()
            {
                graph = VisibilityGraph.ShortestPath(visibilityGraph, gOrigin, gDestination)
            };

            return new Dictionary<string, object>()
            {
                {graphOutputPort, baseGraph },
                {lengthOutputPort, baseGraph.graph.edges.Select(e => e.Length).Sum() }
            };
        }

        /// <summary>
        /// Returns a VisibilityGraph which is used as input for ShortestPath
        /// </summary>
        /// <param name="boundary"></param>
        /// <param name="internals"></param>
        /// <returns name = "visGraph">VisibilityGraph for use in ShortestPath</returns>
        public static Visibility CreateVisibilityGraph(
            List<DSPolygon> boundary,
            List<DSPolygon> internals)
        {
            var graph = BaseGraph.ByBoundaryAndInternalPolygons(boundary, internals);
            var visGraph = Visibility.ByBaseGraph(graph);

            return visGraph;
        }

        /// <summary>
        /// Returns the input graph as a list of lines
        /// </summary>
        /// <returns name="lines">List of lines representing the graph.</returns>
        [NodeCategory("Query")]
        public static List<Line> Lines(BaseGraph path)
        {
            List<Line> lines = new List<Line>();
            foreach (GeometryEdge edge in path.graph.edges)
            {
                var start = Points.ToPoint(edge.StartVertex);
                var end = Points.ToPoint(edge.EndVertex);
                lines.Add(Line.ByStartPointEndPoint(start, end));
            }
            return lines;
        }
    }
}
