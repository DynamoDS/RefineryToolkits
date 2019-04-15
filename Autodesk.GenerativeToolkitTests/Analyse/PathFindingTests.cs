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
    public class PathFindingTests : GeometricTestBase
    {
        [Test]
        public void ShortestPathTest()
        {
            List<Point> boundaryPoints = new List<Point>
            {
                Point.ByCoordinates(25,-25),
                Point.ByCoordinates(25,25),
                Point.ByCoordinates(-25,25),
                Point.ByCoordinates(-25,-25)
            };
            List<Point> internal1Points = new List<Point>
            {
                Point.ByCoordinates(-1,0),
                Point.ByCoordinates(-1,10),
                Point.ByCoordinates(1,10),
                Point.ByCoordinates(1,0)
            };
            List<Point> internal2Points = new List<Point>
            {
                Point.ByCoordinates(11,0),
                Point.ByCoordinates(11,10),
                Point.ByCoordinates(13,10),
                Point.ByCoordinates(13,0)
            };

            Point originPoint = Point.ByCoordinates(-12, 5);
            Point destination = Point.ByCoordinates(17, 3);

            Polygon boundary = Polygon.ByPoints(boundaryPoints);

            List<Polygon> internalPolygons = new List<Polygon>();
            new List<List<Point>> { internal1Points, internal2Points }.ForEach(lst => internalPolygons.Add(Polygon.ByPoints(lst)));

            Visibility visibilityGraph = PathFinding.CreateVisibilityGraph(new List<Polygon> { boundary }, internalPolygons);

            Dictionary<string, object> shortestPath = PathFinding.ShortestPath(visibilityGraph, originPoint, destination);

            //Assert.AreEqual(35.22, Math.Round(length.GetValueOrDefault()));
        }

        [Test]
        public void CreateVisibilityGraphTest()
        {
            Assert.Fail();
        }

        [Test]
        public void LinesTest()
        {
            Assert.Fail();
        }
    }
}