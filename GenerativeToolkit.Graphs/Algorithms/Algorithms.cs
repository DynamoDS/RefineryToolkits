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
        
        public static Graph Dijkstra(Graph graph, gVertex origin, gVertex destination, Graph tempGraph = null)
        {
            MinPriorityQ<gVertex, double> Q = new MinPriorityQ<gVertex, double>();
            bool originInGraph = false;

            foreach(gVertex v in graph.vertices)
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

            Dictionary<gVertex, gVertex> ParentVertices = new Dictionary<gVertex, gVertex>();
            List<gVertex> S = new List<gVertex>();

            while (Q.Size > 0)
            {
                double minDistance = Q.PeekValue();
                gVertex vertex = Q.Take();
                S.Add(vertex);

                if (vertex.Equals(destination)) { break; }

                List<gEdge> edges = new List<gEdge>();
                edges.AddRange(graph.GetVertexEdges(vertex));
                if(tempGraph != null && tempGraph.edges.Any())
                {
                    edges.AddRange(tempGraph.GetVertexEdges(vertex));
                }

                foreach(gEdge e in edges)
                {
                    gVertex w = e.GetVertexPair(vertex);
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
            gVertex dest = destination;
            while (dest != origin)
            {
                gVertex parent = ParentVertices[dest];
                path.AddEdge(new gEdge(parent, dest));
                dest = parent;
            }
            // Reversing edges list so they will be sorted from origin to target
            path.edges.Reverse();
            return path;
            
        }
        
    }
}
