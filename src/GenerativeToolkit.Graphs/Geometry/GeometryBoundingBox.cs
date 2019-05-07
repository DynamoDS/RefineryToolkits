using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenerativeToolkit.Graphs.Extensions;

namespace GenerativeToolkit.Graphs.Geometry
{
    /// <summary>
    /// Axis Aligned Bounding Box
    /// </summary>
    public class GeometryBoundingBox
    {
        #region Private Properties
        private double[] min = new double[3];
        private double[] max = new double[3];
        #endregion

        #region Public Properties
        /// <summary>
        /// Bounding Box's minimum vertex
        /// </summary>
        public GeometryVertex MinVertex
        {
            get { return GeometryVertex.ByCoordinatesArray(min); }
        }

        /// <summary>
        /// Bounding Box's maximum vertex
        /// </summary>
        public GeometryVertex MaxVertex
        {
            get { return GeometryVertex.ByCoordinatesArray(max); }
        }
        #endregion

        #region Private Constructor
        internal GeometryBoundingBox(IEnumerable<double> xCoordinates, IEnumerable<double> yCoordinates, IEnumerable<double> zCoordinates)
        {
            this.min = new double[3] { xCoordinates.Min(), yCoordinates.Min(), zCoordinates.Min() };
            this.max = new double[3] { xCoordinates.Max(), yCoordinates.Max(), zCoordinates.Max() };
        }
        #endregion

        #region Public Constructors
        /// <summary>
        /// Creates a new Bounding Box from a minimum and maximum vertices
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static GeometryBoundingBox ByMinVertexMaxVertex(GeometryVertex min, GeometryVertex max)
        {
            return new GeometryBoundingBox(new double[2] { min.X, max.X}, new double[2] { min.Y, max.Y }, new double[2] { min.Z, max.Z });
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Determines if two Axis Aligned Bounding Boxes intersect
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Intersects(GeometryBoundingBox other)
        {
            return
                (min[0] <= other.max[0]) && (max[0] >= other.min[0]) &&
                (min[1] <= other.max[1]) && (max[1] >= other.min[1]) &&
                (min[2] <= other.max[2]) && (max[2] >= other.min[2]);
        } 
        #endregion

    }
}
