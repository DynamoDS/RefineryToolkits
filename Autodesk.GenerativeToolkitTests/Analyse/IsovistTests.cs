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
    public class IsovistTests : GeometricTestBase
    {
        // Checks if the area returned by the Isovist.FromPoint is correct
        [Test]
        public void IsovistFromPointTestSurfaceAreaIsCorrect()
        {
            // Create layout with no obstructions
            Polygon layoutPolygon = Polygon.ByPoints(new List<Point>
            {
                Point.ByCoordinates(0,0),
                Point.ByCoordinates(10,0),
                Point.ByCoordinates(10,10),
                Point.ByCoordinates(0,10)
            });

            // Create origin point
            Point originPoint = layoutPolygon.Center();

            // Create isovist form the origin point
            Surface isovist = Isovist.FromPoint(new List<Polygon> { layoutPolygon }, new List<Polygon> { layoutPolygon }, originPoint);
            
            // Checks if the area of the isovist is equal to the area of the layout
            // as there are no obstructions the entire layout should be visible from the origin point
            Assert.AreEqual(isovist.Area, Surface.ByPatch(layoutPolygon).Area);
        }

        // Checks if the Isovist.FromPoint are detecting the obstacles of the layout
        [Test]
        public void IsovistObstructionsWorks()
        {
            // Create layout with obstructions
            Polygon layoutPolygon = Polygon.ByPoints(new List<Point>
            {
                Point.ByCoordinates(0,0),
                Point.ByCoordinates(10,0),
                Point.ByCoordinates(10,10),
                Point.ByCoordinates(0,10)
            });

            Polygon internals = Polygon.ByPoints(new List<Point>
            {
                Point.ByCoordinates(2.5,0),
                Point.ByCoordinates(7.5,0),
                Point.ByCoordinates(7.5,5),
                Point.ByCoordinates(2.5,5)
            });

            // Create origin point
            Point originPoint = layoutPolygon.Center();

            // Create isovist form the origin point
            Surface isovist = Isovist.FromPoint(new List<Polygon> { layoutPolygon }, new List<Polygon> { internals }, originPoint);

            // Checks that the area returned by the isovist is not equal to the area of the layout
            // and that the isovist does not intersect the midpoint of the obstacle.
            Assert.AreNotEqual(Surface.ByPatch(layoutPolygon).Area, isovist.Area);
            Assert.False(isovist.DoesIntersect(internals.Center()));
        }
    }
}