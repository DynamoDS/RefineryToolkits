/***************************************************************************************
* This code was originally created by Alvaro Ortega Pickmans
* Title: Graphical
* Author: Alvaro Ortega Pickmans
* Date: 2017
* Availability: https://github.com/alvpickmans/Graphical
*
***************************************************************************************/

using Autodesk.RefineryToolkits.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Autodesk.RefineryToolkits.SpacePlanning.Graphs
{

    /// <summary>
    /// Representation of a Graph.
    /// </summary>
    public class Graph : ICloneable
    {
        #region Properties

        /// <summary>
        /// GUID to verify uniqueness of graph when cloned
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Polygons dictionary with their Id as dictionary key
        /// </summary>
        internal Dictionary<int, Polygon> polygons = new Dictionary<int, Polygon>();

        /// <summary>
        /// Polygon's Id counter.
        /// </summary>
        internal int? pId { get; private set; }

        /// <summary>
        /// Dictionary with vertex as key and values edges associated with the vertex.
        /// </summary>
        internal Dictionary<Vertex, List<Edge>> graph = new Dictionary<Vertex, List<Edge>>();

        /// <summary>
        /// Graph's vertices
        /// </summary>
        public List<Vertex> vertices { get { return graph.Keys.ToList(); } }

        /// <summary>
        /// Graph's edges
        /// </summary>
        public List<Edge> edges { get; internal set; }

        public List<Polygon> Polygons
        {
            get { return polygons.Values.ToList(); }
        }

        #endregion

        #region Constructors
        public Graph()
        {
            edges = new List<Edge>();
            Id = Guid.NewGuid();
        }

        public Graph(List<Polygon> gPolygonsSet)
        {
            edges = new List<Edge>();
            Id = Guid.NewGuid();
            //Setting up Graph instance by adding vertices, edges and polygons
            foreach (Polygon gPolygon in gPolygonsSet)
            {
                List<Vertex> vertices = gPolygon.vertices;

                // Clear pre-existing edges in the case this is an updating process.
                gPolygon.edges.Clear();

                //If there is only one polygon, treat it as boundary
                if (gPolygonsSet.Count() == 1)
                {
                    gPolygon.isBoundary = true;
                }

                //If first and last point of vertices list are the same, remove last.
                if (vertices.First().Equals(vertices.Last()) && vertices.Count() > 1)
                {
                    vertices = vertices.Take(vertices.Count() - 1).ToList();
                }

                //For each point, creates vertex and associated edge and adds them
                //to the polygons Dictionary
                int vertexCount = vertices.Count();

                // If valid polygon
                if (vertexCount >= 3)
                {
                    int newId = GetNextId();
                    for (var j = 0; j < vertexCount; j++)
                    {
                        int next_index = (j + 1) % vertexCount;
                        Vertex vertex = vertices[j];
                        Vertex next_vertex = vertices[next_index];
                        Edge edge = new Edge(vertex, next_vertex);

                        //If is a valid polygon, add id to vertex and
                        //edge to vertices dictionary
                        if (vertexCount > 2)
                        {
                            vertex.polygonId = newId;
                            next_vertex.polygonId = newId;
                            Polygon gPol = new Polygon();
                            if (polygons.TryGetValue(newId, out gPol))
                            {
                                gPol.edges.Add(edge);
                            }
                            else
                            {
                                gPolygon.edges.Add(edge);
                                gPolygon.id = newId;
                                polygons.Add(newId, gPolygon);
                            }
                        }
                        AddEdge(edge);
                    }
                }

            }
        }

        #endregion

        #region Internal Methods

        internal int GetNextId()
        {
            if (this.pId == null)
            {
                this.pId = 0;
            }
            else
            {
                pId++;
            }
            return pId.Value;
        }

        internal void ResetEdgesFromPolygons()
        {
            this.edges.Clear();
            this.graph.Clear();

            foreach (Polygon polygon in polygons.Values)
            {
                foreach (Edge edge in polygon.edges)
                {
                    this.AddEdge(edge);
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Contains mathod for vertex in graph
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public bool Contains(Vertex vertex)
        {
            return graph.ContainsKey(vertex);
        }

        /// <summary>
        /// Contains method for edges in graph
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public bool Contains(Edge edge)
        {
            return edges.Contains(edge);
        }

        public List<Edge> GetVertexEdges(Vertex vertex)
        {
            List<Edge> edgesList = new List<Edge>();
            if (graph.TryGetValue(vertex, out edgesList))
            {
                return edgesList;
            }
            else
            {
                //graph.Add(vertex, new List<gEdge>());
                return new List<Edge>();
            }
        }

        public List<Vertex> GetAdjecentVertices(Vertex v)
        {
            return graph[v].Select(edge => edge.GetVertexPair(v)).ToList();
        }

        /// <summary>
        /// Add edge to the analisys graph
        /// </summary>
        /// <param name="edge">New edge</param>
        public void AddEdge(Edge edge)
        {
            List<Edge> startEdgesList = new List<Edge>();
            List<Edge> endEdgesList = new List<Edge>();
            if (graph.TryGetValue(edge.StartVertex, out startEdgesList))
            {
                if (!startEdgesList.Contains(edge)) { startEdgesList.Add(edge); }
            }
            else
            {
                graph.Add(edge.StartVertex, new List<Edge>() { edge });
            }

            if (graph.TryGetValue(edge.EndVertex, out endEdgesList))
            {
                if (!endEdgesList.Contains(edge)) { endEdgesList.Add(edge); }
            }
            else
            {
                graph.Add(edge.EndVertex, new List<Edge>() { edge });
            }

            if (!edges.Contains(edge)) { edges.Add(edge); }
        }

        /// <summary>
        /// Computes edges and creates polygons from those connected by vertices.
        /// </summary>
        public void BuildPolygons()
        {
            var computedVertices = new List<Vertex>();

            foreach (Vertex v in vertices)
            {
                // If already belongs to a polygon or is not a polygon vertex or already computed
                if (computedVertices.Contains(v) || graph[v].Count > 2) { continue; }

                computedVertices.Add(v);
                Polygon polygon = new Polygon(GetNextId(), false);

                polygon.AddVertex(v);
                foreach (Edge edge in GetVertexEdges(v))
                {
                    Edge currentEdge = edge;
                    Vertex currentVertex = edge.GetVertexPair(v);
                    while (!polygon.vertices.Contains(currentVertex) || !computedVertices.Contains(currentVertex))
                    {
                        polygon.AddVertex(currentVertex);
                        polygon.edges.Add(currentEdge);

                        var connectedEdges = graph[currentVertex];
                        //It is extreme vertex, polygon not closed
                        if (connectedEdges.Count < 2)
                        {
                            break;
                        }
                        // If just two edges, select the one that is not current nextEdge
                        else if (connectedEdges.Count == 2)
                        {
                            currentEdge = connectedEdges[0].Equals(currentEdge) ? connectedEdges[1] : connectedEdges[0];
                        }
                        // If 4, is self intersection
                        else if (connectedEdges.Count == 4)
                        {
                            var edgesWithVertexAlreadyInPolygon = connectedEdges
                                .Where(e => !e.Equals(currentEdge) && polygon.Vertices.Contains(e.GetVertexPair(currentVertex)))
                                .ToList();
                            //If any of them connects to a vertex already on the current polygon,
                            // If only one, set as current and it will close the polygon on the next iteration
                            if (edgesWithVertexAlreadyInPolygon.Count == 1)
                            {
                                currentEdge = edgesWithVertexAlreadyInPolygon.First();
                            }
                            // If two, it means that is a intersection with two previous edges computed,
                            // so set the next to the one that is not parallel to current
                            else if (edgesWithVertexAlreadyInPolygon.Count == 2)
                            {
                                currentEdge = edgesWithVertexAlreadyInPolygon[0].Direction.IsParallelTo(currentEdge.Direction) ?
                                    edgesWithVertexAlreadyInPolygon[1] :
                                    edgesWithVertexAlreadyInPolygon[0];
                            }
                            // More than two, none on the current polygon so select one of those not yet computed
                            else
                            {
                                polygon.edges.Reverse();
                                polygon.Vertices.Reverse();
                                break; //it will go the other way around
                            }
                        }
                        else
                        {
                            throw new Exception("WARNING. Something unexepected happend with the polygons...");
                        }
                        computedVertices.Add(currentVertex);
                        currentVertex = currentEdge.GetVertexPair(currentVertex);
                    }
                    if (!polygon.edges.Last().Equals(currentEdge))
                    {
                        polygon.edges.Add(currentEdge);
                    }
                }
                this.polygons.Add(polygon.id, polygon);
            }
        }

        #endregion

        #region Override Methods
        //TODO: Improve overriding equality methods as per http://www.loganfranken.com/blog/687/overriding-equals-in-c-part-1/

        /// <summary>
        /// Implementation of IClonable interface
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            Graph newGraph = new Graph()
            {
                graph = new Dictionary<Vertex, List<Edge>>(),
                edges = new List<Edge>(this.edges),
                polygons = new Dictionary<int, Polygon>(this.polygons)
            };

            foreach (var item in this.graph)
            {
                newGraph.graph.Add(item.Key, new List<Edge>(item.Value));
            }

            return newGraph;
        }
        #endregion

    }
}
