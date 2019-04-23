using Autodesk.DesignScript.Geometry;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestServices;

namespace Autodesk.GenerativeToolkit.Generate.Tests
{
    [TestFixture]
    public class BinPacking2DTests : GeometricTestBase
    {
        [Test]
        public void PackTest()
        {
            Rectangle bin = Rectangle.ByWidthLength(50,50);
            List<Rectangle> items = new List<Rectangle>
            {
                Rectangle.ByWidthLength(20,23),
                Rectangle.ByWidthLength(12,30),
                Rectangle.ByWidthLength(33,20),
                Rectangle.ByWidthLength(32,31),
                Rectangle.ByWidthLength(12,22),
                Rectangle.ByWidthLength(15,15)
            };

            var result = BinPacking2D.Pack(items, bin);
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
        public void PlacementPropertyBLSFTest()
        {
            string blsf = BinPacking2D.BLSF;
            Assert.True(blsf == "BLSF");
        }

        [Test]
        public void PlacementPropertyBSSFTest()
        {
            string bssf = BinPacking2D.BSSF;
            Assert.True(bssf == "BSSF");
        }

        [Test]
        public void PlacementPropertyBAFTest()
        {
            string baf = BinPacking2D.BAF;
            Assert.True(baf == "BAF");
        }
    }
}