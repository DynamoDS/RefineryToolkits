using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenerativeToolkit.Graphs.Graphs;
using GenerativeToolkit.Graphs.Geometry;
using GenerativeToolkit.Graphs.DataStructures;

namespace GenerativeToolkit.Graphs.Algorithms
{
    public static class Algorithms
    {
        
        public static Graph Dijkstra(Graph graph, GeometryVertex origin, GeometryVertex destination, Graph tempGraph = null)
        {
            MinPriorityQ<GeometryVertex, double> Q = new MinPriorityQ<GeometryVertex, double>();
            bool originInGraph = false;

            foreach(GeometryVertex v in graph.vertices)
            {
                if (v.Equals(origin))
                {
                    originInGraph = true;
                    Q.Add(origin, 0);
                }
                else
                {
                    Q.Add(v, Double.PositiveInfinity);
                }
            }

            //If tempGraph is not null, means graph doesn't contain origin and/or destination vertices.
            if (!originInGraph)
            {
                Q.Add(origin, 0);
            }

            if (!graph.Contains(destination)) { Q.Add(destination, Double.PositiveInfinity); }

            Dictionary<GeometryVertex, GeometryVertex> ParentVertices = new Dictionary<GeometryVertex, GeometryVertex>();
            List<GeometryVertex> S = new List<GeometryVertex>();

            while (Q.Size > 0)
            {
                double minDistance = Q.PeekValue();
                GeometryVertex vertex = Q.Take();
                S.Add(vertex);

                if (vertex.Equals(destination)) { break; }

                List<GeometryEdge> edges = new List<GeometryEdge>();
                edges.AddRange(graph.GetVertexEdges(vertex));
                if(tempGraph != null && tempGraph.edges.Any())
                {
                    edges.AddRange(tempGraph.GetVertexEdges(vertex));
                }

                foreach(GeometryEdge e in edges)
                {
                    GeometryVertex w = e.GetVertexPair(vertex);
                    double newLength = minDistance + e.Length;
                    
                    if(!S.Contains(w) && newLength < Q.GetValue(w))
                    {
                        Q.UpdateItem(w, newLength);
                        //dist[w] = newLength;
                        ParentVertices[w] = vertex;
                    }
                }

            }

            Graph path = new Graph();
            GeometryVertex dest = destination;
            while (dest != origin)
            {
                GeometryVertex parent = ParentVertices[dest];
                path.AddEdge(new GeometryEdge(parent, dest));
                dest = parent;
            }
            // Reversing edges list so they will be sorted from origin to target
            path.edges.Reverse();
            return path;
            
        }
        
    }
}
