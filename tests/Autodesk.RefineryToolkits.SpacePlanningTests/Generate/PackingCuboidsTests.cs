using Autodesk.DesignScript.Geometry;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TestServices;

namespace Autodesk.RefineryToolkits.SpacePlanning.Generate.Tests
{
    [TestFixture()]
    public partial class PackingTests : GeometricTestBase
    {
        private const string percentContainerVolumePackedPort = "% Container Volume Packed";
        private const string percentItemVolumePackedPort = "% Items Volume Packed";

        private List<string> expectedNodeOutputDictionaryKeys = new List<string>
        {
            packedItemsOutputPort,
            indicesOutputPort,
            remainingIndicesOutputPort,
            percentContainerVolumePackedPort,
            percentItemVolumePackedPort
        };

        [Test()]
        public void Cuboids_OneBin_CanPackWithLeftovers()
        {
            // Arrange
            var bin = new List<Cuboid> {
                Cuboid.ByLengths(400, 500, 300)
            };
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
            var expectedIndices = new List<List<int>> {
                new List<int>{ 3, 7, 6, 8, 1 }
            };
            var expectedPackedContainerVolumes = new List<List<int>> {
                new List<int>{ 3, 7, 6, 8, 1 }
            };
            var expectedPercentageContainerVolumePacked = 91.67;
            var expectedPercentageItemVolumePacked = 70.06;

            // Act
            Dictionary<string, object> result = Packing.PackCuboids(items, bin);

            // Assert
            // Check if the result is a dictionary that contains the expected output port keys
            CollectionAssert.AreEqual(result.Keys, expectedNodeOutputDictionaryKeys);

            // extract results from dictionary
            var actualPackeditems = (List<List<Cuboid>>)result[packedItemsOutputPort];
            var actualRemainItems = (List<int>)result[remainingIndicesOutputPort];
            var actualPackedIndices = (List<List<int>>)result[indicesOutputPort];
            var actualPercentContVol = (result[percentContainerVolumePackedPort] as IEnumerable<double>).First();
            var actualPercentItemVol = (result[percentItemVolumePackedPort] as IEnumerable<double>).First();

            // Checks if the right amount of items has been packed
            Assert.AreEqual(1, actualPackeditems.Count); // the number of bins this was packed into is 1
            Assert.AreEqual(5, actualPackeditems.Sum(x => x.Count)); // total number of packed cuboids is 5
            Assert.AreEqual(4, actualRemainItems.Count);

            // Check the packing percentages are correct
            Assert.AreEqual(expectedPercentageContainerVolumePacked, Math.Round(actualPercentContVol, 2));
            Assert.AreEqual(expectedPercentageItemVolumePacked, Math.Round(actualPercentItemVol, 2));

            // Checks that the right items has been packed
            Assert.AreEqual(1, actualPackedIndices.Count); // the number of bins this was packed into is 1
            CollectionAssert.AreEqual(actualPackedIndices, expectedIndices);
        }

        [Test()]
        public void Cuboids_MultiBin_CanPackWithLeftovers()
        {
            // Arrange
            var bins = new List<Cuboid> {
                Cuboid.ByLengths(100, 100, 200), // holds 2 of items below
                Cuboid.ByLengths(100, 200, 100)  // holds 2 of items below
            };
            var items = new List<Cuboid>();
            for (int i = 0; i < 6; i++)
            {
                items.Add(Cuboid.ByLengths(100, 100, 100));
            }

            var expectedIndices = new List<List<int>> {
                new List<int>{ 0, 1 },
                new List<int>{ 2, 3 }
            };

            // Act
            Dictionary<string, object> result = Packing.PackCuboids(items, bins);

            // Assert
            // Checks if the right amount of items has been packed
            var packeditems = (List<List<Cuboid>>)result[packedItemsOutputPort];
            Assert.AreEqual(2, packeditems.Count); // the number of bins this was packed into is 2
            Assert.AreEqual(4, packeditems.Sum(x => x.Count)); // total number of packed cuboids is 4

            var remainItems = (List<int>)result[remainingIndicesOutputPort];
            Assert.AreEqual(2, remainItems.Count);

            // Checks that the right items has been packed
            var packedIndices = (List<List<int>>)result[indicesOutputPort];
            Assert.AreEqual(2, packedIndices.Count); // the number of bins this was packed into is 2
            CollectionAssert.AreEqual(packedIndices, expectedIndices);
        }

        [Test()]
        public void Cuboids_MultiBin_CanPackWithNoLeftovers()
        {
            // Arrange
            var bins = new List<Cuboid> {
                Cuboid.ByLengths(100, 100, 200), // holds 2 of items below
                Cuboid.ByLengths(100, 200, 100),  // holds 2 of items below
                Cuboid.ByLengths(100, 200, 100),  // would hold 1 of items below, if there were more remaining
            };
            var items = new List<Cuboid>();
            for (int i = 0; i < 4; i++)
            {
                items.Add(Cuboid.ByLengths(100, 100, 100));
            }

            var expectedIndices = new List<List<int>> {
                new List<int>{ 0, 1 },
                new List<int>{ 2, 3 }
            };

            // Act
            Dictionary<string, object> result = Packing.PackCuboids(items, bins);

            // Assert
            // Checks if the right amount of items has been packed
            var packeditems = (List<List<Cuboid>>)result[packedItemsOutputPort];
            Assert.AreEqual(2, packeditems.Count); // the number of bins this was packed into is 2
            Assert.AreEqual(4, packeditems.Sum(x => x.Count)); // total number of packed cuboids is 4

            var remainItems = (List<int>)result[remainingIndicesOutputPort];
            Assert.AreEqual(0, remainItems.Count);

            // Checks that the right items has been packed
            var packedIndices = (List<List<int>>)result[indicesOutputPort];
            Assert.AreEqual(2, packedIndices.Count); // the number of bins this was packed into is 2
            CollectionAssert.AreEqual(packedIndices, expectedIndices);
        }

    }
}