using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodesk.RefineryToolkits.SpacePlanning.Generate
{
    public static class BinPacking
    {
        private const string packedItemsOutputPort = "Packed Items";
        private const string indicesOutputPort = "Packed Indices";
        private const string remainingItemsOutputPort = "Remaining Items";
        private const string percentContainerVolumePackedPort = "% Container Volume Packed";
        private const string percentItemVolumePackedPort = "% Item Volume Packed";

        /// <summary>
        /// Packs a list of Rectangles in a set of bins (Rectangles too).
        /// Algorithm sequentially packs rectangles in each bin (order as provided) until there is nothing left to pack or it run out of bins.
        /// You can safely use this with a single bin as well.
        /// </summary>
        /// <param name="rectangles">List of rectangles to pack.</param>
        /// <param name="bins">List of rectangles to pack into. </param>
        /// <param name="packingMethod">Method for choosing where to place the next rectangle when packing.</param>
        /// <returns>List of packed rectangles for each of the bins provided, the indeces of rectangles packed and any items that have not been packed.</returns>
        [MultiReturn(new[] { packedItemsOutputPort, indicesOutputPort, remainingItemsOutputPort })]
        public static Dictionary<string, object> PackRectangles(
            List<Rectangle> rectangles,
            List<Rectangle> bins,
            RectanglePackingStrategy packingMethod)
        {
            var packer = new RectanglePacker();
            var results = packer.PackMultipleContainers(rectangles, bins, packingMethod);
            return results.ToDictionary();
        }

        /// <summary>
        /// Packs a sample list of Cuboids in a bin Cuboid
        /// </summary>
        /// <param name="bins">Cuboid to pack sample Cuboids into</param>
        /// <param name="items">List of Cuboids to pack</param>
        /// <returns name="packedItems">Packed Cuboids</returns>
        /// <returns name="packedIndices">Indices of packed items</returns>
        /// <returns name="remainItems">Cubiods that didn't get packed</returns>
        [MultiReturn(new[] { packedItemsOutputPort, indicesOutputPort, remainingItemsOutputPort })]
        public static Dictionary<string, object> PackCuboids(
            List<Cuboid> bins,
            List<Cuboid> items)
        {
            var packer = new CuboidPacker();
            var packingResult = packer.PackMultipleContainersWithStats(bins, items);
            return packingResult.ToDictionary();
        }

        #region Strategy enums as nodes

        /// <summary>
        /// Packs next rectangle into the free area where the length of the longer leftover side is minimized
        /// </summary>
        public static RectanglePackingStrategy RectangleShortSideStrategy => RectanglePackingStrategy.BestShortSideFits;

        /// <summary>
        /// Packs next rectangle into the free area where the length of the shorter leftover side is minimized.  
        /// </summary>
        public static RectanglePackingStrategy RectangleLongSideStrategy => RectanglePackingStrategy.BestLongSideFits;

        /// <summary>
        /// Picks the free area that is smallest in area to place the next rectangle into.
        /// </summary>
        public static RectanglePackingStrategy RectangleAreaStrategy => RectanglePackingStrategy.BestAreaFits;

        #endregion

        #region Dictionary formatters

        /// <summary>
        /// Formats a list of BinPacker2D results to a dictionary for use in Dynamo multi-return nodes 
        /// </summary>
        /// <param name="packers">The list of packing results to convert.</param>
        /// <returns>A dictionary with 3 items: packed, remaining and indices for each BinPacker2D result, as lists.</returns>
        private static Dictionary<string, object> ToDictionary(this List<IPacker<Rectangle,Rectangle>> packers)
        {
            var packedRects = packers.Select(x => x.PackedItems).ToList();
            var packedIndices = packers.Select(x => x.PackedIndices).ToList();

            // we only need the remaining rectangles from the last bin packing result
            // since the same list of remaining rectangles is used sequentially to pack all bins.
            // using all lists of remaining rects would just show us the progression of what remained after each pack
            var remainRects = packers.LastOrDefault()?.RemainingItems;

            return new Dictionary<string, object>
            {
                {packedItemsOutputPort, packedRects},
                {indicesOutputPort, packedIndices},
                {remainingItemsOutputPort, remainRects}
            };
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
                { packedItemsOutputPort, packedCuboids},
                { indicesOutputPort, packedIndices},
                { remainingItemsOutputPort, remainCuboids},
                { percentContainerVolumePackedPort, percentContainerVolumePacked},
                { percentItemVolumePackedPort, percentItemVolumePacked}
            };
        }

        #endregion
    }
}
