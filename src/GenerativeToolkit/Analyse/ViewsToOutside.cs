#region namespaces
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Autodesk.GenerativeToolkit.Analyse
{
    public static class ViewsToOutside
    {
        private const string output1 = "score";
        private const string output2 = "segments";

        /// <summary>
        /// Calculates the view to outside from a given point based on a 360 degree ratio.
        /// Returns a number from 0-1 where 0 indicates no points are visible and 1 indicates all points are visible.
        /// </summary>
        /// <param name="boundary">Polygon(s) enclosing all internal Polygons</param>
        /// <param name="internals">List of Polygons representing internal obstructions</param>
        /// <param name="viewSegments">Line segments representing the views to outside</param>
        /// <param name="origin">Origin point to measure from</param>
        /// <returns>precentage of 360 view that is to the outside</returns>
        [MultiReturn(new[] { output1, output2 })]
        public static Dictionary<string, object> ByLineSegments(List<Curve> viewSegments, Point origin, List<Polygon> boundary, [DefaultArgument("[]")] List<Polygon> internals)
        {
            Surface isovist = Isovist.FromPoint(boundary, internals, origin);

            List<Curve> lines = new List<Curve>();
            double outsideViewAngles = 0;
            foreach (Curve segment in viewSegments)
            {
                Geometry[] intersectSegment = isovist.Intersect(segment);
                if (intersectSegment != null)
                {
                    foreach (Curve seg in intersectSegment)
                    {
                        lines.Add(seg);
                        Vector vec1 = Vector.ByTwoPoints(origin, seg.StartPoint);
                        Vector vec2 = Vector.ByTwoPoints(origin, seg.EndPoint);
                        outsideViewAngles += vec1.AngleWithVector(vec2);
                        vec1.Dispose();
                        vec2.Dispose();
                    }
                }
                else
                {
                    continue;
                }
            }
            isovist.Dispose();
            double score = outsideViewAngles / 360;

            return new Dictionary<string, object>()
            {
                {output1, score },
                {output2, lines }
            };
        }      
    }
}
