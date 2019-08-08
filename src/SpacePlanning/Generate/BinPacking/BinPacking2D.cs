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
    public static partial class BinPacking
    {
        private const string packedItemsOutputPort2D = "Packed Rectangles";
        private const string indicesOutputPort2D = "Packed Indices";
        private const string remainingItemsOutputPort2D = "Remaining Rectangles";

        /// <summary>
        /// Packs a list of Rectangles in a set of bins (Rectangles too).
        /// Algorithm sequentially packs rectangles in each bin (order as provided) until there is nothing left to pack or it run out of bins.
        /// You can safely use this with a single bin as well.
        /// </summary>
        /// <param name="rectangles">List of rectangles to pack.</param>
        /// <param name="bins">List of rectangles to pack into. </param>
        /// <param name="packingMethod">Method for choosing where to place the next rectangle when packing.</param>
        /// <returns>List of packed rectangles for each of the bins provided, the indeces of rectangles packed and any items that have not been packed.</returns>
        [MultiReturn(new[] { packedItemsOutputPort2D, indicesOutputPort2D, remainingItemsOutputPort2D })]
        public static Dictionary<string, object> Pack2D(
            List<Rectangle> rectangles,
            List<Rectangle> bins,
            RectanglePackingStrategy packingMethod)
        {
            //// we need to keep track of packed items across bins
            //// and then aggregate results, hence the lists external to BinPacker objects
            //var remainingRects = new List<Rectangle>(rectangles);
            //var packedRects = new List<List<Rectangle>>();
            //var packIndices = new List<List<int>>();

            //for (var i = 0; i < bins.Count; i++)
            //{
            //    var packer = new BinPacker2D(bins[i]);

            //    packer.PackRectanglesInBin(remainingRects, packingMethod);
            //    packedRects.Add(packer.PackedRectangles);
            //    packIndices.Add(packer.PackedIndices);

            //    // update remaining rects
            //    remainingRects = new List<Rectangle>(packer.RemainRectangles);
            //}

            //return new Dictionary<string, object>
            //{
            //    {packedItemsOutputPort2D, packedRects},
            //    {indicesOutputPort2D, packIndices},
            //    {remainingItemsOutputPort2D, remainingRects}
            //};
            var results = BinPacker2D.PackRectanglesAcrossBins(rectangles, bins, packingMethod);
            return results.ToDictionary();
        }

        /// <summary>
        /// Packs next rectangle into the free area where the length of the longer leftover side is minimized
        /// </summary>
        public static RectanglePackingStrategy BestShortSideFitsStrategy => RectanglePackingStrategy.BestShortSideFits;

        /// <summary>
        /// Packs next rectangle into the free area where the length of the shorter leftover side is minimized.  
        /// </summary>
        public static RectanglePackingStrategy BestLongSideFitsStrategy => RectanglePackingStrategy.BestLongSideFits;

        /// <summary>
        /// Picks the free area that is smallest in area to place the next rectangle into.
        /// </summary>
        public static RectanglePackingStrategy BestAreaFitsStrategy => RectanglePackingStrategy.BestAreaFits;

        /// <summary>
        /// Formats a list of BinPacker2D results to a dictionary for use in Dynamo multi-return nodes 
        /// </summary>
        /// <param name="packers">The list of packing results to convert.</param>
        /// <returns>A dictionary with 3 items: packed, remaining and indices for each BinPacker2D result, as lists.</returns>
        private static Dictionary<string, object> ToDictionary(this List<BinPacker2D> packers)
        {
            var packedRects = packers.Select(x => x.PackedRectangles).ToList();
            var remainRects = packers.Select(x => x.RemainRectangles).ToList();
            var packedIndices = packers.Select(x => x.PackedIndices).ToList();

            return new Dictionary<string, object>
            {
                {packedItemsOutputPort2D, packedRects},
                {indicesOutputPort2D, packedIndices},
                {remainingItemsOutputPort2D, remainRects}
            };
        }

    }
}
