using NUnit.Framework;
using Autodesk.RefineryToolkits.SpacePlanning.Analyze;
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
    public class IsovistTests : GeometricTestBase
    {
        private static Polygon layoutPolygon;

        /// <summary>
        /// Setup Geometry to be used across tests
        /// </summary>
        [SetUp]
        public void BeforeTest()
        {
            layoutPolygon = Rectangle.ByWidthLength(10, 10) as Polygon;
        }

        /// <summary>
        /// Checks if the area returned by the Isovist.FromPoint is correct
        /// </summary>
        [Test]
        public void IsovistFromPointReturnsCorrectSurfaceAreaTest()
        {          
            // Create origin point
            Point originPoint = layoutPolygon.Center();

            // Create isovist form the origin point
            Surface isovist = Visibility.IsovistFromPoint(originPoint, new List<Polygon> { layoutPolygon }, new List<Polygon> { layoutPolygon });
            
            // Checks if the area of the isovist is equal to the area of the layout
            // as there are no obstructions the entire layout should be visible from the origin point
            Assert.AreEqual(isovist.Area, Surface.ByPatch(layoutPolygon).Area);
        }

        /// <summary>
        /// Checks if the Isovist.FromPoint are detecting if there are obstacles of the layout
        /// </summary>
        [Test]
        public void IsovistFromPointDetectsObstructionsInLayoutTest()
        {
            // Create obstruction
            Polygon internals = Rectangle.ByWidthLength(5, 5) as Polygon;

            // Create origin point
            Point originPoint = Point.ByCoordinates(3,3);

            // Create isovist form the origin point
            Surface isovist = Visibility.IsovistFromPoint(originPoint, new List<Polygon> { layoutPolygon }, new List<Polygon> { internals });

            // Checks that the area returned by the isovist is not equal to the area of the layout
            // and that the isovist does not intersect the midpoint of the obstacle.
            Assert.AreNotEqual(Surface.ByPatch(layoutPolygon).Area, isovist.Area);
            Assert.False(isovist.DoesIntersect(internals.Center()));
        }

        /// <summary>
        /// Checks if the isovist node throws an exception if the origin point is inside
        /// one of the internal polygons.
        /// </summary>
        [Test]
        public void IsovistFromPointReturnsExceptionIfOriginPointIsInsideInternalPolygonTest()
        {
            // Create obstruction
            Polygon internals = Rectangle.ByWidthLength(5, 5) as Polygon;

            // Create origin point
            Point originPoint = Point.ByCoordinates(0, 0);

            Assert.Throws<ApplicationException>(() => Visibility.IsovistFromPoint(originPoint, new List<Polygon> { layoutPolygon }, new List<Polygon> { internals }));

        }
    }
}