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
        [Test]
        public void OneBin_CanPackWithNoLeftovers()
        {
            var bins = new List<Rectangle> { Rectangle.ByWidthLength(50, 50) };
            var items = new List<Rectangle>
            {
                Rectangle.ByWidthLength(20,23),
                Rectangle.ByWidthLength(12,30),
                Rectangle.ByWidthLength(33,20),
                Rectangle.ByWidthLength(12,22)
            };

            Dictionary<string, object> result = BinPacking.Pack2D(items, bins, PlacementMethods.BestShortSideFits);
            Assert.IsTrue(result.Keys.Contains("packedRectangles"));
            Assert.IsTrue(result.Keys.Contains("remainRectangles"));
            Assert.IsTrue(result.Keys.Contains("packedIndices"));

            var packedRectangles = (List<Rectangle>)result["packedRectangles"];
            Assert.AreEqual(4, packedRectangles.Count);

            var remainItems = (List<Rectangle>)result["remainRectangles"];
            Assert.AreEqual(0, remainItems.Count);

            var packedIndices = (List<int>)result["packedIndices"];
            Assert.AreEqual(packedIndices, new List<int> { 0, 1, 2, 3 });
        }

        [Test]
        public void OneBin_CanPackAndHaveLeftovers()
        {
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

            Dictionary<string, object> result = BinPacking.Pack2D(items, bins, PlacementMethods.BestShortSideFits);
            Assert.IsTrue(result.Keys.Contains("packedRectangles"));
            Assert.IsTrue(result.Keys.Contains("remainRectangles"));
            Assert.IsTrue(result.Keys.Contains("packedIndices"));

            var packedRectangles = (List<Rectangle>)result["packedRectangles"];
            Assert.AreEqual(4, packedRectangles.Count);

            var remainItems = (List<Rectangle>)result["remainRectangles"];
            Assert.AreEqual(2, remainItems.Count);

            var packedIndices = (List<int>)result["packedIndices"];
            Assert.AreEqual(packedIndices, new List<int> { 0, 1, 2, 4 });
        }

        [Test]
        public void MultiBin_CanPackAndHaveLeftovers()
        {
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

            Dictionary<string, object> result = BinPacking.Pack2D(items, bins, PlacementMethods.BestShortSideFits);
            Assert.IsTrue(result.Keys.Contains("packedRectangles"));
            Assert.IsTrue(result.Keys.Contains("remainRectangles"));
            Assert.IsTrue(result.Keys.Contains("packedIndices"));

            var packedRectangles = (List<Rectangle>)result["packedRectangles"];
            Assert.AreEqual(6, packedRectangles.Count);

            var remainItems = (List<Rectangle>)result["remainRectangles"];
            Assert.AreEqual(0, remainItems.Count);

            var packedIndices = (List<int>)result["packedIndices"];
            Assert.AreEqual(packedIndices, new List<int> { 0, 1, 2, 3, 4, 5 });
        }
    }
}