using GenerativeToolkit.Graphs;
using GenerativeToolkit.Graphs.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeToolkit.Graphs
{
    /// <summary>
    /// VisibilityGraph graph's EdgeKey class to create a tree data structure.
    /// </summary>
    public class EdgeKey : IComparable<EdgeKey>
    {
        internal GeometryVertex Centre { get; private set; }
        internal GeometryVertex Vertex { get; private set; }
        internal GeometryEdge Edge { get; private set; }
        internal GeometryEdge RayEdge { get; private set; }

        internal EdgeKey(GeometryEdge rayEdge, GeometryEdge e)
        {
            RayEdge = rayEdge;
            Edge = e;
            Centre = RayEdge.StartVertex;
            Vertex = RayEdge.EndVertex;
        }

        internal EdgeKey(GeometryVertex centre, GeometryVertex end, GeometryEdge e)
        {
            Centre = centre;
            Vertex = end;
            Edge = e;
            RayEdge = GeometryEdge.ByStartVertexEndVertex(centre, end);
        }

        internal static double DistanceToIntersection(GeometryVertex centre, GeometryVertex maxVertex, GeometryEdge e)
        {
            var centreProj = GeometryVertex.ByCoordinates(centre.X, centre.Y, 0);
            var maxProj = GeometryVertex.ByCoordinates(maxVertex.X, maxVertex.Y, 0);
            var startProj = GeometryVertex.ByCoordinates(e.StartVertex.X, e.StartVertex.Y, 0);
            var endProj = GeometryVertex.ByCoordinates(e.EndVertex.X, e.EndVertex.Y, 0);
            GeometryEdge rayEdge = GeometryEdge.ByStartVertexEndVertex(centreProj, maxProj);
            GeometryEdge edgeProj = GeometryEdge.ByStartVertexEndVertex(startProj, endProj);
            GeometryBase intersection = rayEdge.Intersection(edgeProj);
            if (intersection != null && intersection.GetType() == typeof(GeometryVertex))
            {
                return centre.DistanceTo((GeometryVertex)intersection);
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
                GeometryVertex sameVertex = null;
                if (other.Edge.Contains(Edge.StartVertex)) { sameVertex = Edge.StartVertex; }
                else if (other.Edge.Contains(Edge.EndVertex)) { sameVertex = Edge.EndVertex; }
                double aslf = GeometryVertex.ArcRadAngle(Vertex, Centre, Edge.GetVertexPair(sameVertex));
                double aot = GeometryVertex.ArcRadAngle(Vertex, Centre, other.Edge.GetVertexPair(sameVertex));

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
