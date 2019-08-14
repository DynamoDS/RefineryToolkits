using Autodesk.DesignScript.Geometry;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TestServices;

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
            this.boundary = Rectangle.ByWidthLength(50, 50) as Polygon;
            this.obstacles = Rectangle.ByWidthLength(15, 15) as Polygon;

            this.samplePoints = new List<Point>();
            foreach (int n in Enumerable.Range(16, 10))
            {
                foreach (int i in Enumerable.Range(-25, 10))
                {
                    this.samplePoints.Add(Point.ByCoordinates(n, i));
                }
            }

            this.origin = Point.ByCoordinates(-20, -2);
        }

        /// <summary>
        /// Check Visible Points dictionary output is correct
        /// </summary>
        [Test]
        public void VisiblePointsDicionaryOutputTest()
        {
            var result = Visibility.OfPointsFromOrigin(
                this.origin,
                this.samplePoints,
                new List<Polygon> { this.boundary },
                new List<Polygon> { this.obstacles });

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

            var result = Visibility.OfPointsFromOrigin(
                this.origin,
                this.samplePoints,
                new List<Polygon> { this.boundary },
                new List<Polygon> { this.obstacles });

            var visiblePointsScore = (double)result["score"];

            Assert.AreEqual(0.57, Math.Round(visiblePointsScore, 2));
        }
    }
}