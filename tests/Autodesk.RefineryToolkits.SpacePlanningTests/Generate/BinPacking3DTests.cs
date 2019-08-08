using Autodesk.DesignScript.Geometry;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TestServices;

namespace Autodesk.RefineryToolkits.SpacePlanning.Generate.Tests
{
    [TestFixture()]
    public class BinPacking3DTests : GeometricTestBase
    {
        const string packedOutputPort = "Packed Items";
        const string remainOutputPort = "Remaining Items";
        const string indicesOutputPort = "Packed Indices";

        [Test()]
        public void PackTest()
        {
            var bin = Cuboid.ByLengths(400, 500, 300);
            var items = new List<Cuboid>
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

            // Check if the result is a dictionary that contains the expected output port keys
            Dictionary<string, object> result = BinPacking.Pack3D(bin, items);
            Assert.IsTrue(result.Keys.Contains(packedOutputPort));
            Assert.IsTrue(result.Keys.Contains(remainOutputPort));
            Assert.IsTrue(result.Keys.Contains(indicesOutputPort));

            // Checks if the right amount of items has been packed
            var packeditems = (List<Cuboid>)result[packedOutputPort];
            Assert.AreEqual(5, packeditems.Count);

            var remainItems = (List<Cuboid>)result[remainOutputPort];
            Assert.AreEqual(4, remainItems.Count);

            // Checks that the right items has been packed
            var packedIndices = (List<int>)result[indicesOutputPort];
            Assert.AreEqual(packedIndices, new List<int> { 3, 7, 6, 8, 1 });
        }
    }
}