using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using ClipperLib;

namespace GenerativeToolkit
{
    using Path = List<IntPoint>;
    using Paths = List<List<IntPoint>>;
    public class PolygonClipper
    {
        public static List<Point> cliptest(List<Polygon> subjPolygons, Polygon clipPolygon)
        {
            Paths subj = new Paths();
            foreach (var poly in subjPolygons)
            {
                Path subjPath = new Path();
                subjPath.AddRange(GetIntPoints(poly));
                subj.Add(subjPath);
            }

            Path clip = new Path();
            clip.AddRange(GetIntPoints(clipPolygon));

            Clipper c = new Clipper();
            c.AddPaths(subj, PolyType.ptSubject, true);
            c.AddPath(clip, PolyType.ptClip, true);

            Paths solution = new Paths();
            c.Execute(ClipType.ctUnion, solution, PolyFillType.pftEvenOdd, PolyFillType.pftPositive);

            List<Point> polygonPoints = new List<Point>();
            foreach (var p in solution)
            {
                foreach (var i in p)
                {
                    polygonPoints.Add(Point.ByCoordinates(i.X, i.Y));
                }
            }
            return polygonPoints;
        }

        private static List<IntPoint> GetIntPoints(Polygon polygon)
        {
            Point[] points = polygon.Points;
            List<IntPoint> intPoints = new List<IntPoint>();
            foreach (Point point in points)
            {
                IntPoint intPoint = new IntPoint(point.X, point.Y);
                intPoints.Add(intPoint);
            }
            return intPoints;
        }
    }
}
