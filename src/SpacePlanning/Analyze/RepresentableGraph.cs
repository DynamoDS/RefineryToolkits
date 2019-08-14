using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Autodesk.RefineryToolkits.Core.Utillites;
using Autodesk.RefineryToolkits.SpacePlanning.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using DSGeom = Autodesk.DesignScript.Geometry;

namespace Autodesk.RefineryToolkits.SpacePlanning.Analyze
{
    /// <summary>
    /// Representation of a Graph.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class RepresentableGraph : BaseGraph
    {
        #region Properties

        private const string graphOutput = "visGraph";
        private const string factorsOutput = "factors";

        internal Dictionary<double, DSCore.Color> colorRange { get; private set; }

        internal List<double> Factors { get; set; }

        #endregion

        #region Constructors

        internal RepresentableGraph() { }

        /// <summary>
        /// Computes the Visibility Graph from a base graph using Lee's algorithm.
        /// </summary>
        /// <param name="baseGraph">Base graph</param>
        /// <param name="reduced">Reduced graph returns edges where its vertices belong to different 
        /// polygons and at least one is not convex/concave to its polygon.</param>
        /// <returns name="visGraph">Visibility graph</returns>
        public static RepresentableGraph ByBaseGraph(BaseGraph baseGraph, bool reduced = true)
        {
            if (baseGraph == null) throw new ArgumentNullException("graph");
            var visGraph = new VisibilityGraph(baseGraph.graph, reduced, true);
            var visibilityGraph = new RepresentableGraph()
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
        public static RepresentableGraph ConnectGraphs(List<RepresentableGraph> visibilityGraphs, List<DSGeom.Line> lines)
        {
            if (visibilityGraphs == null) throw new ArgumentNullException("visibilityGraphs");

            List<VisibilityGraph> visGraphs = visibilityGraphs.Select(vg => (VisibilityGraph)vg.graph).ToList();
            VisibilityGraph mergedGraph = VisibilityGraph.Merge(visGraphs);

            var edges = lines.Select(l => l.ToEdge()).ToList();

            return new RepresentableGraph()
            {
                graph = VisibilityGraph.AddEdges(mergedGraph, edges)
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
        [MultiReturn(new[] { graphOutput, factorsOutput })]
        public static Dictionary<string, object> Connectivity(
            RepresentableGraph visGraph,
            [DefaultArgument("null")] List<DSCore.Color> colours,
            [DefaultArgument("null")] List<double> indices)
        {
            if (visGraph == null) throw new ArgumentNullException("visGraph");

            VisibilityGraph visibilityGraph = visGraph.graph as VisibilityGraph;

            RepresentableGraph graph = new RepresentableGraph()
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
                {graphOutput, graph },
                {factorsOutput, graph.Factors }
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
            package.RequiresPerVertexColoration = true;
            var rangeColors = this.colorRange.Values.ToList();
            for (var i = 0; i < base.graph.edges.Count; i++)
            {
                var e = base.graph.edges[i];
                var factor = this.Factors[i];
                DSCore.Color color;

                if (factor <= this.colorRange.First().Key)
                {
                    color = this.colorRange.First().Value;
                }
                else if (factor >= this.colorRange.Last().Key)
                {
                    color = this.colorRange.Last().Value;
                }
                else
                {
                    int index = this.colorRange.Keys.ToList().BisectIndex(factor);

                    color = DSCore.Color.Lerp(rangeColors[index - 1], rangeColors[index], this.Factors[i]);
                }

                AddColouredEdge(package, e, color);
            }
        }

        #endregion
    }
}
