using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Autodesk.RefineryToolkits.SpacePlanning.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using GTGeom = Autodesk.RefineryToolkits.Core.Geometry;

namespace Autodesk.RefineryToolkits.SpacePlanning.Analyze
{
    /// <summary>
    /// Representation of a Graph.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class BaseGraph : IGraphicItem
    {
        #region Properties
        internal Graph graph { get; set; }
        private readonly DSCore.Color edgeDefaultColour = DSCore.Color.ByARGB(255, 150, 200, 255);
        private readonly DSCore.Color vertexDefaultColour = DSCore.Color.ByARGB(255, 75, 125, 180);

        /// <summary>
        /// Checks if the input is a Visibility or Base graph.
        /// </summary>
        /// <returns>Visibility Graph</returns>
        [IsVisibleInDynamoLibrary(false)]
        public bool IsVisibilityGraph() => GetType() == typeof(RepresentableGraph);

        #endregion

        #region Constructors
        internal BaseGraph()
        {
            graph = new Graph();
        }

        internal BaseGraph(List<GTGeom.Polygon> gPolygons)
        {
            graph = new Graph(gPolygons);
        }

        /// <summary>
        /// Creates a Graph by a set of boundary and internal polygons.
        /// </summary>
        /// <param name="boundaries">Boundary polygons</param>
        /// <param name="obstacles">Internal polygons</param>
        /// <returns name="baseGraph">Base graph</returns>
        [IsVisibleInDynamoLibrary(false)]
        public static BaseGraph ByBoundaryAndInternalPolygons(
            List<Polygon> boundaries,
            [DefaultArgument("[]")] List<Polygon> obstacles)
        {
            ArgumentNullException.ThrowIfNull(boundaries);
            ArgumentNullException.ThrowIfNull(obstacles);

            var input = new List<GTGeom.Polygon>();
            input.AddRange(FromDynamoPolygons(boundaries, true));
            input.AddRange(FromDynamoPolygons(obstacles, false));
            //for (var i = 0; i < boundaries.Count; i++)
            //{
            //    var boundary = boundaries[i];
            //    var vertices = boundary.Points
            //        .Select(pt => GTGeom.Vertex.ByCoordinates(pt.X, pt.Y, pt.Z))
            //        .ToList();
            //    GTGeom.Polygon gPol = GTGeom.Polygon.ByVertices(vertices, true);
            //    input.Add(gPol);
            //}

            //for (var i = 0; i < obstacles.Count; i++)
            //{
            //    var obstacle = obstacles[i];
            //    var vertices = obstacle.Points
            //        .Select(pt => GTGeom.Vertex.ByCoordinates(pt.X, pt.Y, pt.Z))
            //        .ToList();
            //    GTGeom.Polygon gPol = GTGeom.Polygon.ByVertices(vertices, false);
            //    input.Add(gPol);
            //}

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
            if (polygons is null || polygons.Count == 0)
                throw new ArgumentNullException(nameof(polygons));
            List<GTGeom.Polygon> input = [.. FromDynamoPolygons(polygons, false)];
            //foreach (DSGeom.Polygon pol in polygons)
            //{
            //    var vertices = pol.Points.Select(pt => GTGeom.Vertex.ByCoordinates(pt.X, pt.Y, pt.Z)).ToList();
            //    GTGeom.Polygon gPol = GTGeom.Polygon.ByVertices(vertices, false);
            //    input.Add(gPol);
            //}

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
            if (lines is null || lines.Count == 0)
                throw new ArgumentNullException(nameof(lines));
            var g = new BaseGraph();

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                GTGeom.Vertex start = GTGeom.Points.ToVertex(line.StartPoint);
                GTGeom.Vertex end = GTGeom.Points.ToVertex(line.EndPoint);
                g.graph.AddEdge(GTGeom.Edge.ByStartVertexEndVertex(start, end));
            }
            return g;
        }

        #endregion

        #region Helpers

        private static List<GTGeom.Polygon> FromDynamoPolygons(List<Polygon> polygons, bool external)
        {
            var gtPolygons = new List<GTGeom.Polygon>();
            for (var i = 0; i < polygons.Count; i++)
            {
                var vertices = ((PolyCurve)polygons[i]).Points
                    .Select(pt => GTGeom.Vertex.ByCoordinates(pt.X, pt.Y, pt.Z))
                    .ToList();
                GTGeom.Polygon gPol = GTGeom.Polygon.ByVertices(vertices, external);
                gtPolygons.Add(gPol);
            }
            return gtPolygons;
        }
        #endregion

        #region Override Methods

        /// <summary>
        /// Override of ToString Method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Graph:(gVertices: {0}, gEdges: {1})", graph.vertices.Count.ToString(), graph.edges.Count.ToString());
        }

        /// <summary>
        /// Customizing the render of Graph
        /// </summary>
        /// <param name="package"></param>
        /// <param name="parameters"></param>
        [IsVisibleInDynamoLibrary(false)]
        public void Tessellate(
            IRenderPackage package,
            TessellationParameters parameters)
        {
            if (GetType() == typeof(RepresentableGraph))
            {
                RepresentableGraph visGraph = this as RepresentableGraph;
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

        internal void TesselateBaseGraph(
            IRenderPackage package,
            TessellationParameters parameters)
        {
            for (var i = 0; i < graph.vertices.Count; i++)
            {
                var v = graph.vertices[i];
                AddColouredVertex(package, v, vertexDefaultColour);
            }

            for (var i = 0; i < graph.edges.Count; i++)
            {
                var e = graph.edges[i];
                AddColouredEdge(package, e, edgeDefaultColour);
            }
        }

        internal static void AddColouredVertex(
            IRenderPackage package,
            GTGeom.Vertex vertex,
            DSCore.Color color)
        {
            package.AddPointVertex(vertex.X, vertex.Y, vertex.Z);
            package.AddPointVertexColor(color.Red, color.Green, color.Blue, color.Alpha);
        }

        internal static void AddColouredEdge(
            IRenderPackage package,
            GTGeom.Edge edge,
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
