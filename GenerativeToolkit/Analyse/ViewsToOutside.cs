#region namespaces
using Autodesk.DesignScript.Geometry;
using System.Collections.Generic;
#endregion

namespace Autodesk.GenerativeToolkit.Analyse
{
    public static class ViewsToOutside
    {
        /// <summary>
        /// calculates the view to outside from a given point based on a 360 degree ratio.
        /// </summary>
        /// <param name="boundary">Polygon(s) enclosing all internal Polygons</param>
        /// <param name="internals">List of Polygons representing internal obstructions</param>
        /// <param name="viewSegments">Line segments representing the views to outside</param>
        /// <param name="origin">Origin point to measure from</param>
        /// <returns>precentage of 360 view that is to the outside</returns>
        public static double ByLineSegments(List<Polygon> boundary, List<Polygon> internals, List<Curve> viewSegments, Point origin)
        {
            Surface isovist = Isovist.FromPoint(boundary, internals, origin);

            double outsideViewAngles = 0;
            foreach (Curve segment in viewSegments)
            {
                Geometry[] intersectSegment = isovist.Intersect(segment);
                if (intersectSegment != null)
                {
                    foreach (Curve seg in intersectSegment)
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
