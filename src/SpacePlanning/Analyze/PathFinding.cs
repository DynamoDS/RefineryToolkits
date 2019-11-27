using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.RefineryToolkits.SpacePlanning.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using DSGeom = Autodesk.DesignScript.Geometry;
using GTGeom = Autodesk.RefineryToolkits.Core.Geometry;

namespace Autodesk.RefineryToolkits.SpacePlanning.Analyze
{
    public static class PathFinding
    {
        private const string pathOutputPort = "Path";
        private const string lengthOutputPort = "Length";

        /// <summary>
        /// Returns the shortest path between two points, in 2D only. 
        /// Works by computing a visibility graph and then finding shortest path on graph with Dijkstra's algorithm.
        /// </summary>
        /// <param name="startPoint">Start point</param>
        /// <param name="endPoint">Destination point</param>
        /// <param name="boundary">Polygon(s) enclosing all obstacle Polygons</param>
        /// <param name="obstructions">List of Polygons representing internal obstructions</param>
        /// <returns name="path">Set of lines representing the shortest path</returns>
        /// <returns name="length">Length of path.</returns>
        [MultiReturn(new[] { pathOutputPort, lengthOutputPort })]
        public static Dictionary<string, object> ShortestPath(
            Point startPoint,
            Point endPoint,
            List<Polygon> boundary,
            List<Polygon> obstructions)
        {
            if (startPoint is null)
                throw new ArgumentNullException(nameof(startPoint));
            if (endPoint is null)
                throw new ArgumentNullException(nameof(endPoint));
            if (boundary is null)
                throw new ArgumentNullException(nameof(boundary));
            if (obstructions is null)
                throw new ArgumentNullException(nameof(obstructions));

            var gOrigin = GTGeom.Vertex.ByCoordinates(startPoint.X, startPoint.Y, startPoint.Z);
            var gDestination = GTGeom.Vertex.ByCoordinates(endPoint.X, endPoint.Y, endPoint.Z);

            // compute visibility graph
            var visGraph = CreateVisibilityGraph(boundary, obstructions);
            if (visGraph is null) throw new InvalidOperationException("Failed to create visibility graph.");
            var visibilityGraph = visGraph.graph as VisibilityGraph;

            var graph = VisibilityGraph.ShortestPath(visibilityGraph, gOrigin, gDestination);

            return new Dictionary<string, object>()
            {
                {pathOutputPort, LinesFromGraph(graph) },
                {lengthOutputPort, graph.edges.Select(e => e.Length).Sum() }
            };
        }

        /// <summary>
        /// Returns a VisibilityGraph which is used as input for ShortestPath
        /// </summary>
        /// <returns name = "visGraph">VisibilityGraph for use in ShortestPath</returns>
        [IsVisibleInDynamoLibrary(false)]
        public static RepresentableGraph CreateVisibilityGraph(
            List<DSGeom.Polygon> boundary,
            List<DSGeom.Polygon> obstructions)
        {
            var graph = BaseGraph.ByBoundaryAndInternalPolygons(boundary, obstructions);
            var visGraph = RepresentableGraph.ByBaseGraph(graph);

            return visGraph;
        }

        /// <summary>
        /// Returns the input graph as a list of lines
        /// </summary>
        /// <returns name="lines">List of lines representing the graph.</returns>
        [IsVisibleInDynamoLibrary(false)]
        public static List<Line> LinesFromGraph(Graph graph)
        {
            var lines = new List<Line>();
            for (var i = 0; i < graph.edges.Count; i++)
            {
                var edge = graph.edges[i];
                var start = GTGeom.Points.ToPoint(edge.StartVertex);
                var end = GTGeom.Points.ToPoint(edge.EndVertex);
                lines.Add(Line.ByStartPointEndPoint(start, end));
            }
            return lines;
        }
    }
}
