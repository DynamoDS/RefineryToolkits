using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodesk.RefineryToolkits.Core.Utillites
{
    [IsVisibleInDynamoLibrary(false)]
    public static class VectorExtension
    {

        #region ByTwoCurves
        /// <summary>
        /// Creates a vector between the midpoint of two curves
        /// </summary>
        /// <param name="crv1"></param>
        /// <param name="crv2"></param>
        /// <search></search>
        public static Autodesk.DesignScript.Geometry.Vector ByTwoCurves(this Autodesk.DesignScript.Geometry.Curve crv1, Autodesk.DesignScript.Geometry.Curve crv2)
        {
            Autodesk.DesignScript.Geometry.Point pt1 = crv1.PointAtParameter(0.5);
            Autodesk.DesignScript.Geometry.Point pt2 = crv2.PointAtParameter(0.5);

            Autodesk.DesignScript.Geometry.Vector vec = Autodesk.DesignScript.Geometry.Vector.ByTwoPoints(pt1, pt2);

            pt1.Dispose();
            pt2.Dispose();

            return vec;

        }
        #endregion

    }
}
