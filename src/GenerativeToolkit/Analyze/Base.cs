#region namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using DSPoint = Autodesk.DesignScript.Geometry.Point;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using GenerativeToolkit.Graphs.Geometry;
using Dynamo.Graph.Nodes;
using Autodesk.GenerativeToolkit.Utilities.GraphicalGeometry;
using GenerativeToolkit.Graphs;
#endregion

namespace Autodesk.GenerativeToolkit.Analyze
{
    /// <summary>
    /// Representation of a Graph.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class BaseGraph : IGraphicItem
    {
        #region Internal Properties
        internal Graph graph { get; set; }
        internal DSCore.Color edgeDefaultColour = DSCore.Color.ByARGB(255, 150, 200, 255);
        internal DSCore.Color vertexDefaultColour = DSCore.Color.ByARGB(255, 75, 125, 180);

        #endregion

        #region Public Properties

        /// <summary>
        /// Checks if the input is a Visibility or Base graph.
        /// </summary>
        /// <param name="graph">Graph</param>
        /// <returns>Visibility Graph</returns>
        [IsVisibleInDynamoLibrary(false)]
        [NodeCategory("Query")]
        public static bool IsVisibilityGraph(BaseGraph graph)
        {
            return graph.GetType() == typeof(Visibility);
        }

        #endregion

        #region Internal Constructors
        internal BaseGraph() { }

        internal BaseGraph(List<GeometryPolygon> gPolygons)
        {
            graph = new Graph(gPolygons);
        }
        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates a Graph by a set of boundary and internal polygons.
        /// </summary>
        /// <param name="boundaries">Boundary polygons</param>
        /// <param name="internals">Internal polygons</param>
        /// <returns name="baseGraph">Base graph</returns>
        [IsVisibleInDynamoLibrary(false)]
        public static BaseGraph ByBoundaryAndInternalPolygons(List<Polygon> boundaries, 
            [DefaultArgument("[]")]List<Polygon> internals)
        {
            if (boundaries == null) throw new NullReferenceException("boundaryPolygons");
            if (internals == null) throw new NullReferenceException("internalPolygons");
            List<GeometryPolygon> input = new List<GeometryPolygon>();
            foreach (Polygon pol in boundaries)
            {
                var vertices = pol.Points.Select(pt => GeometryVertex.ByCoordinates(pt.X, pt.Y, pt.Z)).ToList();
                GeometryPolygon gPol = GeometryPolygon.ByVertices(vertices, true);
                input.Add(gPol);
            }

            foreach (Polygon pol in internals)
            {
                var vertices = pol.Points.Select(pt => GeometryVertex.ByCoordinates(pt.X, pt.Y, pt.Z)).ToList();
                GeometryPolygon gPol = GeometryPolygon.ByVertices(vertices, false);
                input.Add(gPol);
            }

            return new BaseGraph(input);
        }


        /// <summary>
        /// Creates a graph by a set of closed polygons
        /// </summary>
        /// <param name="polygons">Polygons</param>
        /// <returns name="baseGraph">Base graph</returns>
        [IsVisibleInDynamoLibrary(false)]
        public static BaseGraph ByPolygons(List<Polygon> polygons)
        {
            if (polygons == null) throw new NullReferenceException("polygons");
            List<GeometryPolygon> input = new List<GeometryPolygon>();
            foreach (Polygon pol in polygons)
            {
                var vertices = pol.Points.Select(pt => GeometryVertex.ByCoordinates(pt.X, pt.Y, pt.Z)).ToList();
                GeometryPolygon gPol = GeometryPolygon.ByVertices(vertices, false);
                input.Add(gPol);
            }

            return new BaseGraph(input);
        }

        /// <summary>
        /// Creates a new Graph by a set of lines.
        /// </summary>
        /// <param name="lines">Lines</param>
        /// <returns name="baseGraph">Base Graph</returns>
        [IsVisibleInDynamoLibrary(false)]
        public static BaseGraph ByLines(List<Line> lines)
        {
            if (lines == null) throw new NullReferenceException("lines");
            BaseGraph g = new BaseGraph()
            {
                graph = new Graph()
            };

            foreach (Line line in lines)
            {
                GeometryVertex start = Points.ToVertex(line.StartPoint);
                GeometryVertex end = Points.ToVertex(line.EndPoint);
                g.graph.AddEdge(GeometryEdge.ByStartVertexEndVertex(start, end));
            }
            return g;
        }

        #endregion

        #region Override Methods
        /// <summary>
        /// Override of ToString Method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("Graph:(gVertices: {0}, gEdges: {1})", graph.vertices.Count.ToString(), graph.edges.Count.ToString());
        }

        /// <summary>
        /// Customizing the render of Graph
        /// </summary>
        /// <param name="package"></param>
        /// <param name="parameters"></param>
        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(IRenderPackage package, 
            TessellationParameters parameters)
        {
            if (this.GetType() == typeof(Visibility))
            {
                Visibility visGraph = this as Visibility;
                if (visGraph.Factors != null && visGraph.colorRange != null)
                {
                    visGraph.TessellateVisibilityGraph(package, parameters);
                }
                else
                {
                    TesselateBaseGraph(package, parameters);
                }
            }
            else
            {
                TesselateBaseGraph(package, parameters);
            }


        }

        internal void TesselateBaseGraph(IRenderPackage package, 
            TessellationParameters parameters)
        {
            foreach (GeometryVertex v in graph.vertices)
            {
                AddColouredVertex(package, v, vertexDefaultColour);
            }

            foreach (GeometryEdge e in graph.edges)
            {
                AddColouredEdge(package, e, edgeDefaultColour);
            }
        }

        internal static void AddColouredVertex(IRenderPackage package, 
            GeometryVertex vertex, 
            DSCore.Color color)
        {
            package.AddPointVertex(vertex.X, vertex.Y, vertex.Z);
            package.AddPointVertexColor(color.Red, color.Green, color.Blue, color.Alpha);
        }

        internal static void AddColouredEdge(IRenderPackage package, 
            GeometryEdge edge, 
            DSCore.Color color)
        {
            package.AddLineStripVertex(edge.StartVertex.X, edge.StartVertex.Y, edge.StartVertex.Z);
            package.AddLineStripVertex(edge.EndVertex.X, edge.EndVertex.Y, edge.EndVertex.Z);

            package.AddLineStripVertexColor(color.Red, color.Green, color.Blue, color.Alpha);
            package.AddLineStripVertexColor(color.Red, color.Green, color.Blue, color.Alpha);

            package.AddLineStripVertexCount(2);
        }
        #endregion
    }
}
