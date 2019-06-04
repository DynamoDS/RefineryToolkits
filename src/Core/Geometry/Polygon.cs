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

namespace Autodesk.RefineryToolkits.Core.Geometry
{
    /// <summary>
    /// gPolygon class to hold graph´s polygon information in relation to its function on the graph
    /// like if it is internal or limit boundary.
    /// </summary>
    public class Polygon : GeometryBase, ICloneable
    {
        #region Internal Variables

        /// <summary>
        /// Polygon's id
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// Flag to check polygons role: Internal or Boundary
        /// </summary>
        public bool isBoundary { get; set; }

        /// <summary>
        /// Polygon's edges
        /// </summary>
        public List<Edge> edges = new List<Edge>();

        /// <summary>
        /// Polygon's Vertices
        /// </summary>
        public List<Vertex> vertices = new List<Vertex>();
        #endregion

        #region Public Variables
        /// <summary>
        /// gPolygon's vertices
        /// </summary>
        public List<Vertex> Vertices
        {
            get { return vertices; }
        }

        /// <summary>
        /// gPolygon's edges
        /// </summary>
        public List<Edge> Edges
        {
            get
            {
                return edges;
            }
        }

        /// <summary>
        /// Determines if the gPolygon is closed.
        /// </summary>
        public bool IsClosed
        {
            get
            {
                return this.edges.Count > 2 && (edges.First().StartVertex.OnEdge(edges.Last()) || edges.First().EndVertex.OnEdge(edges.Last()));
            }
        }
        #endregion

        #region Internal Constructors
        public Polygon() { }

        public Polygon(int _id, bool _isExternal)
        {
            id = _id;
            isBoundary = _isExternal;
        }
        #endregion

        #region Public Constructos
        /// <summary>
        /// Creates a new gPolygon by a list of ordered vertices.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="isExternal"></param>
        /// <returns></returns>
        public static Polygon ByVertices(List<Vertex> vertices, bool isExternal = false)
        {
            Polygon polygon = new Polygon(-1, isExternal);
            polygon.vertices = vertices;
            int vertexCount = vertices.Count;
            for (var j = 0; j < vertexCount; j++)
            {
                int next_index = (j + 1) % vertexCount;
                Vertex vertex = vertices[j];
                Vertex next_vertex = vertices[next_index];
                polygon.edges.Add( new Edge(vertex, next_vertex));
            }
            return polygon;
        }

        public static Polygon ByCenterRadiusAndSides(Vertex center, double radius, int sides)
        {
            // TODO: create polygon by plane?
            if(sides < 3) { throw new ArgumentOutOfRangeException("sides", "Any polygon must have at least 3 sides."); }
            List<Vertex> vertices = new List<Vertex>();
            double angle = (Math.PI * 2) / sides;
            for(var i = 0; i < sides; i++)
            {
                var vertex = Vertex.ByCoordinates(
                        (Math.Sin(i * angle) * radius) + center.X,
                        (Math.Cos(i * angle) * radius) + center.Y,
                        center.Z
                        );
                vertices.Add(vertex);
            }
            return Polygon.ByVertices(vertices);
        }
        #endregion

        #region Internal Methods
        public void AddVertex(Vertex vertex)
        {
            if (vertex == null) return;

            vertex.polygonId = this.id;
            vertices.Add(vertex);
        }

        public Polygon AddVertex(Vertex v, Edge intersectingEdge)
        {
            //Assumes that vertex v intersects one of polygons edges.
            Polygon newPolygon = (Polygon)this.Clone();

            // Assign the polygon Id to the new vertex.
            v.polygonId = this.id;

            // Getting the index of the intersecting edge's start vertex and
            // inserting the new vertex at the following index.
            int index = newPolygon.vertices.IndexOf(intersectingEdge.StartVertex);
            newPolygon.vertices.Insert(index + 1, v);

            // Rebuilding edges.
            newPolygon.edges.Clear();
            int verticesCount = newPolygon.vertices.Count;
            for (var i = 0; i < verticesCount; i++)
            {
                int nextIndex = (i + 1) % verticesCount;
                newPolygon.edges.Add(new Edge(newPolygon.vertices[i], newPolygon.vertices[nextIndex]));
            }

            return newPolygon;
        }


