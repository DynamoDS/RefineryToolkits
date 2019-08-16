using Autodesk.DesignScript.Geometry;
using NUnit.Framework;
using System.Collections.Generic;
using TestServices;

namespace Autodesk.RefineryToolkits.SpacePlanning.Analyze.Tests
{
    [TestFixture]
    public class OpenessTests : GeometricTestBase
    {
        private static Polygon obstacle;

        [SetUp]
        public void BeforeTest()
        {
            obstacle = Rectangle.ByWidthLength(10, 10) as Polygon;
        }

        /// <summary>
        /// Check if the openess score detects a obstacle on the left
        /// </summary>
        [Test]
        public void LeftObstacleTest()
        {
            // Create a rectangle object with 4 equal sides to check openess score
            var origin = Point.ByCoordinates(-10, 0);
            var surface = Surface.ByPatch(Rectangle.ByWidthLength(CoordinateSystem.ByOrigin(origin), 10, 10));

            // Calculate openess score
            var openessScore = Openess.FromSurface(surface, 0, new List<Polygon> { }, new List<Polygon> { obstacle });

            // Check of score equals 75%, as the entire left side is blocked by the obstacle
            Assert.AreEqual(75d, openessScore);

            // Dispose unused geometry
            obstacle.Dispose();
            origin.Dispose();
        }

        /// <summary>
        /// Check if the openess score detects a obstacle on the right
        /// </summary>
        [Test]
        public void RightObstacleTest()
        {
            // Create a rectangle object with 4 equal sides to check openess score
            var origin = Point.ByCoordinates(10, 0);
            var surface = Surface.ByPatch(Rectangle.ByWidthLength(CoordinateSystem.ByOrigin(origin), 10, 10));

            // Calculate openess score
            var openessScore = Openess.FromSurface(surface, 0, new List<Polygon> { }, new List<Polygon> { obstacle });

            // Check of score equals 75%, as the entire right side is blocked by the obstacle
            Assert.AreEqual(75d, openessScore);

            // Dispose unused geometry
            obstacle.Dispose();
            origin.Dispose();
        }

        /// <summary>
        /// Check if the openess score detects a obstacle on the top
        /// </summary>
        [Test]
        public void TopObstacleTest()
        {
            // Create a rectangle object with 4 equal sides to check openess score
            var origin = Point.ByCoordinates(0, 10);
            var surface = Surface.ByPatch(Rectangle.ByWidthLength(CoordinateSystem.ByOrigin(origin), 10, 10));

            // Calculate openess score
            var openessScore = Openess.FromSurface(surface, 0, new List<Polygon> { }, new List<Polygon> { obstacle });

            // Check of score equals 75%, as the entire top is blocked by the obstacle
            Assert.AreEqual(75d, openessScore);

            // Dispose unused geometry
            obstacle.Dispose();
            origin.Dispose();
        }

        /// <summary>
        /// Check if the openess score detects a obstacle on the bottom
        /// </summary>
        [Test]
        public void BottomObstacleTest()
        {
            // Create a rectangle object with 4 equal sides to check openess score
            var origin = Point.ByCoordinates(0, -10);
            var surface = Surface.ByPatch(Rectangle.ByWidthLength(CoordinateSystem.ByOrigin(origin), 10, 10));

            // Calculate openess score
            var openessScore = Openess.FromSurface(surface, 0, new List<Polygon> { }, new List<Polygon> { obstacle });

            // Check of score equals 75%, as the entire bottom is blocked by the obstacle
            Assert.AreEqual(75d, openessScore);

            // Dispose unused geometry
            obstacle.Dispose();
            origin.Dispose();
        }

        /// <summary>
        /// Check if the openess score detects a obstacle on the left and right
        /// </summary>
        [Test]
        public void ObstacleOnBothSides()
        {
            var obstaclePolygons = new List<Polygon>()
            {
                obstacle.Translate(-10) as Polygon,
                obstacle.Translate(10) as Polygon
            };

            // Create a rectangle object with 4 equal sides to check openess score
            var surface = Surface.ByPatch(Rectangle.ByWidthLength(10, 10));

            // Calculate openess score
            var openessScore = Openess.FromSurface(surface, 0, new List<Polygon> { }, obstaclePolygons);

            // Check if score equals 50%, as the entire right and left side is blocked by the obstacles
            Assert.AreEqual(50d, openessScore);

            // Dispose unused geometry
            obstaclePolygons.ForEach(poly => poly.Dispose());
        }
    }
}