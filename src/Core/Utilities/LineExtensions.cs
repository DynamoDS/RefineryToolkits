using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static Autodesk.DesignScript.Geometry.Line ExtendAtBothEnds(this Autodesk.DesignScript.Geometry.Curve line, double distance)
        {
            Autodesk.DesignScript.Geometry.Point stPt = line.StartPoint;
            Autodesk.DesignScript.Geometry.Point endPt = line.EndPoint;

            Autodesk.DesignScript.Geometry.Vector vec1 = Autodesk.DesignScript.Geometry.Vector.ByTwoPoints(endPt, stPt);
            Autodesk.DesignScript.Geometry.Vector vec2 = Autodesk.DesignScript.Geometry.Vector.ByTwoPoints(stPt, endPt);

            Autodesk.DesignScript.Geometry.Point newStPt = stPt.Translate(vec1, distance) as Autodesk.DesignScript.Geometry.Point;
            Autodesk.DesignScript.Geometry.Point newEndPt = endPt.Translate(vec2, distance) as Autodesk.DesignScript.Geometry.Point;

            Autodesk.DesignScript.Geometry.Line newLine = Autodesk.DesignScript.Geometry.Line.ByStartPointEndPoint(newStPt, newEndPt);

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
