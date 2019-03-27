#region namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSPoint = Autodesk.DesignScript.Geometry.Point;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using System.Globalization;
using Dynamo.Graph.Nodes;
using Autodesk.GenerativeToolkit.Utilities.GraphicalGeometry;
using System.Drawing;
using GenerativeToolkit.Graphs.Geometry;
using graphs = GenerativeToolkit.Graphs;
using GenerativeToolkit.Graphs.Extensions;
#endregion

namespace Autodesk.GenerativeToolkit.Analyse
{
    /// <summary>
    /// Representation of a Graph.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class VisibilityGraph : BaseGraph
    {
        #region Internal Properties
        internal Dictionary<double, DSCore.Color> colorRange { get; private set; }

        internal List<double> Factors { get; set; }

        #endregion

        #region Public Properties


        #endregion

        #region Internal Constructors

        internal VisibilityGraph() { }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Computes the Visibility Graph from a base graph using Lee's algorithm.
        /// </summary>
        /// <param name="baseGraph">Base graph</param>
        /// <param name="reduced">Reduced graph returns edges where its vertices belong to different 
        /// polygons and at least one is not convex/concave to its polygon.</param>
        /// <returns name="visGraph">Visibility graph</returns>
        [IsVisibleInDynamoLibrary(false)]
        public static VisibilityGraph ByBaseGraph(BaseGraph baseGraph, bool reduced = true)
        {
            if (baseGraph == null) { throw new ArgumentNullException("graph"); }
            var visGraph = new graphs.Graphs.VisibilityGraph(baseGraph.graph, reduced, true);
            var visibilityGraph = new VisibilityGraph()
            {
                graph = visGraph
            };

            return visibilityGraph;

        }


        #endregion

        #region Public Methods

        /// <summary>
        /// Merges a set of Visibility Graphs by connecting them through intersecting lines.
        /// In order to work better, lines end points should intersect VG polygon's edges.
        /// </summary>
        /// <param name="visibilityGraphs"></param>
        /// <param name="lines">Connecting lines</param>
        /// <returns name="visGraph">Connected VisibilityGraph</returns>
        [NodeCategory("Actions")]
        [IsVisibleInDynamoLibrary(false)]
        public static VisibilityGraph ConnectGraphs(List<VisibilityGraph> visibilityGraphs, List<Line> lines)
        {
            if (visibilityGraphs == null) { throw new ArgumentNullException("visibilityGraphs"); }

            List<graphs.Graphs.VisibilityGraph> visGraphs = visibilityGraphs.Select(vg => (graphs.Graphs.VisibilityGraph)vg.graph).ToList();
            graphs.Graphs.VisibilityGraph mergedGraph = graphs.Graphs.VisibilityGraph.Merge(visGraphs);

            var edges = lines.Select(l => l.ToEdge()).ToList();

            return new VisibilityGraph()
            {
                graph = graphs.Graphs.VisibilityGraph.AddEdges(mergedGraph, edges)
            };
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
        [NodeCategory("Query")]
        [MultiReturn(new[] { "visGraph", "factors" })]
        public static Dictionary<string, object> Connectivity(
            VisibilityGraph visGraph,
            [DefaultArgument("null")] List<DSCore.Color> colours,
            [DefaultArgument("null")] List<double> indices)
        {
            if (visGraph == null) { throw new ArgumentNullException("visGraph"); }

            graphs.Graphs.VisibilityGraph visibilityGraph = visGraph.graph as graphs.Graphs.VisibilityGraph;

            VisibilityGraph graph = new VisibilityGraph()
            {
                graph = visibilityGraph,
                Factors = visibilityGraph.ConnectivityFactor()
            };

            if (colours != null && indices != null && colours.Count == indices.Count)
            {
                graph.colorRange = new Dictionary<double, DSCore.Color>();
                // Create KeyValuePairs and sort them by index in case unordered.
                var pairs = indices.Zip(colours, (i, c) => new KeyValuePair<double, DSCore.Color>(i, c)).OrderBy(kv => kv.Key);

                // Adding values to colorRange dictionary
                foreach (KeyValuePair<double, DSCore.Color> kv in pairs)
                {
                    graph.colorRange.Add(kv.Key, kv.Value);
                }
            }

            return new Dictionary<string, object>()
            {
                {"visGraph", graph },
                {"factors", graph.Factors }
            };
        }
        #endregion

        #region Override Methods

        /// <summary>
        /// Customizing the render of Graph
        /// </summary>
        /// <param name="package"></param>
        /// <param name="parameters"></param>
        [IsVisibleInDynamoLibrary(false)]
        public void TessellateVisibilityGraph(IRenderPackage package, TessellationParameters parameters)
        {

            //foreach(gVertex v in graph.vertices)
            //{
            //    AddColouredVertex(package, v, vertexDefaultColour);
            //}

            package.RequiresPerVertexColoration = true;
            var rangeColors = colorRange.Values.ToList();
            for (var i = 0; i < base.graph.edges.Count; i++)
            {
                var e = base.graph.edges[i];
                var factor = Factors[i];
                DSCore.Color color;

                if (factor <= colorRange.First().Key)
                {
                    color = colorRange.First().Value;
                }
                else if (factor >= colorRange.Last().Key)
                {
                    color = colorRange.Last().Value;
                }
                else
                {
                    int index = colorRange.Keys.ToList().BisectIndex(factor);

                    color = DSCore.Color.Lerp(rangeColors[index - 1], rangeColors[index], Factors[i]);
                }


                AddColouredEdge(package, e, color);
            }


        }
        #endregion
    }
}
