using Autodesk.DesignScript.Geometry;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TestServices;

namespace Autodesk.RefineryToolkits.SpacePlanning.Generate.Tests
{
    [TestFixture]
    public class BinPacking2DTests : GeometricTestBase
    {
        private const string packedOutputPort = "Packed Rectangles";
        private const string remainOutputPort = "Remaining Rectangles";
        private const string indicesOutputPort = "Packed Indices";

        [Test]
        public void OneBin_CanPackWithNoLeftovers()
        {
            // Arrange
            var bins = new List<Rectangle> { Rectangle.ByWidthLength(50, 50) };
            var items = new List<Rectangle>
            {
                Rectangle.ByWidthLength(20,23),
                Rectangle.ByWidthLength(12,30),
                Rectangle.ByWidthLength(33,20),
                Rectangle.ByWidthLength(12,22)
            };

            var expectedIndices = new List<List<int>>();
            expectedIndices.Add(new List<int> { 0, 1, 2, 3 });

            // Act
            Dictionary<string, object> result = BinPacking.Pack2D(items, bins, RectanglePackingStrategy.BestShortSideFits);

            // Assert
            Assert.IsTrue(result.Keys.Contains(packedOutputPort));
            Assert.IsTrue(result.Keys.Contains(remainOutputPort));
            Assert.IsTrue(result.Keys.Contains(indicesOutputPort));

            var packedRectangles = (List<List<Rectangle>>)result[packedOutputPort];
            Assert.AreEqual(1, packedRectangles.Count);
            Assert.AreEqual(4, packedRectangles.First().Count);

            var remainItems = (List<Rectangle>)result[remainOutputPort];
            Assert.AreEqual(0, remainItems.Count);

            var packedIndices = (List<List<int>>)result[indicesOutputPort];
            CollectionAssert.AreEqual(packedIndices, expectedIndices);
        }

        [Test]
        public void OneBin_CanPackAndHaveLeftovers()
        {
            // Arrange
            var bins = new List<Rectangle> { Rectangle.ByWidthLength(50, 50) };
            var items = new List<Rectangle>
            {
                Rectangle.ByWidthLength(20,23),
                Rectangle.ByWidthLength(12,30),
                Rectangle.ByWidthLength(33,20),
                Rectangle.ByWidthLength(32,31),
                Rectangle.ByWidthLength(12,22),
                Rectangle.ByWidthLength(15,15)
            };
            var expectedIndices = new List<List<int>>();
            expectedIndices.Add(new List<int> { 0, 1, 2, 4 });

            // Act
            Dictionary<string, object> result = BinPacking.Pack2D(items, bins, RectanglePackingStrategy.BestShortSideFits);

            // Assert
            Assert.IsTrue(result.Keys.Contains(packedOutputPort));
            Assert.IsTrue(result.Keys.Contains(remainOutputPort));
            Assert.IsTrue(result.Keys.Contains(indicesOutputPort));

            var packedRectangles = (List<List<Rectangle>>)result[packedOutputPort];
            Assert.AreEqual(1, packedRectangles.Count);
            Assert.AreEqual(4, packedRectangles.First().Count);

            var remainItems = (List<Rectangle>)result[remainOutputPort];
            Assert.AreEqual(2, remainItems.Count);

            var packedIndices = (List<List<int>>)result[indicesOutputPort];
            CollectionAssert.AreEqual(packedIndices, expectedIndices);
        }

        [Test]
        public void MultiBin_CanPackAndHaveLeftovers()
        {
            // Arrange
            var bins = new List<Rectangle> {
                Rectangle.ByWidthLength(30, 30),
                Rectangle.ByWidthLength(30, 30),
                Rectangle.ByWidthLength(30, 30),
            };
            var items = new List<Rectangle>
            {
                Rectangle.ByWidthLength(20,23),
                Rectangle.ByWidthLength(12,30),
                Rectangle.ByWidthLength(33,20),
                Rectangle.ByWidthLength(32,31),
                Rectangle.ByWidthLength(12,22),
                Rectangle.ByWidthLength(15,15)
            };
            var expectedIndices = new List<List<int>>{
                new List<int>{0},
                new List<int>{1, 4},
                new List<int>{5 }
            };

            // Act
            Dictionary<string, object> result = BinPacking.Pack2D(items, bins, RectanglePackingStrategy.BestShortSideFits);

            // Assert
            Assert.IsTrue(result.Keys.Contains(packedOutputPort));
            Assert.IsTrue(result.Keys.Contains(remainOutputPort));
            Assert.IsTrue(result.Keys.Contains(indicesOutputPort));

            var packedRectangles = (List<List<Rectangle>>)result[packedOutputPort];
            Assert.AreEqual(3, packedRectangles.Count);
            Assert.AreEqual(4, packedRectangles.Sum(x => x.Count));

            var remainItems = (List<Rectangle>)result[remainOutputPort];
            Assert.AreEqual(2, remainItems.Count);

            var packedIndices = (List<List<int>>)result[indicesOutputPort];
            Assert.AreEqual(packedIndices, expectedIndices);
        }

        [Test]
        /// <summary>
        /// Checks that the packing process can be run several times without exceptions.
        /// </summary>
        public void MultiBin_PackingCanRunSeveralTimes()
        {
            // Arrange
            var bins = new List<Rectangle> {
                Rectangle.ByWidthLength(30, 30),
                Rectangle.ByWidthLength(30, 30),
                Rectangle.ByWidthLength(30, 30),
            };
            var bins2 = new List<Rectangle> {
                Rectangle.ByWidthLength(40, 40),
                Rectangle.ByWidthLength(40, 40),
                Rectangle.ByWidthLength(40, 40),
            };
            var items = new List<Rectangle>
            {
                Rectangle.ByWidthLength(20,23),
                Rectangle.ByWidthLength(12,30),
                Rectangle.ByWidthLength(33,20),
                Rectangle.ByWidthLength(32,31),
                Rectangle.ByWidthLength(12,22),
                Rectangle.ByWidthLength(15,15)
            };

            // Act
            Dictionary<string, object> result = BinPacking.Pack2D(items, bins, RectanglePackingStrategy.BestShortSideFits);
            Dictionary<string, object> result2 = BinPacking.Pack2D(items, bins2, RectanglePackingStrategy.BestShortSideFits);
            Dictionary<string, object> result3 = BinPacking.Pack2D(items, bins, RectanglePackingStrategy.BestShortSideFits);

            // Assert
            Assert.IsTrue(result3.Keys.Contains(packedOutputPort));
            Assert.IsTrue(result3.Keys.Contains(remainOutputPort));
            Assert.IsTrue(result3.Keys.Contains(indicesOutputPort));

            var packedRectangles = (List<List<Rectangle>>)result3[packedOutputPort];
            Assert.AreEqual(3, packedRectangles.Count);
            Assert.AreEqual(4, packedRectangles.Sum(x => x.Count));

            var remainItems = (List<Rectangle>)result3[remainOutputPort];
            Assert.AreEqual(2, remainItems.Count);
        }

    }
}