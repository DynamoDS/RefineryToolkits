using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using CromulentBisgetti.ContainerPacking.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Autodesk.RefineryToolkits.SpacePlanning.Generate
{
    public static partial class BinPacking
    {
        private const string packedItemsOutputPort3D = "Packed Items";
        private const string indicesOutputPort3D = "Packed Indices";
        private const string remainingItemsOutputPort3D = "Remaining Items";
        private const string percentContainerVolumePackedPort = "% Container Volume Packed";
        private const string percentItemVolumePackedPort = "% Item Volume Packed";

        /// <summary>
        /// Packs a sample list of Cuboids in a bin Cuboid
        /// </summary>
        /// <param name="bins">Cuboid to pack sample Cuboids into</param>
        /// <param name="items">List of Cuboids to pack</param>
        /// <returns name="packedItems">Packed Cuboids</returns>
        /// <returns name="packedIndices">Indices of packed items</returns>
        /// <returns name="remainItems">Cubiods that didn't get packed</returns>
        [MultiReturn(new[] { packedItemsOutputPort3D, indicesOutputPort3D, remainingItemsOutputPort3D })]
        public static Dictionary<string, object> Pack3D(
            List<Cuboid> bins,
            List<Cuboid> items)
        {
            var packer = new CuboidPacker();
            var packingResult = packer.PackItemsInContainers(bins, items);
            return packingResult.ToDictionary();
        }

        /// <summary>
        /// Formats a list of BinPacker3D results to a dictionary for use in Dynamo multi-return nodes 
        /// </summary>
        /// <param name="packers">The list of packing results to convert.</param>
        /// <returns>A dictionary with 3 items: packed, remaining and indices for each BinPacker3D result, as lists.</returns>
        private static Dictionary<string, object> ToDictionary(this List<CuboidPacker> packers)
        {
            var packedCuboids = packers.Select(x => x.PackedItems).ToList();
            var packedIndices = packers.Select(x => x.PackedIndices).ToList();
            var percentContainerVolumePacked = packers.Select(x => x.PercentContainerVolumePacked).ToList();
            var percentItemVolumePacked = packers.Select(x => x.PercentItemVolumePacked).ToList();

            // we only need the remaining rectangles from the last bin packing result
            var remainCuboids = packers.LastOrDefault()?.RemainingItems;

            return new Dictionary<string, object>
            {
                { packedItemsOutputPort3D, packedCuboids},
                { indicesOutputPort3D, packedIndices},
                { remainingItemsOutputPort3D, remainCuboids},
                { percentContainerVolumePackedPort, percentContainerVolumePacked},
                { percentItemVolumePackedPort, percentItemVolumePacked}
            };
        }
    }
}
