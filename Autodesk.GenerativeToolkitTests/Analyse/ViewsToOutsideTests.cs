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
    [TestFixture]
    public class ViewsToOutsideTests : GeometricTestBase
    {
        [Test]
        public void ViewToOutside360()
        {
            Rectangle boundaryRect = Rectangle.ByWidthLength(20, 20);
            List<Curve> lines = boundaryRect.Explode().Cast<Curve>().ToList();
            
            List<Point> stPoints = new List<Point>();
            List<Point> endPoints = new List<Point>();
            foreach (Curve line in lines)
            {
                stPoints.Add(line.StartPoint);
                endPoints.Add(line.EndPoint);
            }
            Point origin = Point.ByCoordinates(0, 0);

            List<Polygon> boundaryPoly = new List<Polygon> { Polygon.ByPoints(stPoints) };

            var result = ViewsToOutside.ByLineSegments(lines,origin,boundaryPoly,new List<Polygon> { });

            Assert.IsTrue(result.Keys.Contains("score"));
            Assert.IsTrue(result.Keys.Contains("segments"));

            var viewScore = (double)result["score"];

            Assert.AreEqual(1.0,viewScore);
        }
    }
}