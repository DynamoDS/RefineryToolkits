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
        public void AreColinearTest()
        {
            var colinear = this.ColinearPoints.AreColinear();
            var noncolinear = this.NonColinearPoints.AreColinear();

            Assert.IsTrue(colinear);
            Assert.IsFalse(noncolinear);
        }
    }
}