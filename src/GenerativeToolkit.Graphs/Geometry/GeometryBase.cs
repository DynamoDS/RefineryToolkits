using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeToolkit.Graphs.Geometry
{
    /// <summary>
    /// Base abstract class for all spatial geometries
    /// </summary>
    public abstract class GeometryBase
    {

        #region Properties
        //internal double? thresholdOverride { get; private set; }
        //internal int thresholdDecimals { get; private set; }
        #endregion

        private GeometryBoundingBox boundingBox;

        /// <summary>
        /// Geometry's Axis Aligned Bounding Box
        /// </summary>
        public GeometryBoundingBox BoundingBox
        {
            get
            {
                if(boundingBox == null) { boundingBox = ComputeBoundingBox(); }
                return boundingBox;
            }
        }

        internal abstract GeometryBoundingBox ComputeBoundingBox();

    }
}
