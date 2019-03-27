using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenerativeToolkit.Graphs.Core;

namespace GenerativeToolkit.Graphs.Geometry
{
    /// <summary>
    /// gPolygon class to hold graph´s polygon information in relation to its function on the graph
    /// like if it is internal or limit boundary.
    /// </summary>
    public class gPolygon : gBase, ICloneable
    {
        #region Internal Variables

        /// <summary>
        /// Polygon's id
        /// </summary>
        internal int id { get; set; }

        /// <summary>
        /// Flag to check polygons role: Internal or Boundary
        /// </summary>
        internal bool isBoundary { get; set; }

        /// <summary>
        /// Polygon's edges
        /// </summary>
        internal List<gEdge> edges = new List<gEdge>();

        /// <summary>
        /// Polygon's Vertices
        /// </summary>
        internal List<gVertex> vertices = new List<gVertex>();
        #endregion

        #region Public Variables
        /// <summary>
        /// gPolygon's vertices
        /// </summary>
        public List<gVertex> Vertices
        {
            get { return vertices; }
        }

        /// <summary>
        /// gPolygon's edges
        /// </summary>
        public List<gEdge> Edges
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
        internal gPolygon() { }

        internal gPolygon(int _id, bool _isExternal)
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
        public static gPolygon ByVertices(List<gVertex> vertices, bool isExternal = false)
        {
            gPolygon polygon = new gPolygon(-1, isExternal);
            polygon.vertices = vertices;
            int vertexCount = vertices.Count;
            for (var j = 0; j < vertexCount; j++)
            {
                int next_index = (j + 1) % vertexCount;
                gVertex vertex = vertices[j];
                gVertex next_vertex = vertices[next_index];
                polygon.edges.Add( new gEdge(vertex, next_vertex));
            }
            return polygon;
        }

        public static gPolygon ByCenterRadiusAndSides(gVertex center, double radius, int sides)
        {
            // TODO: create polygon by plane?
            if(sides < 3) { throw new ArgumentOutOfRangeException("sides", "Any polygon must have at least 3 sides."); }
            List<gVertex> vertices = new List<gVertex>();
            double angle = (Math.PI * 2) / sides;
            for(var i = 0; i < sides; i++)
            {
                var vertex = gVertex.ByCoordinates(
                        (Math.Sin(i * angle) * radius) + center.X,
                        (Math.Cos(i * angle) * radius) + center.Y,
                        center.Z
                        );
                vertices.Add(vertex);
            }
            return gPolygon.ByVertices(vertices);
        }
        #endregion

        #region Internal Methods
        internal void AddVertex(gVertex vertex)
        {
            vertex.polygonId = this.id;
            vertices.Add(vertex);
        }

