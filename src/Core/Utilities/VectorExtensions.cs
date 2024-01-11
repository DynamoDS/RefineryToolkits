using Autodesk.DesignScript.Runtime;

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
        public static DesignScript.Geometry.Vector ByTwoCurves(this DesignScript.Geometry.Curve crv1, DesignScript.Geometry.Curve crv2)
        {
            DesignScript.Geometry.Point pt1 = crv1.PointAtParameter(0.5);
            DesignScript.Geometry.Point pt2 = crv2.PointAtParameter(0.5);

            DesignScript.Geometry.Vector vec = DesignScript.Geometry.Vector.ByTwoPoints(pt1, pt2);

            pt1.Dispose();
            pt2.Dispose();

            return vec;

        }
        #endregion

    }
}