        /// <summary>
        /// R
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="vertex"></param>
        /// <returns> 
        ///     greater than 0 for vertex left of the edge
        ///     equal 0 for vertex on the edge
        ///     less than 0 for vertex right of the edege
        /// </returns>
        internal double IsLeft(Edge edge, Vertex vertex)
        {
            var left = (edge.EndVertex.X - edge.StartVertex.X) * (vertex.Y - edge.StartVertex.Y) - (edge.EndVertex.Y - edge.StartVertex.Y) * (vertex.X - edge.StartVertex.X);
            return left;
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Determines if a Vertex is inside the gPolygon using Fast Winding Number method
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public bool ContainsVertex(Vertex vertex)
        {
            // http://geomalgorithms.com/a03-_inclusion.html
            Vertex maxVertex = vertices.OrderByDescending(v => v.DistanceTo(vertex)).First();
            double maxDistance = vertex.DistanceTo(maxVertex) * 1.5;
            Vertex v2 = Vertex.ByCoordinates(vertex.X + maxDistance, vertex.Y, vertex.Z);
            Edge ray = Edge.ByStartVertexEndVertex(vertex, v2);
            int windNumber = 0;
            foreach (Edge edge in edges)
            {
                if(vertex.OnEdge(edge)) { return true; }
                Vertex intersection = ray.Intersection(edge) as Vertex;
                if (intersection is Vertex)
                {
                    if (edge.StartVertex.Y <= vertex.Y)
                    {
                        if (edge.EndVertex.Y > vertex.Y)
                        {
                            if(IsLeft(edge, vertex) > 0) {
                                ++windNumber;
                            }
                        }
                    }
                    else
                    {
                        if (edge.EndVertex.Y < vertex.Y)
                        {
                            if(IsLeft(edge, vertex) < 0)
                            {
                                --windNumber;
                            }
                        }
                    }
                }
            }

            // If windNumber is different from 0, vertex is in polygon
            return windNumber != 0;
        }

        /// <summary>
        /// Determines if a gEdge is inside the gPolygon by comparing
        /// it's start, end and mid vertices.
        /// Note: Prone to error if polygon has edges intersecting the edge not at mid vertex?
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public bool ContainsEdge(Edge edge)
        {
            // TODO: Check if edge intersects polygon in vertices different than start/end.
            return this.ContainsVertex(edge.StartVertex)
                && this.ContainsVertex(edge.EndVertex)
                && this.ContainsVertex(Vertex.MidVertex(edge.StartVertex, edge.EndVertex));
        }
        /// <summary>
        /// Checks if a polygon is planar
        /// </summary>
        /// <param name="polygon">gPolygon</param>
        /// <returns>boolean</returns>
        public static bool IsPlanar(Polygon polygon)
        {
            return Vertex.Coplanar(polygon.Vertices);
        }

        /// <summary>
        /// Checks if two gPolygons are coplanar.
        /// </summary>
        /// <param name="polygon">gPolygon</param>
        /// <param name="otherPolygon">Other gPolygon</param>
        /// <returns></returns>
        public static bool Coplanar(Polygon polygon, Polygon otherPolygon)
        {
            List<Vertex> joinedVertices = new List<Vertex>(polygon.Vertices);
            joinedVertices.AddRange(otherPolygon.Vertices);

            return Vertex.Coplanar(joinedVertices);
        }
        /*

        /// <summary>
        /// Determines if two polygons are intersecting
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public bool Intersects(Polygon polygon)
        {
            if (!this.BoundingBox.Intersects(polygon.BoundingBox)) { return false; }
            var sw = new SweepLine(this, polygon, SweepLineType.Intersects);
            return sw.HasIntersection();
        }
        
        /// <summary>
        /// Performes a Union boolean operation between this polygon and a clipping one.
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        public List<Polygon> Union(Polygon clip)
        {
            
            var swLine = new SweepLine(this, clip, SweepLineType.Boolean);

            return swLine.ComputeBooleanOperation(BooleanType.Union);
        }

        public static List<Polygon> Union(List<Polygon> subjects, List<Polygon> clips)
        {
            List<Polygon> result = new List<Polygon>(subjects);
            int count = 0;
            foreach (Polygon clip in clips)
            {
                for (var i = count; i < result.Count; i++)
                {
                    result.AddRange(result[i].Union(clip));
                    count++;
                }
            }
            return result;
        }

        /// <summary>
        /// Performes a Difference boolean operation between this polygon and a clipping one.
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        public List<Polygon> Difference(Polygon clip)
        {
            var swLine = new SweepLine(this, clip, SweepLineType.Boolean);

            return swLine.ComputeBooleanOperation(BooleanType.Differenece);
        }

        public static List<Polygon> Difference(List<Polygon> subjects, List<Polygon> clips)
        {
            List<Polygon> result = new List<Polygon>(subjects);
            int count = 0;
            foreach (Polygon clip in clips)
            {
                for(var i = count; i < result.Count; i++)
                {
                    result.AddRange(result[i].Difference(clip));
                    count++;
                }
            }
            return result;
        }

        /// <summary>
        /// Performes a Intersection boolean operation between this polygon and a clipping one.
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        public List<Polygon> Intersection(Polygon clip)
        {
            var swLine = new SweepLine(this, clip, SweepLineType.Boolean);

            return swLine.ComputeBooleanOperation(BooleanType.Intersection);
        }

        public static List<Polygon> Intersection(List<Polygon> subjects, List<Polygon> clips)
        {
            List<Polygon> result = new List<Polygon>(subjects);
            int count = 0;
            foreach (Polygon clip in clips)
            {
                for (var i = count; i < result.Count; i++)
                {
                    result.AddRange(result[i].Intersection(clip));
                    count++;
                }
            }
            return result;
        }
        */
        #endregion

        /// <summary>
        /// Clone method for gPolygon
        /// </summary>
        /// <returns>Cloned gPolygon</returns>
        public object Clone()
        {
            Polygon newPolygon = new Polygon(this.id, this.isBoundary);
            newPolygon.edges = new List<Edge>(this.edges);
            newPolygon.vertices = new List<Vertex>(this.vertices);
            return newPolygon;
        }

        internal override BoundingBox ComputeBoundingBox()
        {
            var xCoord = new List<double>(this.vertices.Count);
            var yCoord = new List<double>(this.vertices.Count);
            var zCoord = new List<double>(this.vertices.Count);
            foreach(Vertex v in vertices)
            {
                xCoord.Add(v.X);
                yCoord.Add(v.Y);
                zCoord.Add(v.Z);
            }
            return new BoundingBox(xCoord, yCoord, zCoord);
        }
    }
}
