using System;
using System.Collections.Generic;
using System.Linq;
using DSCore;
using DSCore.Properties;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

namespace Autodesk.GenerativeToolkit.Utilities
{
    [IsVisibleInDynamoLibrary(false)]
    internal static class Curve
    {

        #region FindMatchingVectorCurves
        /// <summary>
        /// Compares a single curve to a list of curves to find any curves that have the same normalised vector.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="curves"></param>
        /// <search></search>
        public static List<Autodesk.DesignScript.Geometry.Curve> FindMatchingVectorCurves(this Autodesk.DesignScript.Geometry.Curve curve, List<Autodesk.DesignScript.Geometry.Curve> curves)
        {
            Autodesk.DesignScript.Geometry.Point stPt1 = curve.StartPoint;
            Autodesk.DesignScript.Geometry.Point endPt1 = curve.EndPoint;

            Autodesk.DesignScript.Geometry.Vector vec1 = Autodesk.DesignScript.Geometry.Vector.ByTwoPoints(stPt1, endPt1).Normalized();
            string str1 = vec1.ToString();

            List<string> curveList = new List<string>();
            foreach (var c in curves)
            {
                Autodesk.DesignScript.Geometry.Point stPt2 = c.StartPoint;
                Autodesk.DesignScript.Geometry.Point endPt2 = c.EndPoint;

                Autodesk.DesignScript.Geometry.Vector vec2 = Autodesk.DesignScript.Geometry.Vector.ByTwoPoints(stPt2, endPt2).Normalized();
                string str2 = vec2.ToString();
                curveList.Add(str2);

                //Dispose redundant geometry
                stPt2.Dispose();
                endPt2.Dispose();
                vec2.Dispose();
            }

            List<Autodesk.DesignScript.Geometry.Curve> matchingCurves = new List<Autodesk.DesignScript.Geometry.Curve>();
            for (int i = 0; i < curveList.Count; i++)
            {
                if (str1 == curveList[i])
                {
                    matchingCurves.Add(curves[i]);
                }
            }

            if (matchingCurves.Count == 0)
            {
                List<string> revCurveList = new List<string>();
                foreach (var c in curves)
                {
                    Autodesk.DesignScript.Geometry.Point stPt2 = c.StartPoint;
                    Autodesk.DesignScript.Geometry.Point endPt2 = c.EndPoint;

                    Autodesk.DesignScript.Geometry.Vector vec2 = Autodesk.DesignScript.Geometry.Vector.ByTwoPoints(endPt2, stPt2).Normalized();
                    string str2 = vec2.ToString();
                    revCurveList.Add(str2);

                    //Dispose redundant geometry
                    stPt2.Dispose();
                    endPt2.Dispose();
                    vec2.Dispose();
                }
                for (int i = 0; i < revCurveList.Count; i++)
                {
                    if (str1 == revCurveList[i])
                    {
                        matchingCurves.Add(curves[i]);
                    }
                }
            }

            //Dispose redundant geometry
            stPt1.Dispose();
            endPt1.Dispose();
            vec1.Dispose();

            return matchingCurves;
        }
        #endregion

        #region MaximumLength
        /// <summary>
        /// Return the maximum length curve from a list of curves and all the other curves
        /// </summary>
        /// <param name="curves"></param>
        /// <search></search>
        [MultiReturn(new[] { "maxCrv", "otherCrvs" })]
        public static Dictionary<string, dynamic> MaximumLength(this List<Autodesk.DesignScript.Geometry.Curve> curves)
        {

            List<double> lengths = new List<double>();
            for (int i = 0; i < curves.Count; i++)
            {
                lengths.Add(System.Math.Round(curves[i].Length, 8));
            }

            List<Autodesk.DesignScript.Geometry.Curve> otherCurves = new List<Autodesk.DesignScript.Geometry.Curve>();
            List<Autodesk.DesignScript.Geometry.Curve> maxCurve = new List<Autodesk.DesignScript.Geometry.Curve>();

            if (lengths.Any(o => o != lengths[0]))
            {
                List<int> maxID = new List<int>();
                for (int i = 0; i < lengths.Count; i++)
                {
                    if (lengths[i] == lengths.Max())
                    {
                        maxID.Add(i);
                    }
                    else
                    {
                        otherCurves.Add(curves[i]);
                    }
                }

                foreach (var id in maxID)
                {
                    maxCurve.Add(curves[id]);
                }
            }
            else
            {
                for (int i = 0; i < lengths.Count; i++)
                {
                    otherCurves.Add(curves[i]);
                }
            }

            Dictionary<string, dynamic> newOutput;
            newOutput = new Dictionary<string, dynamic>
            {
                {"maxCrv",maxCurve},
                {"otherCrvs",otherCurves}
            };

            return newOutput;
        }
        #endregion

    }
    [IsVisibleInDynamoLibrary(false)]
    internal static class Line
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
    [IsVisibleInDynamoLibrary(false)]
    internal static class Surface
    {