        internal gPolygon AddVertex(gVertex v, gEdge intersectingEdge)
        {
            //Assumes that vertex v intersects one of polygons edges.
            gPolygon newPolygon = (gPolygon)this.Clone();

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
                newPolygon.edges.Add(new gEdge(newPolygon.vertices[i], newPolygon.vertices[nextIndex]));
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
        internal double IsLeft(gEdge edge, gVertex vertex)
        {
            var left = (edge.EndVertex.X - edge.StartVertex.X) * (vertex.Y - edge.StartVertex.Y) - (edge.EndVertex.Y - edge.StartVertex.Y) * (vertex.X - edge.StartVertex.X);
            return left;
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Determines if a gVertex is inside the gPolygon using Fast Winding Number method
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public bool ContainsVertex(gVertex vertex)
        {
            // http://geomalgorithms.com/a03-_inclusion.html
            gVertex maxVertex = vertices.OrderByDescending(v => v.DistanceTo(vertex)).First();
            double maxDistance = vertex.DistanceTo(maxVertex) * 1.5;
            gVertex v2 = gVertex.ByCoordinates(vertex.X + maxDistance, vertex.Y, vertex.Z);
            gEdge ray = gEdge.ByStartVertexEndVertex(vertex, v2);
            int windNumber = 0;
            foreach (gEdge edge in edges)
            {
                if(vertex.OnEdge(edge)) { return true; }
                gVertex intersection = ray.Intersection(edge) as gVertex;
                if (intersection is gVertex)
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
        public bool ContainsEdge(gEdge edge)
        {
            // TODO: Check if edge intersects polygon in vertices different than start/end.
            return this.ContainsVertex(edge.StartVertex)
                && this.ContainsVertex(edge.EndVertex)
                && this.ContainsVertex(gVertex.MidVertex(edge.StartVertex, edge.EndVertex));
        }
        /// <summary>
        /// Checks if a polygon is planar
        /// </summary>
        /// <param name="polygon">gPolygon</param>
        /// <returns>boolean</returns>
        public static bool IsPlanar(gPolygon polygon)
        {
            return gVertex.Coplanar(polygon.Vertices);
        }

        /// <summary>
        /// Checks if two gPolygons are coplanar.
        /// </summary>
        /// <param name="polygon">gPolygon</param>
        /// <param name="otherPolygon">Other gPolygon</param>
        /// <returns></returns>
        public static bool Coplanar(gPolygon polygon, gPolygon otherPolygon)
        {
            List<gVertex> joinedVertices = new List<gVertex>(polygon.Vertices);
            joinedVertices.AddRange(otherPolygon.Vertices);

            return gVertex.Coplanar(joinedVertices);
        }

        /// <summary>
        /// Determines if two polygons are intersecting
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public bool Intersects(gPolygon polygon)
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
        public List<gPolygon> Union(gPolygon clip)
        {
            
            var swLine = new SweepLine(this, clip, SweepLineType.Boolean);

            return swLine.ComputeBooleanOperation(BooleanType.Union);
        }

        public static List<gPolygon> Union(List<gPolygon> subjects, List<gPolygon> clips)
        {
            List<gPolygon> result = new List<gPolygon>(subjects);
            int count = 0;
            foreach (gPolygon clip in clips)
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
        public List<gPolygon> Difference(gPolygon clip)
        {
            var swLine = new SweepLine(this, clip, SweepLineType.Boolean);

            return swLine.ComputeBooleanOperation(BooleanType.Differenece);
        }

        public static List<gPolygon> Difference(List<gPolygon> subjects, List<gPolygon> clips)
        {
            List<gPolygon> result = new List<gPolygon>(subjects);
            int count = 0;
            foreach (gPolygon clip in clips)
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
        public List<gPolygon> Intersection(gPolygon clip)
        {
            var swLine = new SweepLine(this, clip, SweepLineType.Boolean);

            return swLine.ComputeBooleanOperation(BooleanType.Intersection);
        }

        public static List<gPolygon> Intersection(List<gPolygon> subjects, List<gPolygon> clips)
        {
            List<gPolygon> result = new List<gPolygon>(subjects);
            int count = 0;
            foreach (gPolygon clip in clips)
            {
                for (var i = count; i < result.Count; i++)
                {
                    result.AddRange(result[i].Intersection(clip));
                    count++;
                }
            }
            return result;
        }

        #endregion

        /// <summary>
        /// Clone method for gPolygon
        /// </summary>
        /// <returns>Cloned gPolygon</returns>
        public object Clone()
        {
            gPolygon newPolygon = new gPolygon(this.id, this.isBoundary);
            newPolygon.edges = new List<gEdge>(this.edges);
            newPolygon.vertices = new List<gVertex>(this.vertices);
            return newPolygon;
        }

        internal override gBoundingBox ComputeBoundingBox()
        {
            var xCoord = new List<double>(this.vertices.Count);
            var yCoord = new List<double>(this.vertices.Count);
            var zCoord = new List<double>(this.vertices.Count);
            foreach(gVertex v in vertices)
            {
                xCoord.Add(v.X);
                yCoord.Add(v.Y);
                zCoord.Add(v.Z);
            }
            return new gBoundingBox(xCoord, yCoord, zCoord);
        }
    }
}
