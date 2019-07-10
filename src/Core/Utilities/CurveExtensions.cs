using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.RefineryToolkits.Core.Geometry;

namespace Autodesk.RefineryToolkits.Core.Utillites
{
    [IsVisibleInDynamoLibrary(false)]
    public static class CurveExtension
    {
        #region Internal Methods
        [IsVisibleInDynamoLibrary(false)]
        public static Edge ToEdge(this Autodesk.DesignScript.Geometry.Line line)
        {
            return Edge.ByStartVertexEndVertex(line.StartPoint.ToVertex(), line.EndPoint.ToVertex());
        }
        #endregion

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
}
