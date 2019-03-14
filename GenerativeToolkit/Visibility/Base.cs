#region namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
using GraphicalDynamo.Graphs;
#endregion

namespace GenerativeToolkit.Visibility
{

    /// <summary>
    /// Representation of a Graph.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class GraphicalBaseGraph
    {

        #region Public Properties

        /// <summary>
        /// Checks if the input is a Visibility or Base graph.
        /// </summary>
        /// <param name="graph">Graph</param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(true)]
        [NodeCategory("Query")]
        public static bool IsVisibilityGraph(BaseGraph graph)
        {
            return graph.GetType() == typeof(VisibilityGraph);
        }

        /// <summary>
        /// Returns the input graph as a list of lines
        /// </summary>
        /// <returns name="lines">List of lines representing the graph.</returns>
        [IsVisibleInDynamoLibrary(true)]
        [NodeCategory("Query")]
        public static List<Line> Lines(Graphical.Graphs.Graph graph)
        {
            List<Line> lines = new List<Line>();
            foreach (Graphical.Geometry.gEdge edge in graph.edges)
            {
                
                var start = Point.ByCoordinates(edge.StartVertex.X, edge.StartVertex.Y, edge.StartVertex.Z);
                var end = Point.ByCoordinates(edge.EndVertex.X, edge.EndVertex.Y, edge.EndVertex.Z);
                lines.Add(Line.ByStartPointEndPoint(start, end));
            }
            return lines;
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Creates a graph by a set of closed polygons
        /// </summary>
        /// <param name="polygons">Polygons</param>
        /// <returns name="baseGraph">Base graph</returns>
        [IsVisibleInDynamoLibrary(true)]
        public static BaseGraph ByPolygons(List<Polygon> polygons)
        {
            return BaseGraph.ByPolygons(polygons);
        }

        /// <summary>
        /// Creates a Graph by a set of boundary and internal polygons.
        /// </summary>
        /// <param name="boundaries">Boundary polygons</param>
        /// <param name="internals">Internal polygons</param>
        /// <returns name="baseGraph">Base graph</returns>
        [IsVisibleInDynamoLibrary(true)]
        public static BaseGraph ByBoundaryAndInternalPolygons(List<Polygon> boundaries, [DefaultArgument("[]")]List<Polygon> internals)
        {
            return BaseGraph.ByBoundaryAndInternalPolygons(boundaries, internals);
        }

        /// <summary>
        /// Creates a new Graph by a set of lines.
        /// </summary>
        /// <param name="lines">Lines</param>
        /// <returns name="baseGraph">Base Graph</returns>
        [IsVisibleInDynamoLibrary(true)]
        public static BaseGraph ByLines(List<Line> lines)
        {
            return BaseGraph.ByLines(lines);
        }

        #endregion
    }
}
