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
            ColinearPoints =
            [
                Point.ByCoordinates(260,600),
                Point.ByCoordinates(285,600),
                Point.ByCoordinates(310,600),
                Point.ByCoordinates(335,600)
            ];

            NonColinearPoints = new List<Point>(ColinearPoints)
            {
                Point.ByCoordinates(250, 600 + MarginOfError)
            };
        }

        [Test]
        public void Colinear_CanIdentifyNonColinear()
        {
            var noncolinear = NonColinearPoints.AreColinear();
            Assert.IsFalse(noncolinear);
        }


        [Test]
        public void Colinear_CanIdentify3Points()
        {
            // Arrange
            var threePoints = new List<Point>(ColinearPoints);
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
            var colinear = ColinearPoints.AreColinear();
            Assert.IsTrue(colinear);
            Assert.AreEqual(4, ColinearPoints.Count);
        }

        [Test]
        public void Colinear_CanIdentify6Points()
        {
            // Arrange
            var fivePoints = new List<Point>(ColinearPoints)
            {
                Point.ByCoordinates(250, 600),
                Point.ByCoordinates(350, 600)
            };

            // Act
            var colinear = fivePoints.AreColinear();

            // Assert
            Assert.IsTrue(colinear);
            Assert.AreEqual(6, fivePoints.Count);
        }


    }
}