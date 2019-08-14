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
    public static class PathFinding
    {
        private const string graphOutputPort = "path";
        private const string lengthOutputPort = "length";

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
            RepresentableGraph visGraph,
            Point origin,
            Point destination)
        {

            if (visGraph == null) throw new ArgumentNullException("visibility");
            if (origin == null) throw new ArgumentNullException("origin");
            if (destination == null) throw new ArgumentNullException("destination");

            var gOrigin = GTGeom.Vertex.ByCoordinates(origin.X, origin.Y, origin.Z);
            var gDestination = GTGeom.Vertex.ByCoordinates(destination.X, destination.Y, destination.Z);

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
        public static RepresentableGraph CreateVisibilityGraph(
            List<DSGeom.Polygon> boundary,
            List<DSGeom.Polygon> internals)
        {
            var graph = BaseGraph.ByBoundaryAndInternalPolygons(boundary, internals);
            var visGraph = RepresentableGraph.ByBaseGraph(graph);

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
            foreach (GTGeom.Edge edge in path.graph.edges)
            {
                var start = GTGeom.Points.ToPoint(edge.StartVertex);
                var end = GTGeom.Points.ToPoint(edge.EndVertex);
                lines.Add(Line.ByStartPointEndPoint(start, end));
            }
            return lines;
        }
    }
}
