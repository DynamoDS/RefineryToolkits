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
using Autodesk.RefineryToolkits.SpacePlanning.Graphs.DataStructures;
using System.Collections.Generic;

namespace Autodesk.RefineryToolkits.SpacePlanning.Graphs.Algorithms
{
    public static class Algorithms
    {
        public static Graph Dijkstra(Graph graph, Vertex origin, Vertex destination, Graph tempGraph = null)
        {
            MinPriorityQ<Vertex, double> Q = new();
            bool originInGraph = false;

            foreach (Vertex v in graph.vertices)
            {
                if (v.Equals(origin))
                {
                    originInGraph = true;
                    Q.Add(origin, 0);
                }
                else
                {
                    Q.Add(v, double.PositiveInfinity);
                }
            }

            //If tempGraph is not null, means graph doesn't contain origin and/or destination vertices.
            if (!originInGraph)
            {
                Q.Add(origin, 0);
            }

            if (!graph.Contains(destination)) { Q.Add(destination, double.PositiveInfinity); }

            Dictionary<Vertex, Vertex> ParentVertices = [];
            List<Vertex> S = [];

            while (Q.Size > 0)
            {
                double minDistance = Q.PeekValue();
                Vertex vertex = Q.Take();
                S.Add(vertex);

                if (vertex.Equals(destination)) { break; }

                List<Edge> edges = [.. graph.GetVertexEdges(vertex)];
                if (tempGraph != null && tempGraph.edges.Count != 0)
                {
                    edges.AddRange(tempGraph.GetVertexEdges(vertex));
                }

                foreach (Edge e in edges)
                {
                    Vertex w = e.GetVertexPair(vertex);
                    double newLength = minDistance + e.Length;

                    if (!S.Contains(w) && newLength < Q.GetValue(w))
                    {
                        Q.UpdateItem(w, newLength);
                        //dist[w] = newLength;
                        ParentVertices[w] = vertex;
                    }
                }

            }

            Graph path = new();
            Vertex dest = destination;
            while (dest != origin)
            {
                Vertex parent = ParentVertices[dest];
                path.AddEdge(new Edge(parent, dest));
                dest = parent;
            }
            // Reversing edges list so they will be sorted from origin to target
            path.edges.Reverse();
            return path;

        }

    }
}
