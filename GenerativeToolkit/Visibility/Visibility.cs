#region namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
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
    public class GraphicalVisibilityGraph
    {

        #region Public Methods
        /// <summary>
        /// Computes the Visibility Graph from a base graph using Lee's algorithm.
        /// </summary>
        /// <param name="baseGraph">Base graph</param>
        /// <param name="reduced">Reduced graph returns edges where its vertices belong to different 
        /// polygons and at least one is not convex/concave to its polygon.</param>
        /// <returns name="visGraph">Visibility graph</returns>
        [IsVisibleInDynamoLibrary(true)]
        public static VisibilityGraph ByBaseGraph(BaseGraph baseGraph, bool reduced = true)
        {
            return VisibilityGraph.ByBaseGraph(baseGraph, reduced);
        }

        /// <summary>
        /// Merges a set of Visibility Graphs by connecting them through intersecting lines.
        /// In order to work better, lines end points should intersect VG polygon's edges.
        /// </summary>
        /// <param name="visibilityGraphs"></param>
        /// <param name="lines">Connecting lines</param>
        /// <returns name="visGraph">Connected VisibilityGraph</returns>
        [IsVisibleInDynamoLibrary(true)]
        [NodeCategory("Actions")]
        public static VisibilityGraph ConnectGraphs(List<VisibilityGraph> visibilityGraphs, List<Line> lines)
        {
            return VisibilityGraph.ConnectGraphs(visibilityGraphs, lines);
        }

        /// <summary>
        /// Connectivity factors represent the number of connections an edge has 
        /// on a range from 0 to 1.
        /// </summary>
        /// <param name="visGraph">Visibility Graph</param>
        /// <param name="colours">List of colours to include on the displayed range</param>
        /// <param name="indices">List of values between 0.0 and 1.0 to define the limits of colours</param>
        /// <returns name="visGraph">Visibility Graph</returns>
        /// <returns name="factors">Connectivity factors by edge on graph</returns>
        [IsVisibleInDynamoLibrary(true)]
        [NodeCategory("Query")]
        [MultiReturn(new[] { "visGraph", "factors" })]
        public static Dictionary<string, object> Connectivity(
            VisibilityGraph visGraph,
            [DefaultArgument("null")] List<DSCore.Color> colours,
            [DefaultArgument("null")] List<double> indices)
        {
            return VisibilityGraph.Connectivity(visGraph, colours, indices);
        }
        #endregion
    }
}
