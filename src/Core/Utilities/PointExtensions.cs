using Autodesk.DesignScript.Runtime;
using Autodesk.RefineryToolkits.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSGeom = Autodesk.DesignScript.Geometry;

namespace Autodesk.RefineryToolkits.Core.Utillites
{
    [IsVisibleInDynamoLibrary(false)]
    public static class PointExtension
    {
        /// <summary>
        /// Checks if a list of points are co-linear, meaning they can all sit on the same line.
        /// </summary>
        /// <param name="pointList">The list of points to check.</param>
        /// <search>colinear</search>
        public static bool AreColinear(this List<DSGeom.Point> pointList)
        {
            if (pointList == null)
                throw new ArgumentNullException(nameof(pointList));

            int n = pointList.Count;
            if (n < 3) return true;

            int startIndex = 1;
            var referenceVector = Vector.ByTwoVertices(pointList[0].ToVertex(), pointList[1].ToVertex());

            for (var i = startIndex; i < n-1; i++)
            {
                var currentVector = Vector.ByTwoVertices(pointList[i].ToVertex(), pointList[i+1].ToVertex());
                var crossProduct = referenceVector.Cross(currentVector);
                if (crossProduct.X != 0 || crossProduct.Y != 0 || crossProduct.Z != 0) return false;
            }

            return true;
        }

        /// <summary>
        /// Compare a point against another to see if it is the same
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="tolerance">number of decimal places to round to</param>
        /// <search></search>
        public static bool CompareCoincidental(this DSGeom.Point point1, DSGeom.Point point2, double tolerance = 8)
        {
            double pt1X = System.Math.Round(point1.X, 8);
            double pt1Y = System.Math.Round(point1.Y, 8);
            double pt1Z = System.Math.Round(point1.Z, 8);

            string pt1 = pt1X.ToString() + "," + pt1Y.ToString() + "," + pt1Z.ToString();

            double pt2X = System.Math.Round(point2.X, 8);
            double pt2Y = System.Math.Round(point2.Y, 8);
            double pt2Z = System.Math.Round(point2.Z, 8);

            string pt2 = pt2X.ToString() + "," + pt2Y.ToString() + "," + pt2Z.ToString();

            if (pt1 == pt2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Move a point or list of points along a curve based on a parameter witin a range
        /// </summary>
        /// <param name="point"></param>
        /// <param name="polycurve"></param>
        /// <param name="length"></param>
        /// <param name="param"></param>
        /// <search></search>
        public static dynamic MoveAlongCurve(this DSGeom.Point point, DSGeom.PolyCurve polycurve, double length, double param)
        {
            if (length > polycurve.Length)
            {
                return "Select a segment length smaller than the length of the polycurve.";
            }
            else
            {
                double absLength = DSCore.Math.Abs(length);
                double ptParam = polycurve.ParameterAtPoint(point);
                double dist = polycurve.SegmentLengthAtParameter(ptParam);

                double minusDist = dist - absLength;
                double maxDist = dist + absLength;

                if (minusDist < 0)
                {
                    minusDist = 0;
                }
                if (maxDist > polycurve.Length)
                {
                    maxDist = polycurve.Length;
                }

                Autodesk.DesignScript.Geometry.Point minusPt = polycurve.PointAtSegmentLength(minusDist);
                Autodesk.DesignScript.Geometry.Point maxPt = polycurve.PointAtSegmentLength(maxDist);

                double number = DSCore.Math.MapTo(0, 1, param, minusDist, maxDist);
                Autodesk.DesignScript.Geometry.Point newPoint = polycurve.PointAtSegmentLength(number);
                return newPoint;

            }
        }

        /// <summary>
        /// Randomly move a point or list of points along a curve within a given percentage range.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="polycurve"></param>
        /// <param name="percentage"></param>
        /// <search></search>
        public static dynamic RandomlyMoveAlongCurve(this DSGeom.Point point, DSGeom.PolyCurve polycurve, double percentage)
        {
            if (percentage > 100 || percentage < 0)
            {
                return "Select a percentage number between 0-100.";
            }
            else
            {
                double param = polycurve.ParameterAtPoint(point);
                double start = param - (percentage / 100);
                double end = param + (percentage / 100);

                if (start < 0)
                {
                    start = 0;
                }

                if (end > 1)
                {
                    end = 1;
                }

                double i;
                List<double> range = new List<double>();
                for (i = start; i <= end; i += 0.01)
                    range.Add(i);

                var random = new Random();
                int index = random.Next(range.Count);

                double item = range[index];

                Autodesk.DesignScript.Geometry.Point newPoint = polycurve.PointAtParameter(item);
                return newPoint;

            }
        }
    }
}
