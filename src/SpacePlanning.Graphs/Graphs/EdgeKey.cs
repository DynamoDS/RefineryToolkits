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
using System;

namespace Autodesk.RefineryToolkits.SpacePlanning.Graphs
{
    /// <summary>
    /// VisibilityGraph graph's EdgeKey class to create a tree data structure.
    /// </summary>
    public class EdgeKey : IComparable<EdgeKey>
    {
        internal Vertex Centre { get; private set; }
        internal Vertex Vertex { get; private set; }
        internal Edge Edge { get; private set; }
        internal Edge RayEdge { get; private set; }

        internal EdgeKey(Edge rayEdge, Edge e)
        {
            RayEdge = rayEdge;
            Edge = e;
            Centre = RayEdge.StartVertex;
            Vertex = RayEdge.EndVertex;
        }

        internal EdgeKey(Vertex centre, Vertex end, Edge e)
        {
            Centre = centre;
            Vertex = end;
            Edge = e;
            RayEdge = Edge.ByStartVertexEndVertex(centre, end);
        }

        internal static double DistanceToIntersection(Vertex centre, Vertex maxVertex, Edge e)
        {
            var centreProj = Vertex.ByCoordinates(centre.X, centre.Y, 0);
            var maxProj = Vertex.ByCoordinates(maxVertex.X, maxVertex.Y, 0);
            var startProj = Vertex.ByCoordinates(e.StartVertex.X, e.StartVertex.Y, 0);
            var endProj = Vertex.ByCoordinates(e.EndVertex.X, e.EndVertex.Y, 0);
            Edge rayEdge = Edge.ByStartVertexEndVertex(centreProj, maxProj);
            Edge edgeProj = Edge.ByStartVertexEndVertex(startProj, endProj);
            GeometryBase intersection = rayEdge.Intersection(edgeProj);
            if (intersection != null && intersection.GetType() == typeof(Vertex))
            {
                return centre.DistanceTo((Vertex)intersection);
            }
            else
            {
                return 0;
            }
        }


        /// <summary>
        /// Override of Equals method
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) { return false; }

            EdgeKey k = (EdgeKey)obj;
            return Edge.Equals(k.Edge);
        }

        /// <summary>
        /// Override of GetHashCode method
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Centre.GetHashCode() ^ Vertex.GetHashCode();
        }


        /// <summary>
        /// Implementation of IComparable interaface
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(EdgeKey other)
        {
            if (other == null) { return 1; }
            if (Edge.Equals(other.Edge)) { return 1; }
            if (!VisibilityGraph.EdgeIntersect(RayEdge, other.Edge)) { return -1; }

            double selfDist = DistanceToIntersection(Centre, Vertex, Edge);
            double otherDist = DistanceToIntersection(Centre, Vertex, other.Edge);

            if (selfDist > otherDist) { return 1; }
            else if (selfDist < otherDist) { return -1; }
            else
            {
                Vertex sameVertex = null;
                if (other.Edge.Contains(Edge.StartVertex)) { sameVertex = Edge.StartVertex; }
                else if (other.Edge.Contains(Edge.EndVertex)) { sameVertex = Edge.EndVertex; }
                double aslf = Vertex.ArcRadAngle(Vertex, Centre, Edge.GetVertexPair(sameVertex));
                double aot = Vertex.ArcRadAngle(Vertex, Centre, other.Edge.GetVertexPair(sameVertex));

                if (aslf < aot) { return -1; }
                else { return 1; }
            }

        }

        /// <summary>
        /// Implementaton of IComparable interface
        /// </summary>
        /// <param name="k1"></param>
        /// <param name="k2"></param>
        /// <returns></returns>
        public static bool operator <(EdgeKey k1, EdgeKey k2)
        {
            return k1.CompareTo(k2) < 0;
        }

        /// <summary>
        /// Implementation of IComparable interface
        /// </summary>
        /// <param name="k1"></param>
        /// <param name="k2"></param>
        /// <returns></returns>
        public static bool operator >(EdgeKey k1, EdgeKey k2)
        {
            return k1.CompareTo(k2) > 0;
        }

        /// <summary>
        /// Override of ToString method.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("EdgeKey: (gEdge={0}, centre={1}, vertex={2})", Edge.ToString(), Centre.ToString(), Vertex.ToString());
        }
    }

}
