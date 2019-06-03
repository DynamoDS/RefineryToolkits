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
                if(boundingBox == null) { boundingBox = ComputeBoundingBox(); }
                return boundingBox;
            }
        }

        internal abstract BoundingBox ComputeBoundingBox();

    }
}
