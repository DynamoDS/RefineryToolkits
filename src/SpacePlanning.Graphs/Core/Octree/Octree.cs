using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using BoundingBox = Autodesk.DesignScript.Geometry.BoundingBox;
using MeshToolkit = Autodesk.Dynamo.MeshToolkit.Mesh;

namespace GenerativeToolkit.Graphs.Core.Octree
{
    //  public static float OCTANT_POS[][] =
    //  {
    //    //i&4, i&2, i&1
    //    { 0, 0, 0 },    // [0] - 000
    //    { 0, 0, 1 },    // [1] - 001
    //    { 0, 1, 0 },    // [2] - 010
    //    { 0, 1, 1 },    // [3] - 011
    //    
    //    { 1, 0, 0 },    // [4] - 100
    //    { 1, 0, 1 },    // [5] - 101
    //    { 1, 1, 0 },    // [6] - 110
    //    { 1, 1, 1 },    // [7] - 111
    //  };

    public class Octree
    {
        #region Constants
        const float MIN_DEPTH_FILL_RATIO = 1.5f;
        const int MAX_DEPTH = 10; 
        #endregion

        #region Internal Variables
        internal OctreeNode _root;
        internal MeshToolkit _mesh;
        internal bool _cubic;
        internal IList<Point> vertices;
        internal List<List<int>> vertexIndexByTri;
        internal int MaxDepthReached { get; private set; }
        #endregion

        #region Public Variables

        public OctreeNode Root => _root;
        public MeshToolkit Mesh => _mesh;

        #endregion

        public Octree(MeshToolkit mesh, bool cubic = true)
        {
            this._mesh = mesh;
            this._cubic = cubic;
            //this.triangles = _mesh.Triangles();
            this.vertices = _mesh.Vertices();
            this.vertexIndexByTri = Core.List.Chop<int>(_mesh.VertexIndicesByTri(), 3);
            BoundingBox bbox = Graphical.Geometry.MeshToolkit.BoundingBox(this._mesh);

            // Extending the BoundingBox to be cubical from the centre.
            // Done by getting the max component of the Bounding box and translating min and max points outwards.
            // https://github.com/diwi/Space_Partitioning_Octree_BVH/blob/b7f66fe04e4af3b98ab9404363ab33f5dc1628a9/SpacePartitioning/src/DwOctree/Octree.java#L83
            if (cubic)
            {
                using (Point center = Geometry.Point.MidPoint(bbox.MinPoint, bbox.MaxPoint))
                {
                    Vector bboxSize = Vector.ByTwoPoints(bbox.MinPoint, bbox.MaxPoint);
                    Vector halfSize = bboxSize.Scale(0.5);
                    double[] halfComponents = new double[3] { halfSize.X, halfSize.Y, halfSize.Z };
                    double maxComponent = halfComponents[getSubdivisionPlane(bbox)];
                    Vector expansionDirection = Vector.ByCoordinates(maxComponent, maxComponent, maxComponent);
                    bbox = BoundingBox.ByCorners(
                        (Point)center.Translate(-maxComponent, -maxComponent, -maxComponent),
                        (Point)center.Translate(maxComponent, maxComponent, maxComponent)
                    );
                }
            }

            _root = new OctreeNode(0, bbox);
        }


        #region Internal Methods

        internal int getSubdivisionPlane(BoundingBox bbox)
        {
            using (Vector v = Vector.ByTwoPoints(bbox.MinPoint, bbox.MaxPoint))
            {
                var s = new double[3] { v.X, v.Y, v.Z };
                double max = s.Max();

                if(max == s[0]) { return 0; }
                else if(max == s[1]) { return 1; }
                else { return 2; }
            }
        }

