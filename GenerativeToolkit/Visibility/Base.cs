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
using Graphical.Graphs;
using Dynamo.Graph.Nodes;
using GenerativeToolkit.Geometry;
using Graphical.Core;
using System.Drawing;
#endregion

namespace GenerativeToolkit.Visibility
{
    
    /// <summary>
    /// Representation of a Graph.
    /// </summary>
    public class BaseGraph : IGraphicItem
    {
        #region Internal Properties
        internal Graphical.Graphs.Graph graph { get; set; }
        internal DSCore.Color edgeDefaultColour = DSCore.Color.ByARGB(255, 150, 200, 255);
        internal DSCore.Color vertexDefaultColour = DSCore.Color.ByARGB(255, 75, 125, 180);

        #endregion

        #region Public Properties

        /// <summary>
        /// Checks if the input is a Visibility or Base graph.
        /// </summary>
        /// <param name="graph">Graph</param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static bool IsVisibilityGraph(BaseGraph graph)
        {
            return graph.GetType() == typeof(Graphical.Graphs.VisibilityGraph);
        }

        /// <summary>
        /// Returns the input graph as a list of lines
        /// </summary>
        /// <returns name="lines">List of lines representing the graph.</returns>
        [NodeCategory("Query")]
        public List<Line> Lines()
        {
            List<Line> lines = new List<Line>();
            foreach(gEdge edge in graph.edges)
            {
                var start = Points.ToPoint(edge.StartVertex);
                var end = Points.ToPoint(edge.EndVertex);
                lines.Add(Line.ByStartPointEndPoint(start, end));
            }
            return lines;
        }

        #endregion

        #region Internal Constructors
        internal BaseGraph() { }

        internal BaseGraph(List<gPolygon> gPolygons)
        {
            graph = new Graphical.Graphs.Graph(gPolygons);
        }
        #endregion

        #region Public Constructors
        /// <summary>
        /// Creates a graph by a set of closed polygons
        /// </summary>
        /// <param name="polygons">Polygons</param>
        /// <returns name="baseGraph">Base graph</returns>
        public static BaseGraph ByPolygons(List<Polygon> polygons)
        {
            if (polygons == null) { throw new NullReferenceException("polygons"); }
            List<gPolygon> input = new List<gPolygon>();
            foreach (Polygon pol in polygons)
            {
                var vertices = pol.Points.Select(pt => gVertex.ByCoordinates(pt.X, pt.Y, pt.Z)).ToList();
                gPolygon gPol = gPolygon.ByVertices(vertices, false);
                input.Add(gPol);
            }

            return new BaseGraph(input);
        }

        /// <summary>
        /// Creates a Graph by a set of boundary and internal polygons.
        /// </summary>
        /// <param name="boundaries">Boundary polygons</param>
        /// <param name="internals">Internal polygons</param>
        /// <returns name="baseGraph">Base graph</returns>
        public static BaseGraph ByBoundaryAndInternalPolygons(List<Polygon> boundaries, [DefaultArgument("[]")]List<Polygon> internals)
        {
            if(boundaries == null) { throw new NullReferenceException("boundaryPolygons"); }
            if(internals == null) { throw new NullReferenceException("internalPolygons"); }
            List<gPolygon> input = new List<gPolygon>();
            foreach (Polygon pol in boundaries)
            {
                var vertices = pol.Points.Select(pt => gVertex.ByCoordinates(pt.X, pt.Y, pt.Z)).ToList();
                gPolygon gPol = gPolygon.ByVertices(vertices, true);
                input.Add(gPol);
            }

            foreach (Polygon pol in internals)
            {
                var vertices = pol.Points.Select(pt => gVertex.ByCoordinates(pt.X, pt.Y, pt.Z)).ToList();
                gPolygon gPol = gPolygon.ByVertices(vertices, false);
                input.Add(gPol);
            }

            return new BaseGraph(input);
        }
        
        /// <summary>
        /// Creates a new Graph by a set of lines.
        /// </summary>
        /// <param name="lines">Lines</param>
        /// <returns name="baseGraph">Base Graph</returns>
        public static BaseGraph ByLines(List<Line> lines)
        {
            if(lines == null) { throw new NullReferenceException("lines"); }
            BaseGraph g = new BaseGraph()
            {
                graph = new Graphical.Graphs.Graph()
            };

            foreach(Line line in lines)
            {
                gVertex start = Geometry.Points.ToVertex(line.StartPoint);
                gVertex end = Geometry.Points.ToVertex(line.EndPoint);
                g.graph.AddEdge(gEdge.ByStartVertexEndVertex(start, end));
            }
            return g;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a surface representing the Isovist area visible from 
        /// the given point.
        /// </summary>
        /// <param name="baseGraph">Base Graph</param>
        /// <param name="point">Origin point</param>
        /// <returns name="isovist">Surface representing the isovist area</returns>
        [NodeCategory("Actions")]
        public static Surface IsovistFromPoint(BaseGraph baseGraph, DSPoint point)
        {
            if (baseGraph == null) { throw new ArgumentNullException("graph"); }
            if (point == null) { throw new ArgumentNullException("point"); }

            gVertex origin = gVertex.ByCoordinates(point.X, point.Y, point.Z);

            List<gVertex> vertices = Graphical.Graphs.VisibilityGraph.VertexVisibility(origin, baseGraph.graph);
            List<DSPoint> points = vertices.Select(v => Points.ToPoint(v)).ToList();
            Surface isovist;
            // TODO: Implement better way of checking if polygon is self intersectingç
            
            Polygon polygon = Polygon.ByPoints(points);

            if(polygon.SelfIntersections().Length > 0)
            {
                points.Add(point);
                polygon = Polygon.ByPoints(points);

            }

            return Surface.ByPatch(polygon);
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
        public void Tessellate(IRenderPackage package, TessellationParameters parameters)
        {
            if (this.GetType() == typeof(VisibilityGraph))
            {
                VisibilityGraph visGraph = this as VisibilityGraph;
                if(visGraph.Factors != null && visGraph.colorRange != null)
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

        internal void TesselateBaseGraph(IRenderPackage package, TessellationParameters parameters)
        {
            foreach (gVertex v in graph.vertices)
            {
                AddColouredVertex(package, v, vertexDefaultColour);
            }

            foreach (gEdge e in graph.edges)
            {
                AddColouredEdge(package, e, edgeDefaultColour);
            }
        }

        internal static void AddColouredVertex(IRenderPackage package, gVertex vertex, DSCore.Color color)
        {
            package.AddPointVertex(vertex.X, vertex.Y, vertex.Z);
            package.AddPointVertexColor(color.Red, color.Green, color.Blue, color.Alpha);
        }

        internal static void AddColouredEdge(IRenderPackage package, gEdge edge, DSCore.Color color)
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
