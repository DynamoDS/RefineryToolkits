using NUnit.Framework;
using Autodesk.GenerativeToolkit.Analyse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestServices;
using Autodesk.DesignScript.Geometry;

namespace Autodesk.GenerativeToolkit.Analyse.Tests
{
    [TestFixture()]
    public class VisiblePointsTests : GeometricTestBase
    {
        [Test()]
        public void FromOriginTest()
        {
            Polygon boundary = Rectangle.ByWidthLength(50, 50) as Polygon;
            Polygon obstacles = Rectangle.ByWidthLength(15, 15) as Polygon;

            List<Point> samplePoints = new List<Point>();
            foreach (int n in Enumerable.Range(16, 10))
            {
                foreach (int i in Enumerable.Range(-25, 10))
                {
                    samplePoints.Add(Point.ByCoordinates(n, i));
                }
            }

            Point origin = Point.ByCoordinates(-20, -2);
            double visiblePointsScore = VisiblePoints.FromOrigin(origin, samplePoints, new List<Polygon> { boundary }, new List<Polygon> { obstacles });
            
            Assert.AreEqual(0.57, Math.Round(visiblePointsScore,2));
        }
    }
}