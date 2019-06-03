/***************************************************************************************
* This code is originally created by Alvaro Alvaro Ortega Pickmans, and is available
* in his Graphical Packages
* Title: Graphical
* Author: Alvaro Ortega Pickmans
* Date: 2017
* Availability: https://github.com/alvpickmans/Graphical
*
***************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.RefineryToolkits.Core.Utillites;

namespace Autodesk.RefineryToolkits.Core.Geometry
{
    /// <summary>
    /// Representation of Edges on a graph
    /// </summary>
    public class Edge : GeometryBase
    {
        #region Variables
        /// <summary>
        /// StartVertex
        /// </summary>
        public Vertex StartVertex { get; private set; }

        /// <summary>
        /// EndVertex
        /// </summary>
        public Vertex EndVertex { get; private set; }


        public double Length { get; private set; }

        public Vector Direction { get; private set; }

        #endregion

        #region Constructors
        public Edge(Vertex start, Vertex end)
        {
            StartVertex = start;
            EndVertex = end;
            Length = StartVertex.DistanceTo(EndVertex);
            Direction = Vector.ByTwoVertices(StartVertex, EndVertex);
        }

        /// <summary>
        /// gEdge constructor by start and end vertices
        /// </summary>
        /// <param name="start">Start vertex</param>
        /// <param name="end">End Vertex</param>
        /// <returns name="edge">edge</returns>
        public static Edge ByStartVertexEndVertex(Vertex start, Vertex end)
        {
            return new Edge(start, end);
        }

        /// <summary>
        /// gEdge constructor by line
        /// </summary>
        /// <param name="line">line</param>
        /// <returns name="edge">edge</returns>
        //public static gEdge ByLine(Line line)
        //{
        //    Vertex start = Vertex.ByCoordinates(line.StartPoint.X, line.StartPoint.Y, line.StartPoint.Z);
        //    Vertex end = Vertex.ByCoordinates(line.EndPoint.X, line.EndPoint.Y, line.EndPoint.Z);
        //    return new gEdge(start, end);
        //}
        #endregion

        /// <summary>
        /// Method to check if vertex belongs to edge
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public bool Contains(Vertex vertex)
        {
            return StartVertex.Equals(vertex) || EndVertex.Equals(vertex);
        }

        /// <summary>
        /// Method to return the other end vertex of the gEdge
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public Vertex GetVertexPair(Vertex vertex)
        {
            return (StartVertex.Equals(vertex)) ? EndVertex : StartVertex;
        }

        public bool IsCoplanarTo(Edge edge)
        {
            // http://mathworld.wolfram.com/Coplanar.html
            Vector a = this.Direction;
            Vector b = edge.Direction;
            Vector c = Vector.ByTwoVertices(this.StartVertex, edge.StartVertex);

            return c.Dot(a.Cross(b)) == 0;
        }

        public GeometryBase Intersection(Edge other)
        {
            // http://mathworld.wolfram.com/Line-LineIntersection.html
            if (!this.BoundingBox.Intersects(other.BoundingBox)) { return null; }
            if (!this.IsCoplanarTo(other)) { return null; }
            if (this.Equals(other)) { return this; } // Issues if same polygon id???

            var a = this.Direction;
            var b = other.Direction;
            
            if (a.IsParallelTo(b))
            {
                // Fully contains the test edge
                if (other.StartVertex.OnEdge(this) && other.EndVertex.OnEdge(this)) { return other; }
                // Is fully contained by test edge
                else if (this.StartVertex.OnEdge(other) && this.EndVertex.OnEdge(other)) { return this; }
                // Not fully inclusive but overlapping
                else if (this.StartVertex.OnEdge(other) || this.EndVertex.OnEdge(other))
                {
                    Vertex[] vertices = new Vertex[4]
                    {
                        this.StartVertex,
                        this.EndVertex,
                        other.StartVertex,
                        other.EndVertex
                    };
                    var sorted = vertices.OrderBy(v => v.Y).ThenBy(v => v.X).ThenBy(v => v.Z).ToList();
                    return Edge.ByStartVertexEndVertex(sorted[1], sorted[2]);
                }
                // Not intersecting
                else
                {
                    return null;
                }
            }

            // No parallels but intersecting on one of the extreme vertices
            if (other.Contains(this.StartVertex)) { return this.StartVertex; }
            else if (other.Contains(this.EndVertex)) { return this.EndVertex; }


            // No coincident nor same extremes
            var c = Vector.ByTwoVertices(this.StartVertex, other.StartVertex);
            var cxb = c.Cross(b);
            var axb = a.Cross(b);
            var dot = cxb.Dot(axb);

            // If dot == 0 it means that other edge contains at least a vertex from this edge
            // and they are parallel or perpendicular. Cannot be parallel as that was tested before.
            // It might also mean they don't intersect but the would if extending the projections
            double s = (dot) / Math.Pow(axb.Length, 2);

            if (s.AlmostEqualTo(0))
            {
                if (this.StartVertex.OnEdge(other)) { return this.StartVertex; }
                else if(this.EndVertex.OnEdge(other)) { return this.EndVertex; }
                else if(other.StartVertex.OnEdge(this)) { return other.StartVertex; }
                else if(other.EndVertex.OnEdge(this)) { return other.EndVertex; }
                else { return null; }
            }

            
            
            // s > 1, means that "intersection" vertex is not on either edge
            // s == NaN means they are parallels so never intersect
            if (s < 0 || s > 1 || Double.IsNaN(s)) { return null; }

            Vertex intersection = this.StartVertex.Translate(a.Scale(s));

            if (intersection.Equals(other.StartVertex)){ return other.StartVertex; }
            if (intersection.Equals(other.EndVertex)) { return other.EndVertex; }
            if (!intersection.OnEdge(other))
            {
                return null;
            }

            return intersection;
        }

        public bool Intersects(Edge edge)
        {
            if(this.StartVertex.OnEdge(edge) || this.EndVertex.OnEdge(edge))
            {
                if (this.Direction.IsParallelTo(edge.Direction))
                {
                    return true;
                }
            }
            return this.Intersection(edge) != null;
        }

        public double DistanceTo(Vertex vertex)
        {
            return vertex.DistanceTo(this);
        }

        public double DistanceTo(Edge edge)
        {
            // http://mathworld.wolfram.com/Line-LineDistance.html
            if (this.IsCoplanarTo(edge))
            {
                var distances = new double[4]{
                    StartVertex.DistanceTo(edge),
                    EndVertex.DistanceTo(edge),
                    edge.StartVertex.DistanceTo(this),
                    edge.EndVertex.DistanceTo(this)
                };
                return distances.Min();
            }else
            {
                var a = this.Direction;
                var b = edge.Direction;
                var c = Vector.ByTwoVertices(this.StartVertex, edge.StartVertex);
                Vector cross = a.Cross(b);
                double numerator = c.Dot(cross);
                double denominator = cross.Length;
                return Math.Abs(numerator) / Math.Abs(denominator);

            }
            
        }

        #region override methods
        //TODO: Improve overriding equality methods as per http://www.loganfranken.com/blog/687/overriding-equals-in-c-part-1/

        /// <summary>
        /// Override of Equal Method
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) { return false; }

            Edge e= (Edge)obj;
            if (StartVertex.Equals(e.StartVertex) && EndVertex.Equals(e.EndVertex)) { return true; }
            if (StartVertex.Equals(e.EndVertex) && EndVertex.Equals(e.StartVertex)) { return true; }
            return false;

        }

        /// <summary>
        /// Override of GetHashCode Method
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return StartVertex.GetHashCode() ^ EndVertex.GetHashCode();
        }


        /// <summary>
        /// Override of ToString method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("gEdge(StartVertex: {0}, EndVertex: {1})", StartVertex, EndVertex);
        }

        internal override BoundingBox ComputeBoundingBox()
        {
            return BoundingBox.ByMinVertexMaxVertex(StartVertex, EndVertex);
        }

        #endregion
    }   
}