        internal bool StoreAtFirstFit(OctreeNode ot, int idx)
        {
            // 1 - If max depth reached, save triangle an return.
            if(ot.depth >= MAX_DEPTH)
            {
                SaveTriangleToNode(ot, idx);
                return true;
            }

            // 2 - Generate children, if not possible this node is a leaf, so save the item here.
            if(!AssureChildren(ot, MAX_DEPTH))
            {
                SaveTriangleToNode(ot, idx);
                return true;
            }

            // 3 - Check if one child fully contains the triangle, If so, step down to the child
            foreach(OctreeNode child in ot.children)
            {
                if(TriangleFullyInside(child, idx))
                {
                    if(StoreAtFirstFit(child, idx)) { return true; }
                }
            }

            // 4 - No child fully contains the triangle, so push it to the leaves
            foreach(OctreeNode child in ot.children)
            {
                StoreInLeaves(child, idx);
            }

            return true;
        }

        internal bool AssureChildren(OctreeNode ot, int maxDepth)
        {
            if(ot.depth >= maxDepth) { return false; }
            if (ot.isLeaf())
            {
                ot.children = new OctreeNode[8];
                // Half vector size, by scaling total diagonal or maybe faster creating the MidPoint?
                Vector halfSize = Vector.ByTwoPoints(ot.bbox.MinPoint, ot.bbox.MaxPoint).Scale(0.5);
                int childDepth = ot.depth + 1;
                for(int i = 0; i < ot.children.Count(); i++)
                {
                    Point min = Point.ByCoordinates(
                        ot.bbox.MinPoint.X + (((i&4) > 0) ? halfSize.X : 0),
                        ot.bbox.MinPoint.Y + (((i&2) > 0) ? halfSize.Y : 0),
                        ot.bbox.MinPoint.Z + (((i&1) > 0) ? halfSize.Z : 0)
                    );
                    Point max = (Point)min.Translate(halfSize);

                    ot.children[i] = new OctreeNode(childDepth, BoundingBox.ByCorners(min, max));
                }
            }
            return true;
        }

        internal bool SaveTriangleToNode(OctreeNode ot, int idx)
        {
            if (!ot.triangle_Idx.Contains(idx))
            {
                ot.triangle_Idx.Add(idx);
            }
            return true;
        }

        internal bool TriangleFullyInside(OctreeNode ot, int triangleIndex)
        {
            List<int> triangleVertexIndex = this.vertexIndexByTri[triangleIndex];

            foreach(int index in triangleVertexIndex)
            {
                Point vertex = this.vertices[index];
                //bool inside =
                //    (ot.bbox.MinPoint.X <= vertex.X && ot.bbox.MinPoint.Y <= vertex.Y && ot.bbox.MinPoint.Z <= vertex.Z) &&
                //    (ot.bbox.MaxPoint.X >= vertex.X && ot.bbox.MaxPoint.Y >= vertex.Y && ot.bbox.MaxPoint.Z >= vertex.Z);
                if (!ot.bbox.Contains(vertex)) { return false; }
            }
            return true;
        }

        internal void StoreInLeaves(OctreeNode ot, int idx)
        {
            // If there is no intersection between the current node and the triangle, return
            if(!IntersectsWithTriangle(ot, idx)) { return; }

            // If current node is a leaf and overlaps intersects with the triangle, save it here
            if (ot.isLeaf())
            {
                SaveTriangleToNode(ot, idx);
                return;
            }

            // If the current node is not a leaf, step down to the children
            foreach(OctreeNode child in ot.children)
            {
                StoreInLeaves(child, idx);
            }
        }

        internal bool IntersectsWithTriangle(OctreeNode ot, int triangleIndex)
        {
            List<Point> vertices = vertexIndexByTri[triangleIndex].Select(v => this.vertices[v]).ToList();
            using(Polygon pol = Polygon.ByPoints(vertices))
            {
                if (!ot.bbox.Intersects(BoundingBox.ByGeometry(pol)))
                {
                    return false;
                }

                using (Cuboid cube = ot.bbox.ToCuboid())
                {
                    return cube.DoesIntersect(pol);
                }
            }
            
        }

