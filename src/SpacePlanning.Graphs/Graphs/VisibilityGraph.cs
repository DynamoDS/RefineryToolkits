/***************************************************************************************
* This code is originally created by Alvaro Alvaro Ortega Pickmans, and is available
* in his Graphical Packages
* Title: Graphical
* Author: Alvaro Ortega Pickmans
* Date: 2017
* Availability: https://github.com/alvpickmans/Graphical
*
***************************************************************************************/

using Autodesk.RefineryToolkits.Core.Geometry;
using Autodesk.RefineryToolkits.Core.Utillites;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Autodesk.RefineryToolkits.SpacePlanning.Graphs
{
    /// <summary>
    /// Construction of VisibilityGraph Graph
    /// </summary>
    public class VisibilityGraph : Graph, ICloneable
    {
        #region Internal Properties

        internal Graph baseGraph { get; set; }

        #endregion

        #region Internal Constructors
        internal VisibilityGraph() : base()
        {
            baseGraph = new Graph();
        }

        public VisibilityGraph(Graph inputGraph, bool reducedGraph, bool halfScan = true) : base()
        {
            baseGraph = inputGraph;

            List<Edge> resultEdges = VisibilityAnalysis(baseGraph, baseGraph.vertices, reducedGraph, halfScan);

            foreach (Edge edge in resultEdges)
            {
                this.AddEdge(edge);
            }
        }
        #endregion

        #region Public Constructors

        public static List<Vertex> VertexVisibility(Vertex origin, Graph baseGraph)
        {
            Vertex o = origin;
            if (baseGraph.Contains(origin)) { o = baseGraph.vertices[baseGraph.vertices.IndexOf(origin)]; }
            var visibleVertices = VisibleVertices(o, baseGraph, null, null, null, false, false, true);

            return visibleVertices;

        }

        public static VisibilityGraph Merge(List<VisibilityGraph> graphs)
        {
            Graph graph = new Graph();
            List<Edge> edges = new List<Edge>();
            foreach (VisibilityGraph g in graphs)
            {
                Dictionary<int, int> oldNewIds = new Dictionary<int, int>();
                foreach (Polygon p in g.baseGraph.polygons.Values)
                {
                    int nextId = graph.GetNextId();
                    oldNewIds.Add(p.id, nextId);
                    Polygon polygon = (Polygon)p.Clone();
                    polygon.id = nextId;
                    graph.polygons.Add(nextId, polygon);
                }

                foreach (Edge e in g.edges)
                {
                    Vertex start = (Vertex)e.StartVertex.Clone();
                    Vertex end = (Vertex)e.EndVertex.Clone();
                    //start.polygonId = oldNewIds[start.polygonId];
                    //end.polygonId = oldNewIds[end.polygonId];
                    edges.Add(Edge.ByStartVertexEndVertex(start, end));
                }
            }

            VisibilityGraph visibilityGraph = new VisibilityGraph()
            {
                baseGraph = new Graph(graph.polygons.Values.ToList()),
            };

            foreach (Edge edge in edges)
            {
                visibilityGraph.AddEdge(edge);
            }

            return visibilityGraph;


        }
        #endregion

        #region Internal Methods

        internal List<Edge> VisibilityAnalysis(Graph baseGraph, List<Vertex> vertices, bool reducedGraph, bool halfScan)
        {
            List<Edge> visibleEdges = new List<Edge>();

            foreach (Vertex v in vertices)
            {
                foreach (Vertex v2 in VisibleVertices(v, baseGraph, null, null, null, halfScan, reducedGraph))
                {
                    Edge newEdge = new Edge(v, v2);
                    if (!visibleEdges.Contains(newEdge)) { visibleEdges.Add(newEdge); }
                }
            }

            return visibleEdges;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="centre"></param>
        /// <param name="baseGraph"></param>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <param name="singleVertices"></param>
        /// <param name="scan"></param>
        /// <returns name="visibleVertices">List of vertices visible from the analysed vertex</returns>
        public static List<Vertex> VisibleVertices(
            Vertex centre,
            Graph baseGraph,
            Vertex origin = null,
            Vertex destination = null,
            List<Vertex> singleVertices = null,
            bool halfScan = true,
            bool reducedGraph = true,
            bool maxVisibility = false)
        {
            #region Initialize variables and sort vertices
            List<Edge> edges = baseGraph.edges;
            List<Vertex> vertices = baseGraph.vertices;


            if (origin != null) { vertices.Add(origin); }
            if (destination != null) { vertices.Add(destination); }
            if (singleVertices != null) { vertices.AddRange(singleVertices); }


            Vertex maxVertex = vertices.OrderByDescending(v => v.DistanceTo(centre)).First();
            double maxDistance = centre.DistanceTo(maxVertex) * 1.5;
            //vertices = vertices.OrderBy(v => Point.RadAngle(centre.point, v.point)).ThenBy(v => centre.DistanceTo(v)).ToList();
            vertices = Vertex.OrderByRadianAndDistance(vertices, centre);

            #endregion

            #region Initialize openEdges
            //Initialize openEdges with any intersection edges on the half line 
            //from centre to maxDistance on the XAxis
            List<EdgeKey> openEdges = new List<EdgeKey>();
            double xMax = Math.Abs(centre.X) + 1.5 * maxDistance;
            Edge halfEdge = Edge.ByStartVertexEndVertex(centre, Vertex.ByCoordinates(xMax, centre.Y, centre.Z));
            foreach (Edge e in edges)
            {
                if (centre.OnEdge(e)) { continue; }
                if (halfEdge.Intersects(e))
                {
                    if (e.StartVertex.OnEdge(halfEdge)) { continue; }
                    if (e.EndVertex.OnEdge(halfEdge)) { continue; }
                    EdgeKey k = new EdgeKey(halfEdge, e);
                    openEdges.AddItemSorted(k);
                }
            }

            #endregion

            List<Vertex> visibleVertices = new List<Vertex>();
            Vertex prev = null;
            bool prevVisible = false;
            for (var i = 0; i < vertices.Count; i++)
            {
                Vertex vertex = vertices[i];
                if (vertex.Equals(centre) || vertex.Equals(prev)) { continue; }// v == to centre or to previous when updating graph
                //Check only half of vertices as eventually they will become 'v'
                if (halfScan && Vertex.RadAngle(centre, vertex) > Math.PI) { break; }
                //Removing clock wise edges incident on v
                if (openEdges.Count > 0 && baseGraph.graph.ContainsKey(vertex))
                {
                    foreach (Edge edge in baseGraph.graph[vertex])
                    {
                        int orientation = Vertex.Orientation(centre, vertex, edge.GetVertexPair(vertex));

                        if (orientation == -1)
                        {
                            EdgeKey k = new EdgeKey(centre, vertex, edge);
                            int index = openEdges.BisectIndex(k) - 1;
                            index = (index < 0) ? openEdges.Count - 1 : index;
                            if (openEdges.Count > 0 && openEdges.ElementAt(index).Equals(k))
                            {
                                openEdges.RemoveAt(index);
                            }
                        }
                    }
                }

                //Checking if p is visible from p.
                bool isVisible = false;
                Polygon vertexPolygon = null;
                if (vertex.polygonId >= 0)
                {
                    baseGraph.polygons.TryGetValue(vertex.polygonId, out vertexPolygon);
                }
                // If centre is on an edge of a inner polygon vertex belongs, check if the centre-vertex edge lies inside
                // or if on one of vertex's edges.
                if (vertexPolygon != null && !vertexPolygon.isBoundary && vertexPolygon.ContainsVertex(centre))
                {
                    Vertex mid = Vertex.MidVertex(centre, vertex);
                    // If mid is on any edge of vertex, is visible, otherwise not.
                    foreach (Edge edge in baseGraph.graph[vertex])
                    {
                        if (mid.OnEdge(edge))
                        {
                            isVisible = true;
                            break;
                        }
                    }
                }
                //No collinear vertices
                else if (prev == null || Vertex.Orientation(centre, prev, vertex) != 0 || !prev.OnEdge(centre, vertex))
                {

                    if (openEdges.Count == 0)
                    {
                        if (vertexPolygon != null && vertexPolygon.isBoundary && vertexPolygon.ContainsVertex(centre))
                        {
                            isVisible = vertexPolygon.ContainsVertex(Vertex.MidVertex(centre, vertex));
                        }
                        else
                        {
                            isVisible = true;
                        }
                    }
                    else if (vertex.OnEdge(openEdges.First().Edge) || !openEdges.First().Edge.Intersects(new Edge(centre, vertex))) //TODO: Change this intersection to Edge.Intersects
                    {
                        isVisible = true;
                    }
                }
                //For collinear vertices, if previous was not visible, vertex is not either
                else if (!prevVisible)
                {
                    isVisible = false;
                }
                //For collinear vertices, if prev was visible need to check that
                //the edge from prev to vertex does not intersect with any open edge
                else
                {
                    isVisible = true;
                    foreach (EdgeKey k in openEdges)
                    {
                        //if (!k.edge.Contains(prev) && EdgeIntersect(prev, vertex, k.edge))
                        if (EdgeIntersect(prev, vertex, k.Edge) && !k.Edge.Contains(prev))
                        {
                            isVisible = false;
                            break;
                        }
                    }
                    // If visible (doesn't intersect any open edge) and edge 'prev-vertex'
                    // is in any polygon, vertex is visible if it belongs to a external boundary
                    if (isVisible && EdgeInPolygon(prev, vertex, baseGraph, maxDistance))
                    {
                        isVisible = IsBoundaryVertex(vertex, baseGraph);
                    }

                    // If still visible (not inside polygon or is boundary vertex),
                    // if not on 'centre-prev' edge means there is a gap between prev and vertex
                    if (isVisible && !vertex.OnEdge(centre, prev))
                    {
                        isVisible = !IsBoundaryVertex(vertex, baseGraph);
                    }
                }

                //If vertex is visible and centre belongs to any polygon, checks
                //if the visible edge is interior to its polygon
                if (isVisible && centre.polygonId >= 0 && !baseGraph.GetAdjecentVertices(centre).Contains(vertex))
                {
                    if (IsBoundaryVertex(centre, baseGraph) && IsBoundaryVertex(vertex, baseGraph))
                    {
                        isVisible = EdgeInPolygon(centre, vertex, baseGraph, maxDistance);
                    }
                    else
                    {
                        isVisible = !EdgeInPolygon(centre, vertex, baseGraph, maxDistance);
                    }
                }


                prev = vertex;
                prevVisible = isVisible;


                if (isVisible)
                {
                    // Check reducedGraph if vertices belongs to different polygons
                    if (reducedGraph && centre.polygonId != vertex.polygonId)
                    {
                        bool isOriginExtreme = true;
                        bool isTargetExtreme = true;
                        // For reduced graphs, it is checked if the edge is extrem or not.
                        // For an edge to be extreme, the edges coincident at the start and end vertex
                        // will have the same orientation (both clock or counter-clock wise)

                        // Vertex belongs to a polygon
                        if (centre.polygonId >= 0 && !IsBoundaryVertex(centre, baseGraph))
                        {
                            var orientationsOrigin = baseGraph.GetAdjecentVertices(centre).Select(otherVertex => Vertex.Orientation(vertex, centre, otherVertex)).ToList();
                            isOriginExtreme = orientationsOrigin.All(o => o == orientationsOrigin.First());
                        }

                        if (vertex.polygonId >= 0 && !IsBoundaryVertex(vertex, baseGraph))
                        {
                            var orientationsTarget = baseGraph.GetAdjecentVertices(vertex).Select(otherVertex => Vertex.Orientation(centre, vertex, otherVertex)).ToList();
                            isTargetExtreme = orientationsTarget.All(o => o == orientationsTarget.First());
                        }

                        if (isTargetExtreme || isOriginExtreme) { visibleVertices.Add(vertex); }
                    }
                    else
                    {
                        visibleVertices.Add(vertex);
                    }
                }

                if (baseGraph.Contains(vertex))
                {
                    foreach (Edge e in baseGraph.graph[vertex])
                    {
                        if (!centre.OnEdge(e) && Vertex.Orientation(centre, vertex, e.GetVertexPair(vertex)) == 1)
                        {
                            EdgeKey k = new EdgeKey(centre, vertex, e);
                            openEdges.AddItemSorted(k);
                        }
                    }
                }

                if (isVisible && maxVisibility && vertex.polygonId >= 0)
                {
                    List<Vertex> vertexPairs = baseGraph.GetAdjecentVertices(vertex);
                    int firstOrientation = Vertex.Orientation(centre, vertex, vertexPairs[0]);
                    int secondOrientation = Vertex.Orientation(centre, vertex, vertexPairs[1]);
                    bool isColinear = false;

                    //if both edges lie on the same side of the centre-vertex edge or one of them is colinear or centre is contained on any of the edges
                    if (firstOrientation == secondOrientation || firstOrientation == 0 || secondOrientation == 0)
                    {
                        Vertex rayVertex = vertex.Translate(Vector.ByTwoVertices(centre, vertex), maxDistance);
                        Edge rayEdge = Edge.ByStartVertexEndVertex(centre, rayVertex);
                        Vertex projectionVertex = null;

                        // if both orientation are not on the same side, means that one of them is colinear
                        isColinear = firstOrientation != secondOrientation ? true : false;

                        foreach (EdgeKey ek in openEdges)
                        {
                            Vertex intersection = rayEdge.Intersection(ek.Edge) as Vertex;
                            if (intersection != null && !intersection.Equals(vertex))
                            {
                                projectionVertex = intersection;
                                Polygon polygon = null;
                                baseGraph.polygons.TryGetValue(vertex.polygonId, out polygon);
                                if (polygon != null)
                                {
                                    // If polygon is internal, don't compute intersection if mid point lies inside the polygon but not on its edges
                                    Vertex mid = Vertex.MidVertex(vertex, intersection);
                                    bool containsEdge = Vertex.Orientation(centre, vertex, mid) != 0 && polygon.ContainsVertex(mid);
                                    if (!polygon.isBoundary && containsEdge)
                                    {
                                        projectionVertex = null;
                                    }
                                }
                                break;
                            }
                        }
                        if (projectionVertex != null)
                        {
                            // if edges are before rayEdge, projection Vertex goes after vertex
                            if (firstOrientation == -1 || secondOrientation == -1)
                            {
                                visibleVertices.Add(projectionVertex);
                            }
                            else
                            {
                                visibleVertices.Insert(visibleVertices.Count - 1, projectionVertex);
                            }
                        }
                    }
                    if (vertexPairs.Contains(centre) && !visibleVertices.Contains(centre))
                    {
                        visibleVertices.Add(centre);
                    }
                }
            }

            return visibleVertices;
        }

        internal static bool EdgeIntersect(Edge halfEdge, Edge edge)
        {
            //For simplicity, it only takes into acount the 2d projection to the xy plane,
            //so the result will be based on a porjection even if points have z values.
            bool intersects = EdgeIntersectProjection(
                halfEdge.StartVertex,
                halfEdge.EndVertex,
                edge.StartVertex,
                edge.EndVertex,
                "xy");

            return intersects;
        }

        internal static bool EdgeIntersect(Vertex start, Vertex end, Edge edge)
        {
            //For simplicity, it only takes into acount the 2d projection to the xy plane,
            //so the result will be based on a projection even if points have z values.
            bool intersects = EdgeIntersectProjection(
                start,
                end,
                edge.StartVertex,
                edge.EndVertex,
                "xy");

            return intersects;
        }

        internal static bool EdgeIntersectProjection(
            Vertex p1,
            Vertex q1,
            Vertex p2,
            Vertex q2,
            string plane = "xy")
        {
            //For more details https://www.geeksforgeeks.org/check-if-two-given-line-segments-intersect/

            int o1 = Vertex.Orientation(p1, q1, p2, plane);
            int o2 = Vertex.Orientation(p1, q1, q2, plane);
            int o3 = Vertex.Orientation(p2, q2, p1, plane);
            int o4 = Vertex.Orientation(p2, q2, q1, plane);

            //General case
            if (o1 != o2 && o3 != o4) { return true; }

            //Special Cases
            // p1, q1 and p2 are colinear and p2 lies on segment p1q1
            if (o1 == 0 && Vertex.OnEdgeProjection(p1, p2, q1, plane)) { return true; }

            // p1, q1 and p2 are colinear and q2 lies on segment p1q1
            if (o2 == 0 && Vertex.OnEdgeProjection(p1, q2, q1, plane)) { return true; }

            // p2, q2 and p1 are colinear and p1 lies on segment p2q2
            if (o3 == 0 && Vertex.OnEdgeProjection(p2, p1, q2, plane)) { return true; }

            // p2, q2 and q1 are colinear and q1 lies on segment p2q2
            if (o4 == 0 && Vertex.OnEdgeProjection(p2, q1, q2, plane)) { return true; }

            return false; //Doesn't fall on any of the above cases


        }

        internal static bool EdgeInPolygon(Vertex v1, Vertex v2, Graph graph, double maxDistance)
        {
            //Not on the same polygon
            if (v1.polygonId != v2.polygonId) { return false; }
            //At least one doesn't belong to any polygon
            if (v1.polygonId == -1 || v2.polygonId == -1) { return false; }
            Vertex midVertex = Vertex.MidVertex(v1, v2);
            return graph.polygons[v1.polygonId].ContainsVertex(midVertex);
        }

        internal static bool IsBoundaryVertex(Vertex vertex, Graph graph)
        {
            return (vertex.polygonId < 0) ? false : graph.polygons[vertex.polygonId].isBoundary;
        }


        #endregion

        #region Public Methods
        /// <summary>
        /// Adds specific lines as gEdges to the visibility graph
        /// </summary>
        /// <param name="visibilityGraph">VisibilityGraph Graph</param>
        /// <param name="edges">Lines to add as new gEdges</param>
        /// <returns></returns>
        public static VisibilityGraph AddEdges(VisibilityGraph visibilityGraph, List<Edge> edges)
        {
            if (edges == null) { throw new NullReferenceException("edges"); }
            List<Vertex> singleVertices = new List<Vertex>();

            foreach (Edge e in edges)
            {
                if (!singleVertices.Contains(e.StartVertex)) { singleVertices.Add(e.StartVertex); }
                if (!singleVertices.Contains(e.EndVertex)) { singleVertices.Add(e.EndVertex); }
            }
            VisibilityGraph updatedGraph = (VisibilityGraph)visibilityGraph.Clone();
            if (singleVertices.Any())
            {
                updatedGraph = AddVertices(visibilityGraph, singleVertices);

            }

            foreach (Edge e in edges) { updatedGraph.AddEdge(e); }

            return updatedGraph;
        }

        /// <summary>
        /// Adds specific points as gVertices to the VisibilityGraph Graph
        /// </summary>
        /// <param name="visibilityGraph">VisibilityGraph Graph</param>
        /// <param name="vertices">Points to add as gVertices</param>
        /// <returns></returns>
        public static VisibilityGraph AddVertices(VisibilityGraph visibilityGraph, List<Vertex> vertices, bool reducedGraph = true)
        {
            //TODO: Seems that original graph gets updated as well
            if (vertices == null) { throw new NullReferenceException("vertices"); }

            VisibilityGraph newVisGraph = (VisibilityGraph)visibilityGraph.Clone();
            List<Vertex> singleVertices = new List<Vertex>();

            foreach (Vertex v in vertices)
            {
                if (newVisGraph.Contains(v)) { continue; }
                Edge closestEdge = newVisGraph.baseGraph.edges.OrderBy(e => e.DistanceTo(v)).First();

                if (!closestEdge.DistanceTo(v).AlmostEqualTo(0))
                {
                    singleVertices.Add(v);
                }
                else if (v.OnEdge(closestEdge.StartVertex, closestEdge.EndVertex))
                {
                    v.polygonId = closestEdge.StartVertex.polygonId;
                    newVisGraph.baseGraph.polygons[v.polygonId] = newVisGraph.baseGraph.polygons[v.polygonId].AddVertex(v, closestEdge);
                    singleVertices.Add(v);
                }
            }

            newVisGraph.baseGraph.ResetEdgesFromPolygons();

            foreach (Vertex centre in singleVertices)
            {
                foreach (Vertex v in VisibleVertices(centre, newVisGraph.baseGraph, null, null, singleVertices, false, reducedGraph))
                {
                    newVisGraph.AddEdge(new Edge(centre, v));
                }
            }

            return newVisGraph;
        }

        public static Graph ShortestPath(VisibilityGraph visibilityGraph, Vertex origin, Vertex destination)
        {
            Graph shortest;

            bool containsOrigin = visibilityGraph.Contains(origin);
            bool containsDestination = visibilityGraph.Contains(destination);

            if (containsOrigin && containsDestination)
            {
                shortest = Algorithms.Algorithms.Dijkstra(visibilityGraph, origin, destination);
            }
            else
            {
                Vertex gO = (!containsOrigin) ? origin : null;
                Vertex gD = (!containsDestination) ? destination : null;
                Graph tempGraph = new Graph();

                if (!containsOrigin)
                {
                    foreach (Vertex v in VisibleVertices(origin, visibilityGraph.baseGraph, null, gD, null, false, true))
                    {
                        tempGraph.AddEdge(new Edge(origin, v));
                    }
                }
                if (!containsDestination)
                {
                    foreach (Vertex v in VisibleVertices(destination, visibilityGraph.baseGraph, gO, null, null, false, true))
                    {
                        tempGraph.AddEdge(new Edge(destination, v));
                    }
                }
                shortest = Algorithms.Algorithms.Dijkstra(visibilityGraph, origin, destination, tempGraph);
            }


            return shortest;
        }

        public List<double> ConnectivityFactor()
        {
            List<int> connected = new List<int>();
            foreach (Edge edge in edges)
            {
                connected.Add(graph[edge.StartVertex].Count + graph[edge.EndVertex].Count());
            }
            int min = connected.Min();
            int max = connected.Max();

            return connected.Select(x => Convert.ToDouble(x).Map(min, max, 0, 1)).ToList();
        }

        #endregion

        public new object Clone()
        {
            VisibilityGraph newGraph = new VisibilityGraph()
            {
                graph = new Dictionary<Vertex, List<Edge>>(),
                edges = new List<Edge>(this.edges),
                polygons = new Dictionary<int, Polygon>(this.polygons),
                baseGraph = (Graph)this.baseGraph.Clone()
            };

            foreach (var item in this.graph)
            {
                newGraph.graph.Add(item.Key, new List<Edge>(item.Value));
            }

            return newGraph;
        }

    }
}