#region namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenerativeToolkit.Graphs.Geometry;
#endregion

namespace GenerativeToolkit.Graphs
{
    
    /// <summary>
    /// Representation of a Graph.
    /// Graph contains a Dictionary where
    /// </summary>
    public class Graph : ICloneable
    {
        #region Variables

        /// <summary>
        /// GUID to verify uniqueness of graph when cloned
        /// </summary>
        internal Guid Id { get; private set; }

        /// <summary>
        /// Polygons dictionary with their Id as dictionary key
        /// </summary>
        internal Dictionary<int, GeometryPolygon> polygons = new Dictionary<int, GeometryPolygon>();

        /// <summary>
        /// Polygon's Id counter.
        /// </summary>
        internal int? pId { get; private set; }

        /// <summary>
        /// Dictionary with vertex as key and values edges associated with the vertex.
        /// </summary>
        internal Dictionary<GeometryVertex, List<GeometryEdge>> graph = new Dictionary<GeometryVertex, List<GeometryEdge>>();

        /// <summary>
        /// Graph's vertices
        /// </summary>
        public List<GeometryVertex> vertices { get { return graph.Keys.ToList(); } }

        /// <summary>
        /// Graph's edges
        /// </summary>
        public List<GeometryEdge> edges { get; internal set; }

        public List<GeometryPolygon> Polygons
        {
            get { return polygons.Values.ToList(); }
        }

        #endregion

        #region Internal Constructors
        public Graph()
        {
            edges = new List<GeometryEdge>();
            Id = Guid.NewGuid();
        }

        public Graph(List<GeometryPolygon> gPolygonsSet)
        {
            edges = new List<GeometryEdge>();
            Id = Guid.NewGuid();
            //Setting up Graph instance by adding vertices, edges and polygons
            foreach(GeometryPolygon gPolygon in gPolygonsSet)
            {
                List<GeometryVertex> vertices = gPolygon.vertices;

                // Clear pre-existing edges in the case this is an updating process.
                gPolygon.edges.Clear();

                //If there is only one polygon, treat it as boundary
                if(gPolygonsSet.Count() == 1)
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
                        GeometryVertex vertex = vertices[j];
                        GeometryVertex next_vertex = vertices[next_index];
                        GeometryEdge edge = new GeometryEdge(vertex, next_vertex);

                        //If is a valid polygon, add id to vertex and
                        //edge to vertices dictionary
                        if (vertexCount > 2)
                        {
                            vertex.polygonId = newId;
                            next_vertex.polygonId = newId;
                            GeometryPolygon gPol = new GeometryPolygon();
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
            if(this.pId == null)
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

            foreach(GeometryPolygon polygon in polygons.Values)
            {
                foreach(GeometryEdge edge in polygon.edges)
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
        public bool Contains(GeometryVertex vertex)
        {
            return graph.ContainsKey(vertex);
        }

        /// <summary>
        /// Contains method for edges in graph
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public bool Contains(GeometryEdge edge)
        {
            return edges.Contains(edge);
        }

        public List<GeometryEdge> GetVertexEdges(GeometryVertex vertex)
        {
            List<GeometryEdge> edgesList = new List<GeometryEdge>();
            if(graph.TryGetValue(vertex, out edgesList))
            {
                return edgesList;
            }else
            {
                //graph.Add(vertex, new List<gEdge>());
                return new List<GeometryEdge>();
            }
        }

        public List<GeometryVertex> GetAdjecentVertices(GeometryVertex v)
        {
            return graph[v].Select(edge => edge.GetVertexPair(v)).ToList();
        }

        /// <summary>
        /// Add edge to the analisys graph
        /// </summary>
        /// <param name="edge">New edge</param>
        public void AddEdge(GeometryEdge edge)
        {
            List<GeometryEdge> startEdgesList = new List<GeometryEdge>();
            List<GeometryEdge> endEdgesList = new List<GeometryEdge>();
            if (graph.TryGetValue(edge.StartVertex, out startEdgesList))
            {
                if (!startEdgesList.Contains(edge)) { startEdgesList.Add(edge); }
            }
            else
            {
                graph.Add(edge.StartVertex, new List<GeometryEdge>() { edge });
            }

            if (graph.TryGetValue(edge.EndVertex, out endEdgesList))
            {
                if (!endEdgesList.Contains(edge)) { endEdgesList.Add(edge); }
            }
            else
            {
                graph.Add(edge.EndVertex, new List<GeometryEdge>() { edge });
            }
            
            if (!edges.Contains(edge)) { edges.Add(edge); }
        }

        /// <summary>
        /// Computes edges and creates polygons from those connected by vertices.
        /// </summary>
        public void BuildPolygons()
        {
            var computedVertices = new List<GeometryVertex>();
            
            foreach(GeometryVertex v in vertices)
            {
                // If already belongs to a polygon or is not a polygon vertex or already computed
                if( computedVertices.Contains(v) || graph[v].Count > 2) { continue; }

                computedVertices.Add(v);
                GeometryPolygon polygon = new GeometryPolygon(GetNextId(), false);
                
                polygon.AddVertex(v);
                foreach(GeometryEdge edge in GetVertexEdges(v))
                {
                    GeometryEdge currentEdge = edge;
                    GeometryVertex currentVertex = edge.GetVertexPair(v);
                    while (!polygon.vertices.Contains(currentVertex) || !computedVertices.Contains(currentVertex))
                    {
                        polygon.AddVertex(currentVertex);
                        polygon.edges.Add(currentEdge);

                        var connectedEdges = graph[currentVertex];
                        //It is extreme vertex, polygon not closed
                        if(connectedEdges.Count < 2)
                        {
                            break;
                        }
                        // If just two edges, select the one that is not current nextEdge
                        else if(connectedEdges.Count == 2)
                        {
                            currentEdge = connectedEdges[0].Equals(currentEdge) ? connectedEdges[1] : connectedEdges[0];
                        }
                        // If 4, is self intersection
                        else if(connectedEdges.Count == 4)
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
                            else if(edgesWithVertexAlreadyInPolygon.Count == 2)
                            {
                                currentEdge = edgesWithVertexAlreadyInPolygon[0].Direction.IsParallelTo(currentEdge.Direction) ?
                                    edgesWithVertexAlreadyInPolygon[1] :
                                    edgesWithVertexAlreadyInPolygon[0] ; 
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
        /// Customizing the render of gVertex
        /// </summary>
        /// <param name="package"></param>
        /// <param name="parameters"></param>
        //[IsVisibleInDynamoLibrary(false)]
        //public void Tessellate(IRenderPackage package, TessellationParameters parameters)
        //{
        //    foreach(gVertex v in vertices)
        //    {
        //        v.Tessellate(package, parameters);
        //    }
        //    foreach(gEdge e in edges)
        //    {
        //        e.Tessellate(package, parameters);
        //    }
        //}

        /// <summary>
        /// Implementation of IClonable interface
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            Graph newGraph = new Graph()
            {
                graph = new Dictionary<GeometryVertex, List<GeometryEdge>>(),
                edges = new List<GeometryEdge>(this.edges),
                polygons = new Dictionary<int, GeometryPolygon>(this.polygons)
            };

            foreach(var item in this.graph)
            {
                newGraph.graph.Add(item.Key, new List<GeometryEdge>(item.Value));
            }

            return newGraph;
        }
        #endregion

    }
}
