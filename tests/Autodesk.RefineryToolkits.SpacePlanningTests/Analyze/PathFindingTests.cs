using Autodesk.DesignScript.Geometry;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TestServices;

namespace Autodesk.RefineryToolkits.SpacePlanning.Analyze.Tests
{
    [TestFixture]
    public class PathFindingTests : GeometricTestBase
    {
        private const string pathOutputPort = "Path";
        private const string lengthOutputPort = "Length";

        private Polygon boundary;
        private Polygon internalPolygon;
        private Point originPoint;
        private Point destination;
        private RepresentableGraph visibilityGraph;
        private Dictionary<string, object> result;

        [SetUp]
        public void BeforeTest()
        {
            boundary = Rectangle.ByWidthLength(50, 50) as Polygon;
            internalPolygon = Rectangle.ByWidthLength(5, 25) as Polygon;
            // Create origin and desitination point
            originPoint = Point.ByCoordinates(-10, 5);
            destination = Point.ByCoordinates(20, 3);
        }

        /// <summary>
        /// Checks if shortest path can create a new VisibilityGraph
        /// </summary>
        [Test]
        public void ShortestPathCanCreateVisibilityGraphTest()
        {
            // Create visibility graph used to calculate the shortest path
            visibilityGraph = PathFinding.CreateVisibilityGraph([boundary], [internalPolygon]);
            // Check if the visibility graph is created properly
            Assert.IsNotNull(visibilityGraph);
        }

        /// <summary>
        /// Check shortest path dictionary output is correct
        /// </summary>
        [Test]
        public void ShortestPathDictionaryOutputTest()
        {
            // Create shortest path
            result = PathFinding.ShortestPath(
                originPoint,
                destination,
                [boundary],
                [internalPolygon]);

            // Check if the result of the Shortest path is a dictionary containing the keys "path" and "length"
            Assert.IsTrue(result.Keys.Contains(pathOutputPort));
            Assert.IsTrue(result.Keys.Contains(lengthOutputPort));
        }

        /// <summary>
        /// Check if the length of the path is correct
        /// </summary>
        [Test]
        public void ShortestPathLengthTest()
        {
            var length = (double)result[lengthOutputPort];
            Assert.AreEqual(35.519, Math.Round(length, 3));
        }

        /// <summary>
        /// Check if the PathFinding.Lines returns the correct amount of lines
        /// </summary>
        [Test]
        public void ShortestPathDynamoLinesFromPathTest()
        {
            var lines = (List<Line>)result[pathOutputPort];
            Assert.AreEqual(3, lines.Count);
        }
    }
}