        internal void PushToLeaves(OctreeNode ot)
        {
            if (ot.isLeaf()) { return; }

            //Current node is not leaf, if it is not empty move its items down to its children
            if (!ot.isEmpty())
            {
                foreach(int idx in ot.triangle_Idx)
                {
                    foreach(OctreeNode child in ot.children)
                    {
                        StoreInLeaves(child, idx);
                    }
                }
                ot.triangle_Idx.Clear();
            }

            // Repeat for all children
            foreach(OctreeNode child in ot.children)
            {
                PushToLeaves(child);
            }
        }

        internal void OptimizeSpaceCost(OctreeNode ot)
        {
            if (!ot.isEmpty())
            {
                if (!PositiveFillRatio(ot))
                {
                    AssureChildren(ot, MAX_DEPTH);
                    PushToLeaves(ot);
                }
            }

            if (ot.isLeaf()) { return; }

            foreach(OctreeNode child in ot.children)
            {
                OptimizeSpaceCost(child);
            }
        }

        internal bool PositiveFillRatio(OctreeNode ot)
        {
            double ratio_items_depth = ot.itemCount() / ot.depth;
            return ratio_items_depth < MIN_DEPTH_FILL_RATIO;
        }

        internal void EvenToMaxDepthReached(OctreeNode ot, int maxDepthReached)
        {
            if (!ot.isEmpty())
            {
                if (ot.depth < maxDepthReached)
                {
                    AssureChildren(ot, MAX_DEPTH);
                    PushToLeaves(ot);
                }
            }

            if (ot.isLeaf()) { return; }

            foreach (OctreeNode child in ot.children)
            {
                EvenToMaxDepthReached(child, maxDepthReached);
            }
        }

        internal OctreeNode ClosestNodeRecursive(OctreeNode ot, Autodesk.DesignScript.Geometry.Geometry geom)
        {
            if (ot.isLeaf()) { return ot; }
            double minDist = -10;
            OctreeNode closestNode = null;
            foreach(OctreeNode child in ot.children)
            {
                if (!ot.isValid()) { continue; }
                double childDist = child.DistanceTo(geom);
                if(minDist < 0 || childDist < minDist)
                {
                    minDist = childDist;
                    closestNode = child;
                }
            }

            return ClosestNodeRecursive(closestNode, geom);
        }
        #endregion

        #region Public Method

        public List<OctreeNode> GetNodes()
        {
            return _root.getNodes_recursive();
        }

        [MultiReturn(new[] { "octree", "print" })]
        public Dictionary<string, object> BuildOctree()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("> Start Building Octree:");
            Octree newOctree = new Octree(this._mesh, this._cubic);

            Stopwatch sw = new Stopwatch();
            
            try
            {
                sw.Start();
                for (int i = 0; i < newOctree._mesh.TriangleCount; i++)
                {
                    StoreAtFirstFit(newOctree._root, i);
                }

                sb.Append("\n\t1) StoreAtFirstFit(" + String.Format("{0} ms", sw.ElapsedMilliseconds) + "), stored items: " + newOctree.GetNodes().Count);

                sw.Restart();

                PushToLeaves(newOctree._root);
                sb.Append("\n\t2) PushToLeaves(" + String.Format("{0} ms", sw.ElapsedMilliseconds) + "), stored items: " + newOctree.GetNodes().Count);

                sw.Restart();

                newOctree.MaxDepthReached = newOctree.GetNodes().Select(node => node.depth).Max();
                //EvenToMaxDepthReached(newOctree._root, newOctree.MaxDepthReached);
                OptimizeSpaceCost(newOctree._root);

                sb.Append("\n\t3) OptimizeSpaceCost(" + String.Format("{0} ms", sw.ElapsedMilliseconds) + "), stored items: " + newOctree.GetNodes().Count);

                sw.Stop();

            }
            catch(Exception e)
            {
                throw new Exception(e.StackTrace);
            }
            

            return new Dictionary<string, object>()
            {
                {"octree", newOctree },
                {"print", sb.ToString() }
            };

        }

        public OctreeNode ClosestNode(Autodesk.DesignScript.Geometry.Geometry geometry)
        {
            return ClosestNodeRecursive(this._root, geometry);
        }

        #endregion

    }
}
