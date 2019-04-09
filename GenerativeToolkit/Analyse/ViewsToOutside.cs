using Autodesk.DesignScript.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodesk.GenerativeToolkit.Analyse
{
    public static class ViewsToOutside
    {
        public static double ByLineSegments(List<Polygon> boundary, List<Polygon> internals, List<Line> viewSegments, Point origin)
        {
            Surface isovist = Isovist.IsovistFromPoint(boundary, internals, origin);

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
