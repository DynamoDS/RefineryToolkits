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
        // Check if the openess score detects a obstacle on the left
        [Test]
        public void LeftObstacleTest()
        {
            // Create obstacle
            List<Point> polyPoints = new List<Point>()
            {
                Point.ByCoordinates(-1,0),
                Point.ByCoordinates(-1,10),
                Point.ByCoordinates(1,10),
                Point.ByCoordinates(1,0)
            };
            Polygon obstacle =  Polygon.ByPoints(polyPoints);

            // Create a rectangle object with 4 equal sides to check openess score
            Point origin = Point.ByCoordinates(6, 5);
            Surface surface = Surface.ByPatch(Rectangle.ByWidthLength(CoordinateSystem.ByOrigin(origin), 10, 10));

            // Calculate openess score
            double openessScore = Openess.FromSurface(surface,0,new List<Polygon> { }, new List<Polygon> { obstacle });

            // Check of score equals 0.25, as the entire left side is blocked by the obstacle
            Assert.AreEqual(0.25, openessScore);

            // Dispose unused geometry
            polyPoints.ForEach(pt => pt.Dispose());
            obstacle.Dispose();
            origin.Dispose();
        }

        // Check if the openess score detects a obstacle on the right
        [Test]
        public void RightObstacleTest()
        {
            // Create obstacle
            List<Point> polyPoints = new List<Point>()
            {
                Point.ByCoordinates(-1,0),
                Point.ByCoordinates(-1,10),
                Point.ByCoordinates(1,10),
                Point.ByCoordinates(1,0)
            };
            Polygon obstacle = Polygon.ByPoints(polyPoints);

            // Create a rectangle object with 4 equal sides to check openess score
            Point origin = Point.ByCoordinates(-6, 5);
            Surface surface = Surface.ByPatch(Rectangle.ByWidthLength(CoordinateSystem.ByOrigin(origin), 10, 10));

            // Calculate openess score
            double openessScore = Openess.FromSurface(surface, 0, new List<Polygon> { }, new List<Polygon> { obstacle });

            // Check of score equals 0.25, as the entire right side is blocked by the obstacle
            Assert.AreEqual(0.25, openessScore);

            // Dispose unused geometry
            polyPoints.ForEach(pt => pt.Dispose());
            obstacle.Dispose();
            origin.Dispose();
        }

        // Check if the openess score detects a obstacle on the top
        [Test]
        public void TopObstacleTest()
        {
            // Create obstacle
            List<Point> polyPoints = new List<Point>()
            {
                Point.ByCoordinates(-1,0),
                Point.ByCoordinates(-1,10),
                Point.ByCoordinates(1,10),
                Point.ByCoordinates(1,0)
            };
            Polygon obstacle = Polygon.ByPoints(polyPoints);

            // Create a rectangle object with 4 equal sides to check openess score
            Point origin = Point.ByCoordinates(0, 11);
            Surface surface = Surface.ByPatch(Rectangle.ByWidthLength(CoordinateSystem.ByOrigin(origin), 2, 2));

            // Calculate openess score
            double openessScore = Openess.FromSurface(surface, 0, new List<Polygon> { }, new List<Polygon> { obstacle });

            // Check of score equals 0.25, as the entire top is blocked by the obstacle
            Assert.AreEqual(0.25, openessScore);

            // Dispose unused geometry
            polyPoints.ForEach(pt => pt.Dispose());
            obstacle.Dispose();
            origin.Dispose();
        }

        // Check if the openess score detects a obstacle on the bottom
        [Test]
        public void BottomObstacleTest()
        {
            // Create obstacle
            List<Point> polyPoints = new List<Point>()
            {
                Point.ByCoordinates(-1,0),
                Point.ByCoordinates(-1,10),
                Point.ByCoordinates(1,10),
                Point.ByCoordinates(1,0)
            };
            Polygon obstacle = Polygon.ByPoints(polyPoints);

            // Create a rectangle object with 4 equal sides to check openess score
            Point origin = Point.ByCoordinates(0, -1);
            Surface surface = Surface.ByPatch(Rectangle.ByWidthLength(CoordinateSystem.ByOrigin(origin), 2, 2));

            // Calculate openess score
            double openessScore = Openess.FromSurface(surface, 0, new List<Polygon> { }, new List<Polygon> { obstacle });

            // Check of score equals 0.25, as the entire bottom is blocked by the obstacle
            Assert.AreEqual(0.25, openessScore);
            
            // Dispose unused geometry
            polyPoints.ForEach(pt => pt.Dispose());
            obstacle.Dispose();
            origin.Dispose();
        }

        // Check if the openess score detects a obstacle on the left and right
        [Test]
        public void ObstacleOnBothSides()
        {
            // Create obstacles
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

            // Create a rectangle object with 4 equal sides to check openess score
            Point origin = Point.ByCoordinates(6, 5);
            Surface surface = Surface.ByPatch(Rectangle.ByWidthLength(CoordinateSystem.ByOrigin(origin), 10, 10));

            // Calculate openess score
            double openessScore = Openess.FromSurface(surface, 0, new List<Polygon> { }, obstaclePolygons);

            // Check if score equals 0.50, as the entire right and left side is blocked by the obstacles
            Assert.AreEqual(0.50, openessScore);

            // Dispose unused geometry
            leftObstaclePoints.ForEach(pt => pt.Dispose());
            rightObstaclePoints.ForEach(pt => pt.Dispose());
            obstaclePolygons.ForEach(poly => poly.Dispose());
            origin.Dispose();
        }
    }
}