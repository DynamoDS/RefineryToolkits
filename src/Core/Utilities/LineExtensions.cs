using Autodesk.DesignScript.Runtime;

namespace Autodesk.RefineryToolkits.Core.Utillites
{
    [IsVisibleInDynamoLibrary(false)]
    public static class LineExtension
    {

        #region ExtendAtBothEnds
        /// <summary>
        /// Extends a straight line at both ends
        /// </summary>
        /// <param name="line"></param>
        /// <param name="distance"></param>
        /// <search></search>
        public static DesignScript.Geometry.Line ExtendAtBothEnds(this DesignScript.Geometry.Curve line, double distance)
        {
            DesignScript.Geometry.Point stPt = line.StartPoint;
            DesignScript.Geometry.Point endPt = line.EndPoint;

            DesignScript.Geometry.Vector vec1 = DesignScript.Geometry.Vector.ByTwoPoints(endPt, stPt);
            DesignScript.Geometry.Vector vec2 = DesignScript.Geometry.Vector.ByTwoPoints(stPt, endPt);

            DesignScript.Geometry.Point newStPt = stPt.Translate(vec1, distance) as DesignScript.Geometry.Point;
            DesignScript.Geometry.Point newEndPt = endPt.Translate(vec2, distance) as DesignScript.Geometry.Point;

            DesignScript.Geometry.Line newLine = DesignScript.Geometry.Line.ByStartPointEndPoint(newStPt, newEndPt);

            //Dispose redundant geometry
            stPt.Dispose();
            endPt.Dispose();
            vec1.Dispose();
            vec2.Dispose();
            newStPt.Dispose();
            newEndPt.Dispose();

            return newLine;

        }
        #endregion

    }
}