        #region BoundingSurface
        /// <summary>
        /// Return the bounding surface of the input surface
        /// </summary>
        /// <param name="surface"></param>
        /// <search></search>
        public static Autodesk.DesignScript.Geometry.Surface BoundingSurface(this Autodesk.DesignScript.Geometry.Surface surface)
        {
            BoundingBox bb = surface.BoundingBox;
            Cuboid c = bb.ToCuboid();
            Autodesk.DesignScript.Geometry.Surface srf = (c.Explode())[0] as Autodesk.DesignScript.Geometry.Surface;

            bb.Dispose();
            c.Dispose();

            return srf;
        }
        #endregion

        #region MaximumArea
        /// <summary>
        /// Return the maximum area surface from a list of surfaces and all the other surfaces
        /// </summary>
        /// <param name="surfaces"></param>
        /// <search></search>
        [MultiReturn(new[] { "maxSrf", "otherSrfs" })]
        public static Dictionary<string, dynamic> MaximumArea(this List<Autodesk.DesignScript.Geometry.Surface> surfaces)
        {
            List<double> areas = new List<double>();
            for (int i = 0; i < surfaces.Count; i++)
            {
                areas.Add(surfaces[i].Area);
            }


            List<Autodesk.DesignScript.Geometry.Surface> otherSurfaces = new List<Autodesk.DesignScript.Geometry.Surface>();
            List<int> maxID = new List<int>();
            for (int i = 0; i < areas.Count; i++)
            {
                if (areas[i] == areas.Max())
                {
                    maxID.Add(i);
                }
                else
                {
                    otherSurfaces.Add(surfaces[i]);
                }
            }

            Autodesk.DesignScript.Geometry.Surface maxSurface = surfaces[maxID[0]];

            Dictionary<string, dynamic> newOutput;
            newOutput = new Dictionary<string, dynamic>
            {
                {"maxSrf",maxSurface},
                {"otherSrfs",otherSurfaces}
            };

            return newOutput;
        }
        #endregion

