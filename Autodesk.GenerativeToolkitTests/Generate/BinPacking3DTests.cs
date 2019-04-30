using NUnit.Framework;
using Autodesk.GenerativeToolkit.Generate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestServices;
using Autodesk.DesignScript.Geometry;

namespace Autodesk.GenerativeToolkit.Generate.Tests
{
    [TestFixture()]
    public class BinPacking3DTests : GeometricTestBase
    {
        [Test()]
        public void PackTest()
        {
            Cuboid bin = Cuboid.ByLengths(400, 500, 300);
            List<Cuboid> items = new List<Cuboid>
            {
                Cuboid.ByLengths(100, 200, 100),
                Cuboid.ByLengths(200, 200, 250),
                Cuboid.ByLengths(100, 150, 300),
                Cuboid.ByLengths(300, 200, 350),
                Cuboid.ByLengths(150, 200, 500),
                Cuboid.ByLengths(200, 100, 100),
                Cuboid.ByLengths(100, 150, 200),
                Cuboid.ByLengths(200, 150, 200),
                Cuboid.ByLengths(250, 200, 300)
            };

            // Check if the result is a dictionary that contains the keys
            // "packedItems", "remainItems" and "packedIndices"
            var result = BinPacking3D.Pack(bin, items);
            Assert.IsTrue(result.Keys.Contains("packedItems"));
            Assert.IsTrue(result.Keys.Contains("remainItems"));
            Assert.IsTrue(result.Keys.Contains("packedIndices"));

            // Checks if the right amount of items has been packed
            var packeditems = (List<Cuboid>)result["packedItems"];
            Assert.AreEqual(5, packeditems.Count);

            var remainItems = (List<Cuboid>)result["remainItems"];
            Assert.AreEqual(4, remainItems.Count);

            // Checks that the right items has been packed
            var packedIndices = (List<int>)result["packedIndices"];
            Assert.AreEqual(packedIndices, new List<int> { 3, 7, 6, 8, 1 });
        }
    }
}