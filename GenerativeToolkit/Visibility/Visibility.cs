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
using Graphical.Geometry;
using Graphical.Extensions;
using Graphical.Graphs;
using Dynamo.Graph.Nodes;
using GenerativeToolkit.Geometry;
using Graphical.Core;
using System.Drawing;
using GenerativeToolkit.Visibility;
#endregion

namespace GenerativeToolkit.Visibility
{
    
    /// <summary>
    /// Representation of a Graph.
    /// </summary>
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
        public static VisibilityGraph ByBaseGraph(BaseGraph baseGraph, bool reduced = true)
        {
            if(baseGraph == null) { throw new ArgumentNullException("graph"); }
            var visGraph = new Graphical.Graphs.VisibilityGraph(baseGraph.graph, reduced, true);

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
        public static VisibilityGraph ConnectGraphs(List<VisibilityGraph> visibilityGraphs, List<Line> lines)
        {
            if(visibilityGraphs == null) { throw new ArgumentNullException("visibilityGraphs"); }

            List<Graphical.Graphs.VisibilityGraph> visGraphs = visibilityGraphs.Select(vg => (Graphical.Graphs.VisibilityGraph)vg.graph).ToList();
            Graphical.Graphs.VisibilityGraph mergedGraph = Graphical.Graphs.VisibilityGraph.Merge(visGraphs);

            var edges = lines.Select(l => l.ToEdge()).ToList();

            return new VisibilityGraph()
            {
                graph = Graphical.Graphs.VisibilityGraph.AddEdges(mergedGraph, edges)
            };
        }

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
        public static Dictionary<string, object> ShortestPath(VisibilityGraph visGraph, DSPoint origin, DSPoint destination)
        {
            if (visGraph == null) { throw new ArgumentNullException("visGraph"); }
            if (origin == null) { throw new ArgumentNullException("origin"); }
            if (destination == null) { throw new ArgumentNullException("destination"); }

            gVertex gOrigin = gVertex.ByCoordinates(origin.X, origin.Y, origin.Z);
            gVertex gDestination = gVertex.ByCoordinates(destination.X, destination.Y, destination.Z);

            Graphical.Graphs.VisibilityGraph visibilityGraph = visGraph.graph as Graphical.Graphs.VisibilityGraph;

            BaseGraph baseGraph = new BaseGraph()
            {
                graph = Graphical.Graphs.VisibilityGraph.ShortestPath(visibilityGraph, gOrigin, gDestination)
            };

            return new Dictionary<string, object>()
            {
                {"graph", baseGraph },
                {"length", baseGraph.graph.edges.Select(e => e.Length).Sum() }
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

            Graphical.Graphs.VisibilityGraph visibilityGraph = visGraph.graph as Graphical.Graphs.VisibilityGraph;

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
                else if(factor >= colorRange.Last().Key)
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
