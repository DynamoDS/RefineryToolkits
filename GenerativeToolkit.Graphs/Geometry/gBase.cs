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
    public abstract class gBase
    {

        #region Properties
        //internal double? thresholdOverride { get; private set; }
        //internal int thresholdDecimals { get; private set; }
        #endregion

        private gBoundingBox boundingBox;

        /// <summary>
        /// Geometry's Axis Aligned Bounding Box
        /// </summary>
        public gBoundingBox BoundingBox
        {
            get
            {
                if(boundingBox == null) { boundingBox = ComputeBoundingBox(); }
                return boundingBox;
            }
        }

        internal abstract gBoundingBox ComputeBoundingBox();

    }
}
