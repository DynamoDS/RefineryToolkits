using Autodesk.DesignScript.Geometry;
using NUnit.Framework;
using System.Collections.Generic;
using TestServices;

namespace Autodesk.RefineryToolkits.Core.Utillites.Tests
{
    [TestFixture]
    public class PointExtensionTests : GeometricTestBase
    {
        private List<Point> ColinearPoints;
        private List<Point> NonColinearPoints;
        private const double MarginOfError = 1e-5;

        [SetUp]
        public void BeforeTest()
        {
            this.ColinearPoints = new List<Point>
            {
                Point.ByCoordinates(260,600),
                Point.ByCoordinates(285,600),
                Point.ByCoordinates(310,600),
                Point.ByCoordinates(335,600)
            };

            this.NonColinearPoints = new List<Point>(this.ColinearPoints);
            this.NonColinearPoints.Add(Point.ByCoordinates(250, 600 + MarginOfError));
        }

        [Test]
        public void Colinear_CanIdentifyNonColinear()
        {
            var noncolinear = this.NonColinearPoints.AreColinear();
            Assert.IsFalse(noncolinear);
        }


        [Test]
        public void Colinear_CanIdentify3Points()
        {
            // Arrange
            var threePoints = new List<Point>(this.ColinearPoints);
            threePoints.RemoveAt(threePoints.Count - 1);

            // Act
            var colinear = threePoints.AreColinear();

            // Assert
            Assert.AreEqual(3, threePoints.Count);
            Assert.IsTrue(colinear);
        }

        [Test]
        public void Colinear_CanIdentify4Points()
        {
            var colinear = this.ColinearPoints.AreColinear();
            Assert.IsTrue(colinear);
            Assert.AreEqual(4, ColinearPoints.Count);
        }

        [Test]
        public void Colinear_CanIdentify6Points()
        {
            // Arrange
            var fivePoints = new List<Point>(this.ColinearPoints);
            fivePoints.Add(Point.ByCoordinates(250, 600));
            fivePoints.Add(Point.ByCoordinates(350, 600));

            // Act
            var colinear = fivePoints.AreColinear();

            // Assert
            Assert.IsTrue(colinear);
            Assert.AreEqual(6, fivePoints.Count);
        }


    }
}