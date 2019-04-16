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
        [Test]
        public void IsovistFromPointTestSurfaceAreaIsCorrect()
        {
            Polygon layoutPolygon = Polygon.ByPoints(new List<Point>
            {
                Point.ByCoordinates(0,0),
                Point.ByCoordinates(10,0),
                Point.ByCoordinates(10,10),
                Point.ByCoordinates(0,10)
            });

            Point originPoint = layoutPolygon.Center();

            Surface isovist = Isovist.FromPoint(new List<Polygon> { layoutPolygon }, new List<Polygon> { layoutPolygon }, originPoint);

            Assert.AreEqual(Math.Ceiling(isovist.Area), 10*10);
        }

        [Test]
        public void IsovistObstructionsWorks()
        {
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
            Point originPoint = layoutPolygon.Center();

            Surface isovist = Isovist.FromPoint(new List<Polygon> { layoutPolygon }, new List<Polygon> { internals }, originPoint);

            Assert.AreNotEqual(10 * 10, Math.Ceiling(isovist.Area));
            Assert.False(isovist.DoesIntersect(internals.Center()));
        }
    }
}