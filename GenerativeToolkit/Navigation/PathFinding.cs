using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Geometry;

namespace GenerativeToolkit.Navigation
{
    [IsVisibleInDynamoLibrary(false)]
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
        [IsVisibleInDynamoLibrary(true)]
        [MultiReturn(new[] { "graph", "length" })]
        public static Dictionary<string, object> ShortestPath(GraphicalDynamo.Graphs.VisibilityGraph visGraph, Point origin, Point destination)
        {
            return GraphicalDynamo.Graphs.VisibilityGraph.ShortestPath(visGraph, origin, destination);
        }
    }
}
