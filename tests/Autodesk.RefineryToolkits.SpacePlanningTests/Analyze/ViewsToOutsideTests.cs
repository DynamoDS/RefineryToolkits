using Autodesk.DesignScript.Geometry;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TestServices;

namespace Autodesk.RefineryToolkits.SpacePlanning.Analyze.Tests
{
    [TestFixture]
    public class ViewsToOutsideTests : GeometricTestBase
    {
        private List<Curve> lines;
        private Point origin;
        private List<Polygon> boundaryPoly;

        [SetUp]
        public void BeforeTest()
        {
            this.boundaryPoly = new List<Polygon> { Rectangle.ByWidthLength(20, 20) as Polygon };
            this.lines = this.boundaryPoly[0].Explode().Cast<Curve>().ToList();
            this.origin = Point.ByCoordinates(0, 0);
        }

        /// <summary>
        /// Check views to outside dictionary output is correct
        /// </summary>
        [Test]
        public void ViewsToOutsideDictionaryOutputTest()
        {

            // Result of Visibility.OfLinesFromOrigin
            var result = Visibility.OfLinesFromOrigin(
                this.origin,
                this.lines,
                this.boundaryPoly,
                new List<Polygon> { });

            // Check if output of node is a Dictionary that contains both the
            // "score" and "segments" key
            Assert.IsTrue(result.Keys.Contains("score"));
            Assert.IsTrue(result.Keys.Contains("segments"));
        }

        /// <summary>
        /// Checks if the output score is correct in a layout with no obstacles blocking the views
        /// </summary>
        [Test]
        public void CheckIfOutputScoreIsCorrectWithNoObstrutions()
        {
            // Result of Visibility.OfLinesFromOrigin
            var result = Visibility.OfLinesFromOrigin(
                this.origin,
                this.lines,
                this.boundaryPoly,
                new List<Polygon> { });

            // Check if the score output is 1.0
            // as there are no obstacles blocking the views to outside
            var viewScore = (double)result["score"];
            Assert.AreEqual(1.0, viewScore);
        }

        /// <summary>
        /// Checks that internal obstacels in the layout are detected
        /// </summary>
        [Test]
        public void CheckIfViewsToOutsideDetectsObstaclesInLayout()
        {
            Polygon internalPoly = Rectangle.ByWidthLength(5, 5) as Polygon;
            Point newOrigin = this.origin.Translate(10) as Point;
            // Result of Visibility.OfLinesFromOrigin
            var result = Visibility.OfLinesFromOrigin(
                newOrigin,
                this.lines,
                this.boundaryPoly,
                new List<Polygon> { internalPoly });

            var viewScore = (double)result["score"];
            Assert.AreNotEqual(1.0, viewScore);
        }


    }
}