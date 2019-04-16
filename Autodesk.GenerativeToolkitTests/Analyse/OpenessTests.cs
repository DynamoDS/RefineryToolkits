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
    public class OpenessTests : GeometricTestBase
    {
        [Test]
        public void LeftObstacleTest()
        {
            List<Point> polyPoints = new List<Point>()
            {
                Point.ByCoordinates(-1,0),
                Point.ByCoordinates(-1,10),
                Point.ByCoordinates(1,10),
                Point.ByCoordinates(1,0)
            };

            Polygon obstacle =  Polygon.ByPoints(polyPoints);
            Point origin = Point.ByCoordinates(6, 5);

            double openessScore = Openess.FromPoint(origin, new List<Polygon> { obstacle }, 10, 10, 0);

            Assert.AreEqual(0.25, openessScore);

            polyPoints.ForEach(pt => pt.Dispose());
            obstacle.Dispose();
            origin.Dispose();
        }

        [Test]
        public void RightObstacleTest()
        {
            List<Point> polyPoints = new List<Point>()
            {
                Point.ByCoordinates(-1,0),
                Point.ByCoordinates(-1,10),
                Point.ByCoordinates(1,10),
                Point.ByCoordinates(1,0)
            };

            Polygon obstacle = Polygon.ByPoints(polyPoints);
            Point origin = Point.ByCoordinates(-6, 5);

            double openessScore = Openess.FromPoint(origin, new List<Polygon> { obstacle }, 10, 10, 0);

            Assert.AreEqual(0.25, openessScore);

            polyPoints.ForEach(pt => pt.Dispose());
            obstacle.Dispose();
            origin.Dispose();
        }

        [Test]
        public void TopObstacleTest()
        {
            List<Point> polyPoints = new List<Point>()
            {
                Point.ByCoordinates(-1,0),
                Point.ByCoordinates(-1,10),
                Point.ByCoordinates(1,10),
                Point.ByCoordinates(1,0)
            };

            Polygon obstacle = Polygon.ByPoints(polyPoints);
            Point origin = Point.ByCoordinates(0, 15);

            double openessScore = Openess.FromPoint(origin, new List<Polygon> { obstacle }, 10, 10, 0);

            Assert.AreEqual(0.25, openessScore);

            polyPoints.ForEach(pt => pt.Dispose());
            obstacle.Dispose();
            origin.Dispose();
        }

        [Test]
        public void BottomObstacleTest()
        {
            List<Point> polyPoints = new List<Point>()
            {
                Point.ByCoordinates(-1,0),
                Point.ByCoordinates(-1,10),
                Point.ByCoordinates(1,10),
                Point.ByCoordinates(1,0)
            };

            Polygon obstacle = Polygon.ByPoints(polyPoints);
            Point origin = Point.ByCoordinates(0, -5);

            double openessScore = Openess.FromPoint(origin, new List<Polygon> { obstacle }, 10, 10, 0);

            Assert.AreEqual(0.25, openessScore);

            polyPoints.ForEach(pt => pt.Dispose());
            obstacle.Dispose();
            origin.Dispose();
        }

        [Test]
        public void ObstacleOnBothSides()
        {
            List<Point> leftObstaclePoints = new List<Point>()
            {
                Point.ByCoordinates(-1,0),
                Point.ByCoordinates(-1,10),
                Point.ByCoordinates(1,10),
                Point.ByCoordinates(1,0)
            };
            List<Point> rightObstaclePoints = new List<Point>()
            {
                Point.ByCoordinates(11,0),
                Point.ByCoordinates(11,10),
                Point.ByCoordinates(13,10),
                Point.ByCoordinates(13,0)
            };

            List<Polygon> obstaclePolygons = new List<Polygon>();
            new List<List<Point>> { leftObstaclePoints, rightObstaclePoints }.ForEach(lst => obstaclePolygons.Add(Polygon.ByPoints(lst)));

            Point origin = Point.ByCoordinates(6, 5);
            double openessScore = Openess.FromPoint(origin, obstaclePolygons, 10, 10, 0);

            Assert.AreEqual(0.50, openessScore);

            leftObstaclePoints.ForEach(pt => pt.Dispose());
            rightObstaclePoints.ForEach(pt => pt.Dispose());
            obstaclePolygons.ForEach(poly => poly.Dispose());
            origin.Dispose();
        }
    }
}