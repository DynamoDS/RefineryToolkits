#region namespaces
using Autodesk.DesignScript.Runtime;
using GenerativeToolkit.Graphs.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using DSPolygon = Autodesk.DesignScript.Geometry.Polygon;
using DSPoint = Autodesk.DesignScript.Geometry.Point;
using GenerativeToolkit.Graphs;
using Dynamo.Graph.Nodes;
using Autodesk.DesignScript.Geometry;
using Autodesk.GenerativeToolkit.Utilities.GraphicalGeometry;
#endregion

namespace Analyse
{
    public static class PathFinding
    {
        /// <summary>
        /// Returns a graph representing the shortest path 
        /// between two points on a given Visibility Graph.
        /// </summary>
        /// <param name="visGraph">Visibility Graph</param>
        /// <param name="origin">Origin point</param>
        /// <param name="destination">Destination point</param>
        /// <returns name="path">Graph representing the shortest path</returns>
        /// <returns name="length">Length of path</returns>
        [MultiReturn(new[] { "path", "length" })]
        public static Dictionary<string, object> ShortestPath(Visibility visibility, DSPoint origin, DSPoint destination)
        {

            if (visibility == null) { throw new ArgumentNullException("visibility"); }
            if (origin == null) { throw new ArgumentNullException("origin"); }
            if (destination == null) { throw new ArgumentNullException("destination"); }

            GeometryVertex gOrigin = GeometryVertex.ByCoordinates(origin.X, origin.Y, origin.Z);
            GeometryVertex gDestination = GeometryVertex.ByCoordinates(destination.X, destination.Y, destination.Z);

            VisibilityGraph visibilityGraph = visibility.graph as VisibilityGraph;

            BaseGraph baseGraph = new BaseGraph()
            {
                graph = VisibilityGraph.ShortestPath(visibilityGraph, gOrigin, gDestination)
            };

            return new Dictionary<string, object>()
            {
                {"path", baseGraph },
                {"length", baseGraph.graph.edges.Select(e => e.Length).Sum() }
            };
        }

        /// <summary>
        /// Returns a VisibilityGraph which is used as input for ShortestPath
        /// </summary>
        /// <param name="boundary"></param>
        /// <param name="internals"></param>
        /// <returns name = "visGraph">VisibilityGraph for use in ShortestPath</returns>
        public static Visibility CreateVisibilityGraph(List<DSPolygon> boundary, List<DSPolygon> internals)
        {
            BaseGraph graph = BaseGraph.ByBoundaryAndInternalPolygons(boundary, internals);
            Visibility visGraph = Visibility.ByBaseGraph(graph);

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
