using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.RefineryToolkits.SpacePlanning.Generate.Packers;
using System.Collections.Generic;
using System.Linq;

namespace Autodesk.RefineryToolkits.SpacePlanning.Generate
{
    /// <summary>
    /// Wrapper class for Packing nodes
    /// </summary>
    public static class Packing
    {
        private const string packedItemsOutputPort = "Packed Items";
        private const string indicesOutputPort = "Packed Indices";
        private const string remainingIndicesOutputPort = "Remaining Indices";
        private const string percentContainerVolumePackedPort = "% Container Volume Packed";
        private const string percentItemVolumePackedPort = "% Items Volume Packed";

        /// <summary>
        /// Packs a list of items (Rectangles) into a set of containers (Rectangles too).
        /// Algorithm sequentially packs rectangles in each container (order as provided) until there is nothing left to pack or it run out of bins.
        /// You can safely use this with a single container as well.
        /// </summary>
        /// <param name="items">List of items (rectangles) to pack.</param>
        /// <param name="containers">List of containers (rectangles) to pack into. </param>
        /// <param name="strategy">(optional) Method for choosing where to place the next rectangle when packing.
        /// Possible values are : RectangleShortSideStrategy, RectangleLongSideStrategy, RectangleAreaStrategy</param>
        /// <returns name="Packed Items">List of packed rectangles for each of the containers provided.</returns>
        /// <returns name="Packed Indices">Indices of packed rectangles for correlation to input items list.</returns>
        /// <returns name="Remaining Indices">Indices of items (rectangles) that didn't get packed.</returns>
        [MultiReturn(new[] { packedItemsOutputPort, indicesOutputPort, remainingIndicesOutputPort })]
        public static Dictionary<string, object> PackRectangles(
            List<Rectangle> items,
            List<Rectangle> containers,
            [DefaultArgument("RectanglePackingStrategy.BestAreaFits")] RectanglePackingStrategy strategy)
        {
            var packer = new RectanglePacker();
            var results = packer.PackMultipleContainers(items, containers, strategy);
            return results.ToDictionary();
        }

        /// <summary>
        /// Packs a list of items (Cuboids) into a set of containers (Cuboids too).
        /// When supplying multiple containers, it sequentially packs each until no space is left, before moving on to next container.
        /// </summary>
        /// <param name="items">List of Cuboids to pack</param>
        /// <param name="containers">Set of Cuboids to pack into.</param>
        /// <returns name="Packed Items">The cuboids that were packed.</returns>
        /// <returns name="Packed Indices">Indices of packed cuboids for correlation to input items list.</returns>
        /// <returns name="Remaining Indices">Indices of Cuboids that didn't get packed.</returns>
        /// <returns name="% Container Volume Packed">Metric : percentage of each container volume that was packed.</returns>
        /// <returns name="% Item Volume Packed">Metric : percentage expressing how much of total items volume was packed in each container.</returns>
        [MultiReturn(new[] { packedItemsOutputPort, indicesOutputPort, remainingIndicesOutputPort, percentContainerVolumePackedPort, percentItemVolumePackedPort })]
        public static Dictionary<string, object> PackCuboids(
            List<Cuboid> items,
            List<Cuboid> containers)
        {
            var packer = new CuboidPacker();
            var packingResult = packer.PackMultipleContainersWithStats(items, containers);
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
        private static Dictionary<string, object> ToDictionary(this List<IPacker<Rectangle, Rectangle>> packers)
        {
            var packedRects = packers.Select(x => x.PackedItems).ToList();
            var packedIndices = packers.Select(x => x.PackedIndices).ToList();

            // we only need the remaining rectangles from the last bin packing result
            // since the same list of remaining rectangles is used sequentially to pack all bins.
            // using all lists of remaining rects would just show us the progression of what remained after each pack
            var remainIndices = packers.LastOrDefault()?.RemainingIndices;

            return new Dictionary<string, object>
            {
                {packedItemsOutputPort, packedRects},
                {indicesOutputPort, packedIndices},
                {remainingIndicesOutputPort, remainIndices}
            };
        }

        /// <summary>
        /// Formats a list of BinPacker3D results to a dictionary for use in Dynamo multi-return nodes 
        /// </summary>
        /// <param name="packers">The list of packing results to convert.</param>
        /// <returns>A dictionary with 5 items: packed items, their indices, remaining items and 2 packing performance metrics.</returns>
        private static Dictionary<string, object> ToDictionary(this List<CuboidPacker> packers)
        {
            var packedCuboids = packers.Select(x => x.PackedItems).ToList();
            var packedIndices = packers.Select(x => x.PackedIndices).ToList();
            var percentContainerVolumePacked = packers.Select(x => x.PercentContainerVolumePacked).ToList();
            var percentItemVolumePacked = packers.Select(x => x.PercentItemVolumePacked).ToList();

            // we only need the remaining rectangles from the last bin packing result
            var remainIndices = packers.LastOrDefault()?.RemainingIndices;

            return new Dictionary<string, object>
            {
                { packedItemsOutputPort, packedCuboids},
                { indicesOutputPort, packedIndices},
                { remainingIndicesOutputPort, remainIndices},
                { percentContainerVolumePackedPort, percentContainerVolumePacked},
                { percentItemVolumePackedPort, percentItemVolumePacked}
            };
        }

        #endregion
    }
}