        #region OffsetPerimeterCurves
        /// <summary>
        /// Creates a amentiy space on a given surface
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="offset"></param>
        /// <search></search>
        [MultiReturn(new[] { "insetCrvs", "outsetCrvs" })]
        public static Dictionary<string, Autodesk.DesignScript.Geometry.Curve[]> OffsetPerimeterCurves(this Autodesk.DesignScript.Geometry.Surface surface, double offset)
        {
            List<Autodesk.DesignScript.Geometry.Curve> srfPerimCrvs = surface.PerimeterCurves().ToList();

            Autodesk.DesignScript.Geometry.PolyCurve plyCrv = Autodesk.DesignScript.Geometry.PolyCurve.ByJoinedCurves(srfPerimCrvs);

            double inOffset;
            double outOffset;

            if (offset < 0)
            {
                inOffset = offset;
                outOffset = -offset;
            }
            else
            {
                inOffset = -offset;
                outOffset = offset;
            }

            Autodesk.DesignScript.Geometry.Curve[] inPerimCrvs;
            try
            {
                List<Autodesk.DesignScript.Geometry.Curve> inOffsetCrv = new List<Autodesk.DesignScript.Geometry.Curve>() { (plyCrv.Offset(inOffset)) };
                Autodesk.DesignScript.Geometry.PolyCurve inOffsetPolyCrv = Autodesk.DesignScript.Geometry.PolyCurve.ByJoinedCurves(inOffsetCrv);
                List<Autodesk.DesignScript.Geometry.Curve> inOffsetCrvList = inOffsetPolyCrv.Curves().ToList();
                List<Autodesk.DesignScript.Geometry.Point> inPts = new List<Autodesk.DesignScript.Geometry.Point>();
                foreach (Autodesk.DesignScript.Geometry.Curve c in inOffsetCrvList)
                {
                    inPts.Add(c.StartPoint);
                }
                Autodesk.DesignScript.Geometry.PolyCurve inOffsetPolyCrv2 = PolyCurve.ByPoints(inPts, true);
                Autodesk.DesignScript.Geometry.Surface inOffsetSrf = Autodesk.DesignScript.Geometry.Surface.ByPatch(inOffsetPolyCrv2);
                inPerimCrvs = inOffsetSrf.PerimeterCurves();
                inOffsetSrf.Dispose();
                inOffsetPolyCrv.Dispose();
                inOffsetPolyCrv2.Dispose();
            }
            catch (Exception)
            {
                inPerimCrvs = null;
            }

            Autodesk.DesignScript.Geometry.Curve[] outPerimCrvs;
            try
            {
                List<Autodesk.DesignScript.Geometry.Curve> outOffsetCrv = new List<Autodesk.DesignScript.Geometry.Curve>() { (plyCrv.Offset(outOffset)) };
                Autodesk.DesignScript.Geometry.PolyCurve outOffsetPolyCrv = Autodesk.DesignScript.Geometry.PolyCurve.ByJoinedCurves(outOffsetCrv);
                List<Autodesk.DesignScript.Geometry.Curve> outOffsetCrvList = outOffsetPolyCrv.Curves().ToList();
                List<Autodesk.DesignScript.Geometry.Point> outPts = new List<Autodesk.DesignScript.Geometry.Point>();
                foreach (Autodesk.DesignScript.Geometry.Curve c in outOffsetCrvList)
                {
                    outPts.Add(c.StartPoint);
                }
                Autodesk.DesignScript.Geometry.PolyCurve outOffsetPolyCrv2 = PolyCurve.ByPoints(outPts, true);
                Autodesk.DesignScript.Geometry.Surface outOffsetSrf = Autodesk.DesignScript.Geometry.Surface.ByPatch(outOffsetPolyCrv2);
                outPerimCrvs = outOffsetSrf.PerimeterCurves();
                outOffsetSrf.Dispose();
                outOffsetPolyCrv.Dispose();
                outOffsetPolyCrv2.Dispose();

            }
            catch (Exception)
            {
                outPerimCrvs = null;
            }
            

            Dictionary<string, Autodesk.DesignScript.Geometry.Curve[]> newOutput;
            newOutput = new Dictionary<string, Autodesk.DesignScript.Geometry.Curve[]>
            {
                {"insetCrvs",inPerimCrvs},
                {"outsetCrvs",outPerimCrvs}
            };

            //Dispose all redundant geometry
            
            plyCrv.Dispose();
            foreach (Autodesk.DesignScript.Geometry.Curve c in srfPerimCrvs)
            {
                c.Dispose();
            }

            return newOutput;

        }
        #endregion

        #region PlanarBase
        /// <summary>
        /// Extract the planar base surface/s from a list of surfaces.
        /// </summary>
        /// <param name="surfaces"></param>
        /// <search>geometry,base,planar,surface,surfaces,bottom</search>
        [MultiReturn(new[] { "baseSurfaces", "otherSurfaces" })]
        public static Dictionary<string, object> PlanarBase(this List<Autodesk.DesignScript.Geometry.Surface> surfaces)
        {
            List<double> zValues = new List<double>();
            foreach (var srf in surfaces)
            {
                double z = DSCore.Math.Round(srf.PointAtParameter(0.5, 0.5).Z);
                zValues.Add(z);
            }
            double min = zValues.Min();

            List<bool> boolList = new List<bool>();
            foreach (double z in zValues)
            {
                if (z == min)
                {
                    boolList.Add(true);
                }
                else
                {
                    boolList.Add(false);
                }
            }
            List<Autodesk.DesignScript.Geometry.Surface> baseSrfs = surfaces.Zip(boolList, (name, filter) => new { name = name, filter = filter, }).Where(item => item.filter == true).Select(item => item.name).ToList();
            List<Autodesk.DesignScript.Geometry.Surface> otherSrfs = surfaces.Zip(boolList, (name, filter) => new { name = name, filter = filter, }).Where(item => item.filter == false).Select(item => item.name).ToList();

            Dictionary<string, object> newOutput;
            newOutput = new Dictionary<string, object>
            {
                {"baseSurfaces",baseSrfs},
                {"otherSurfaces",otherSrfs}

            };
            return newOutput;
        }
        #endregion

