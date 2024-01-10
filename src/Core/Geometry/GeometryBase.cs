/***************************************************************************************
* This code is originally created by Alvaro Alvaro Ortega Pickmans, and is available
* in his Graphical Packages
* Title: Graphical
* Author: Alvaro Ortega Pickmans
* Date: 2017
* Availability: https://github.com/alvpickmans/Graphical
*
***************************************************************************************/

namespace Autodesk.RefineryToolkits.Core.Geometry
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

        private BoundingBox boundingBox;

        /// <summary>
        /// Geometry's Axis Aligned Bounding Box
        /// </summary>
        public BoundingBox BoundingBox
        {
            get
            {
                boundingBox ??= ComputeBoundingBox();
                return boundingBox;
            }
        }

        internal abstract BoundingBox ComputeBoundingBox();

    }
}
