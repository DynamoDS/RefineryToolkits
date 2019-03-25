#region namespaces
using Autodesk.DesignScript.Runtime;
using GenerativeToolkit.Graphs.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using DSPolygon = Autodesk.DesignScript.Geometry.Polygon;
using DSPoint = Autodesk.DesignScript.Geometry.Point;
using graphs = GenerativeToolkit.Graphs;
#endregion

namespace Autodesk.GenerativeToolkit.Analyse
{
    public class PathFinding
    {
        /// <summary>
        /// Returns a graph representing the shortest path 
        /// between two points on a given Visibility Graph.
        /// </summary>
        /// <param name="visGraph">Visibility Graph</param>
        /// <param name="origin">Origin point</param>
        /// <param name="destination">Destination point</param>
        /// <returns name="graph">Graph representing the shortest path</returns>
        /// <returns name="length">Length of path</returns>
        [MultiReturn(new[] { "graph", "length" })]
        [IsVisibleInDynamoLibrary(true)]
        public static Dictionary<string, object> ShortestPath(List<DSPolygon> boundary, List<DSPolygon> internals, DSPoint origin, DSPoint destination)
        {
            BaseGraph graph = BaseGraph.ByBoundaryAndInternalPolygons(boundary, internals);
            VisibilityGraph visGraph = VisibilityGraph.ByBaseGraph(graph);

            if (visGraph == null) { throw new ArgumentNullException("visGraph"); }
            if (origin == null) { throw new ArgumentNullException("origin"); }
            if (destination == null) { throw new ArgumentNullException("destination"); }

            gVertex gOrigin = gVertex.ByCoordinates(origin.X, origin.Y, origin.Z);
            gVertex gDestination = gVertex.ByCoordinates(destination.X, destination.Y, destination.Z);

            graphs.Graphs.VisibilityGraph visibilityGraph = visGraph.graph as graphs.Graphs.VisibilityGraph;

            BaseGraph baseGraph = new BaseGraph()
            {
                graph = graphs.Graphs.VisibilityGraph.ShortestPath(visibilityGraph, gOrigin, gDestination)
            };

            return new Dictionary<string, object>()
            {
                {"graph", baseGraph },
                {"length", baseGraph.graph.edges.Select(e => e.Length).Sum() }
            };
        }
    }  
}