        #region PlanarTop
        /// <summary>
        /// Extract the planar top surface/s from a list of surfaces.
        /// </summary>
        /// <param name="surfaces"></param>
        /// <search>geometry,top,planar,surface,surfaces,ceiling</search>
        [MultiReturn(new[] { "topSurfaces", "otherSurfaces" })]
        public static Dictionary<string, object> PlanarTop(this List<Autodesk.DesignScript.Geometry.Surface> surfaces)
        {
            List<double> zValues = new List<double>();
            foreach (var srf in surfaces)
            {
                double z = System.Math.Round(srf.PointAtParameter(0.5, 0.5).Z);
                zValues.Add(z);
            }
            double max = zValues.Max();

            List<bool> boolList = new List<bool>();
            foreach (double z in zValues)
            {
                if (z == max)
                {
                    boolList.Add(true);
                }
                else
                {
                    boolList.Add(false);
                }
            }
            List<Autodesk.DesignScript.Geometry.Surface> topSrfs = surfaces.Zip(boolList, (name, filter) => new { name = name, filter = filter, }).Where(item => item.filter == true).Select(item => item.name).ToList();
            List<Autodesk.DesignScript.Geometry.Surface> otherSrfs = surfaces.Zip(boolList, (name, filter) => new { name = name, filter = filter, }).Where(item => item.filter == false).Select(item => item.name).ToList();

            Dictionary<string, object> newOutput;
            newOutput = new Dictionary<string, object>
            {
                {"topSurfaces",topSrfs},
                {"otherSurfaces",otherSrfs}

            };
            return newOutput;
        }
        #endregion

        #region SplitPlanarSurfaceByMultipleCurves
        /// <summary>
        /// Splits a surface by multiple curves.
        /// </summary>
        /// <param name="srf"></param>
        /// <param name="crvs"></param>
        /// <search></search>
        public static Autodesk.DesignScript.Geometry.Geometry[] SplitPlanarSurfaceByMultipleCurves(this Autodesk.DesignScript.Geometry.Surface srf, List<Autodesk.DesignScript.Geometry.Curve> crvs)
        {
            Autodesk.DesignScript.Geometry.Vector vec = srf.NormalAtParameter(0.5, 0.5);

            List<Autodesk.DesignScript.Geometry.Surface> srfLst = new List<Autodesk.DesignScript.Geometry.Surface>();
            foreach (Autodesk.DesignScript.Geometry.Curve crv in crvs)
            {
                Autodesk.DesignScript.Geometry.Surface splitSrf = crv.Extrude(vec, 5000);
                srfLst.Add(splitSrf);
            }

            Autodesk.DesignScript.Geometry.PolySurface polysrf = Autodesk.DesignScript.Geometry.PolySurface.ByJoinedSurfaces(srfLst);

            Autodesk.DesignScript.Geometry.Geometry[] geo = srf.Split(polysrf);

            vec.Dispose();
            polysrf.Dispose();
            foreach (Autodesk.DesignScript.Geometry.Surface s in srfLst)
            {
                s.Dispose();
            }

            return geo;

        }
        #endregion
    }
    [IsVisibleInDynamoLibrary(false)]
    internal static class Point
    {

        #region CompareCoincidental
        /// <summary>
        /// Compare a point against another to see if it is the same
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="tolerance">number of decimal places to round to</param>
        /// <search></search>
        public static bool CompareCoincidental(this Autodesk.DesignScript.Geometry.Point point1, Autodesk.DesignScript.Geometry.Point point2, double tolerance = 8)
        {
            double pt1X = System.Math.Round(point1.X,8);
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
        #endregion

        #region MoveAlongCurve
        /// <summary>
        /// Move a point or list of points along a curve based on a parameter witin a range
        /// </summary>
        /// <param name="point"></param>
        /// <param name="polycurve"></param>
        /// <param name="length"></param>
        /// <param name="param"></param>
        /// <search></search>
        public static dynamic MoveAlongCurve(this Autodesk.DesignScript.Geometry.Point point, Autodesk.DesignScript.Geometry.PolyCurve polycurve, double length, double param)
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

                if (minusDist<0)
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
        #endregion

        #region RandomlyMoveAlongCurve
        /// <summary>
        /// Randomly move a point or list of points along a curve within a given percentage range.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="polycurve"></param>
        /// <param name="percentage"></param>
        /// <search></search>
        public static dynamic RandomlyMoveAlongCurve(this Autodesk.DesignScript.Geometry.Point point, Autodesk.DesignScript.Geometry.PolyCurve polycurve, double percentage)
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
        #endregion

    }
    [IsVisibleInDynamoLibrary(false)]
    internal static class Vector
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
