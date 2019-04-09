#region namespaces
using Autodesk.DesignScript.Geometry;
using System.Collections.Generic;
#endregion

namespace Autodesk.GenerativeToolkit.Analyse
{
    public static class ViewsToOutside
    {
        /// <summary>
        /// 2d representation of the view to outside from any given point.
        /// Uses isovist to calculate the view to outside on a 360 degree ratio.
        /// </summary>
        /// <param name="boundary"></param>
        /// <param name="internals"></param>
        /// <param name="viewSegments"></param>
        /// <param name="origin"></param>
        /// <returns>precentage of 360 view that is to the outside</returns>
        public static double ByLineSegments(List<Polygon> boundary, List<Polygon> internals, List<Line> viewSegments, Point origin)
        {
            Surface isovist = Isovist.FromPoint(boundary, internals, origin);

            double outsideViewAngles = 0;
            foreach (Line segment in viewSegments)
            {
                Geometry[] intersectSegment = isovist.Intersect(segment);
                if (intersectSegment != null)
                {
                    foreach (Line seg in intersectSegment)
                    {
                        Vector vec1 = Vector.ByTwoPoints(origin, seg.StartPoint);
                        Vector vec2 = Vector.ByTwoPoints(origin, seg.EndPoint);
                        outsideViewAngles += vec1.AngleWithVector(vec2);
                        vec1.Dispose();
                        vec2.Dispose();
                        seg.Dispose();
                    }
                }
                else
                {
                    continue;
                }
            }
            isovist.Dispose();
            double score = outsideViewAngles / 360;
            return score;
        }      
    }
}
