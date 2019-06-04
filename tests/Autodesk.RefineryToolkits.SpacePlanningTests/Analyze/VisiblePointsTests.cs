using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestServices;
using Autodesk.DesignScript.Geometry;

namespace Autodesk.RefineryToolkits.SpacePlanning.Analyze.Tests
{
    [TestFixture]
    public class VisiblePointsTests : GeometricTestBase
    {
        private Polygon boundary;
        private Polygon obstacles;
        private List<Point> samplePoints;
        private Point origin;

        [SetUp]
        public void BeforeTest()
        {
            boundary = Rectangle.ByWidthLength(50, 50) as Polygon;
            obstacles = Rectangle.ByWidthLength(15, 15) as Polygon;

            samplePoints = new List<Point>();
            foreach (int n in Enumerable.Range(16, 10))
            {
                foreach (int i in Enumerable.Range(-25, 10))
                {
                    samplePoints.Add(Point.ByCoordinates(n, i));
                }
            }

            origin = Point.ByCoordinates(-20, -2);
        }

        /// <summary>
        /// Check Visible Points dictionary output is correct
        /// </summary>
        [Test]
        public void VisiblePointsDicionaryOutputTest()
        {
            var result = VisiblePoints.FromOrigin(origin, samplePoints, new List<Polygon> { boundary }, new List<Polygon> { obstacles });

            Assert.IsTrue(result.Keys.Contains("score"));
            Assert.IsTrue(result.Keys.Contains("visiblePoints"));
        }


        /// <summary>
        /// Check if VisiblePoints returns the right score of visible points in a layout
        /// with one obstacle and a grid of 
        /// </summary>
        [Test]
        public void FromOriginTest()
        {
            
            var result = VisiblePoints.FromOrigin(origin, samplePoints, new List<Polygon> { boundary }, new List<Polygon> { obstacles });

            var visiblePointsScore = (double)result["score"];

            Assert.AreEqual(0.57, Math.Round(visiblePointsScore,2));
        }
    }
}