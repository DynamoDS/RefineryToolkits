using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeToolkit.Graphs.Geometry
{
    /// <summary>
    /// gPolygon class to hold graph´s polygon information in relation to its function on the graph
    /// like if it is internal or limit boundary.
    /// </summary>
    public class GeometryPolygon : GeometryBase, ICloneable
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
        internal List<GeometryEdge> edges = new List<GeometryEdge>();

        /// <summary>
        /// Polygon's Vertices
        /// </summary>
        internal List<GeometryVertex> vertices = new List<GeometryVertex>();
        #endregion

        #region Public Variables
        /// <summary>
        /// gPolygon's vertices
        /// </summary>
        public List<GeometryVertex> Vertices
        {
            get { return vertices; }
        }

        /// <summary>
        /// gPolygon's edges
        /// </summary>
        public List<GeometryEdge> Edges
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
        internal GeometryPolygon() { }

        internal GeometryPolygon(int _id, bool _isExternal)
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
        public static GeometryPolygon ByVertices(List<GeometryVertex> vertices, bool isExternal = false)
        {
            GeometryPolygon polygon = new GeometryPolygon(-1, isExternal);
            polygon.vertices = vertices;
            int vertexCount = vertices.Count;
            for (var j = 0; j < vertexCount; j++)
            {
                int next_index = (j + 1) % vertexCount;
                GeometryVertex vertex = vertices[j];
                GeometryVertex next_vertex = vertices[next_index];
                polygon.edges.Add( new GeometryEdge(vertex, next_vertex));
            }
            return polygon;
        }

        public static GeometryPolygon ByCenterRadiusAndSides(GeometryVertex center, double radius, int sides)
        {
            // TODO: create polygon by plane?
            if(sides < 3) { throw new ArgumentOutOfRangeException("sides", "Any polygon must have at least 3 sides."); }
            List<GeometryVertex> vertices = new List<GeometryVertex>();
            double angle = (Math.PI * 2) / sides;
            for(var i = 0; i < sides; i++)
            {
                var vertex = GeometryVertex.ByCoordinates(
                        (Math.Sin(i * angle) * radius) + center.X,
                        (Math.Cos(i * angle) * radius) + center.Y,
                        center.Z
                        );
                vertices.Add(vertex);
            }
            return GeometryPolygon.ByVertices(vertices);
        }
        #endregion

        #region Internal Methods
        internal void AddVertex(GeometryVertex vertex)
        {
            vertex.polygonId = this.id;
            vertices.Add(vertex);
        }

        internal GeometryPolygon AddVertex(GeometryVertex v, GeometryEdge intersectingEdge)
        {
            //Assumes that vertex v intersects one of polygons edges.
            GeometryPolygon newPolygon = (GeometryPolygon)this.Clone();

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
                newPolygon.edges.Add(new GeometryEdge(newPolygon.vertices[i], newPolygon.vertices[nextIndex]));
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
        internal double IsLeft(GeometryEdge edge, GeometryVertex vertex)
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
        public bool ContainsVertex(GeometryVertex vertex)
        {
            // http://geomalgorithms.com/a03-_inclusion.html
            GeometryVertex maxVertex = vertices.OrderByDescending(v => v.DistanceTo(vertex)).First();
            double maxDistance = vertex.DistanceTo(maxVertex) * 1.5;
            GeometryVertex v2 = GeometryVertex.ByCoordinates(vertex.X + maxDistance, vertex.Y, vertex.Z);
            GeometryEdge ray = GeometryEdge.ByStartVertexEndVertex(vertex, v2);
            int windNumber = 0;
            foreach (GeometryEdge edge in edges)
            {
                if(vertex.OnEdge(edge)) { return true; }
                GeometryVertex intersection = ray.Intersection(edge) as GeometryVertex;
                if (intersection is GeometryVertex)
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
        public bool ContainsEdge(GeometryEdge edge)
        {
            // TODO: Check if edge intersects polygon in vertices different than start/end.
            return this.ContainsVertex(edge.StartVertex)
                && this.ContainsVertex(edge.EndVertex)
                && this.ContainsVertex(GeometryVertex.MidVertex(edge.StartVertex, edge.EndVertex));
        }
        /// <summary>
        /// Checks if a polygon is planar
        /// </summary>
        /// <param name="polygon">gPolygon</param>
        /// <returns>boolean</returns>
        public static bool IsPlanar(GeometryPolygon polygon)
        {
            return GeometryVertex.Coplanar(polygon.Vertices);
        }

        /// <summary>
        /// Checks if two gPolygons are coplanar.
        /// </summary>
        /// <param name="polygon">gPolygon</param>
        /// <param name="otherPolygon">Other gPolygon</param>
        /// <returns></returns>
        public static bool Coplanar(GeometryPolygon polygon, GeometryPolygon otherPolygon)
        {
            List<GeometryVertex> joinedVertices = new List<GeometryVertex>(polygon.Vertices);
            joinedVertices.AddRange(otherPolygon.Vertices);

            return GeometryVertex.Coplanar(joinedVertices);
        }
        /*

        /// <summary>
        /// Determines if two polygons are intersecting
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public bool Intersects(GeometryPolygon polygon)
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
        public List<GeometryPolygon> Union(GeometryPolygon clip)
        {
            
            var swLine = new SweepLine(this, clip, SweepLineType.Boolean);

            return swLine.ComputeBooleanOperation(BooleanType.Union);
        }

        public static List<GeometryPolygon> Union(List<GeometryPolygon> subjects, List<GeometryPolygon> clips)
        {
            List<GeometryPolygon> result = new List<GeometryPolygon>(subjects);
            int count = 0;
            foreach (GeometryPolygon clip in clips)
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
        public List<GeometryPolygon> Difference(GeometryPolygon clip)
        {
            var swLine = new SweepLine(this, clip, SweepLineType.Boolean);

            return swLine.ComputeBooleanOperation(BooleanType.Differenece);
        }

        public static List<GeometryPolygon> Difference(List<GeometryPolygon> subjects, List<GeometryPolygon> clips)
        {
            List<GeometryPolygon> result = new List<GeometryPolygon>(subjects);
            int count = 0;
            foreach (GeometryPolygon clip in clips)
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
        public List<GeometryPolygon> Intersection(GeometryPolygon clip)
        {
            var swLine = new SweepLine(this, clip, SweepLineType.Boolean);

            return swLine.ComputeBooleanOperation(BooleanType.Intersection);
        }

        public static List<GeometryPolygon> Intersection(List<GeometryPolygon> subjects, List<GeometryPolygon> clips)
        {
            List<GeometryPolygon> result = new List<GeometryPolygon>(subjects);
            int count = 0;
            foreach (GeometryPolygon clip in clips)
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
            GeometryPolygon newPolygon = new GeometryPolygon(this.id, this.isBoundary);
            newPolygon.edges = new List<GeometryEdge>(this.edges);
            newPolygon.vertices = new List<GeometryVertex>(this.vertices);
            return newPolygon;
        }

        internal override GeometryBoundingBox ComputeBoundingBox()
        {
            var xCoord = new List<double>(this.vertices.Count);
            var yCoord = new List<double>(this.vertices.Count);
            var zCoord = new List<double>(this.vertices.Count);
            foreach(GeometryVertex v in vertices)
            {
                xCoord.Add(v.X);
                yCoord.Add(v.Y);
                zCoord.Add(v.Z);
            }
            return new GeometryBoundingBox(xCoord, yCoord, zCoord);
        }
    }
}
