using Autodesk.DesignScript.Geometry;
using Autodesk.RefineryToolkits.SpacePlanning.Generate.Packers;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TestServices;

namespace Autodesk.RefineryToolkits.SpacePlanning.Generate.Tests
{
    [TestFixture]
    public partial class PackingTests
    {
        private const string packedItemsOutputPort = "Packed Items";
        private const string remainingIndicesOutputPort = "Remaining Indices";
        private const string indicesOutputPort = "Packed Indices";

        [Test]
        public void Rectangles_OneBin_CanPackWithNoLeftovers()
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

            var expectedIndices = new List<List<int>>
            {
                new List<int> { 0, 1, 2, 3 }
            };

            // Act
            Dictionary<string, object> result = Packing.PackRectangles(items, bins, RectanglePackingStrategy.BestShortSideFits);

            // Assert
            Assert.IsTrue(result.Keys.Contains(packedItemsOutputPort));
            Assert.IsTrue(result.Keys.Contains(remainingIndicesOutputPort));
            Assert.IsTrue(result.Keys.Contains(indicesOutputPort));

            var packedRectangles = (List<List<Rectangle>>)result[packedItemsOutputPort];
            Assert.AreEqual(1, packedRectangles.Count);
            Assert.AreEqual(4, packedRectangles.First().Count);

            var remainItems = (List<int>)result[remainingIndicesOutputPort];
            Assert.AreEqual(0, remainItems.Count);

            var packedIndices = (List<List<int>>)result[indicesOutputPort];
            CollectionAssert.AreEqual(packedIndices, expectedIndices);
        }

        [Test]
        public void Rectangles_OneBin_CanPackAndHaveLeftovers()
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
            var expectedIndices = new List<List<int>>
            {
                new List<int> { 0, 1, 2, 4 }
            };

            // Act
            Dictionary<string, object> result = Packing.PackRectangles(items, bins, RectanglePackingStrategy.BestShortSideFits);

            // Assert
            Assert.IsTrue(result.Keys.Contains(packedItemsOutputPort));
            Assert.IsTrue(result.Keys.Contains(remainingIndicesOutputPort));
            Assert.IsTrue(result.Keys.Contains(indicesOutputPort));

            var packedRectangles = (List<List<Rectangle>>)result[packedItemsOutputPort];
            Assert.AreEqual(1, packedRectangles.Count);
            Assert.AreEqual(4, packedRectangles.First().Count);

            var remainItems = (List<int>)result[remainingIndicesOutputPort];
            Assert.AreEqual(2, remainItems.Count);

            var packedIndices = (List<List<int>>)result[indicesOutputPort];
            CollectionAssert.AreEqual(packedIndices, expectedIndices);
        }

        [Test]
        public void Rectangles_MultiBin_CanPackAndHaveLeftovers()
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
            Dictionary<string, object> result = Packing.PackRectangles(items, bins, RectanglePackingStrategy.BestShortSideFits);

            // Assert
            Assert.IsTrue(result.Keys.Contains(packedItemsOutputPort));
            Assert.IsTrue(result.Keys.Contains(remainingIndicesOutputPort));
            Assert.IsTrue(result.Keys.Contains(indicesOutputPort));

            var packedRectangles = (List<List<Rectangle>>)result[packedItemsOutputPort];
            Assert.AreEqual(3, packedRectangles.Count); // 3 bins were packed
            Assert.AreEqual(4, packedRectangles.Sum(x => x.Count)); // the number of rectangles packed is 4

            var remainItems = (List<int>)result[remainingIndicesOutputPort];
            Assert.AreEqual(2, remainItems.Count); // 2 rectangles were not packed

            var packedIndices = (List<List<int>>)result[indicesOutputPort];
            CollectionAssert.AreEqual(packedIndices, expectedIndices); // the rectangles were packed in the order and grouping expected
        }

        [Test]
        /// <summary>
        /// Checks that the packing process can be run several times without exceptions.
        /// </summary>
        public void Rectangles_MultiBin_PackingCanRunSeveralTimes()
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
            Dictionary<string, object> result = Packing.PackRectangles(items, bins, RectanglePackingStrategy.BestShortSideFits);
            Dictionary<string, object> result2 = Packing.PackRectangles(items, bins2, RectanglePackingStrategy.BestShortSideFits);
            Dictionary<string, object> result3 = Packing.PackRectangles(items, bins, RectanglePackingStrategy.BestShortSideFits);

            // Assert
            Assert.IsTrue(result3.Keys.Contains(packedItemsOutputPort));
            Assert.IsTrue(result3.Keys.Contains(remainingIndicesOutputPort));
            Assert.IsTrue(result3.Keys.Contains(indicesOutputPort));

            var packedRectangles = (List<List<Rectangle>>)result3[packedItemsOutputPort];
            Assert.AreEqual(3, packedRectangles.Count);
            Assert.AreEqual(4, packedRectangles.Sum(x => x.Count));

            var remainItems = (List<int>)result3[remainingIndicesOutputPort];
            Assert.AreEqual(2, remainItems.Count);
        }
    }
}