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
        /// <param name="rects">List of rectangles to pack.</param>
        /// <param name="bins">List of rectangles to pack into. </param>
        /// <param name="placementMethod">Method for choosing where to place the next rectangle when packing.</param>
        /// <returns>List of packed rectangles for each of the bins provided, the indeces of rectangles packed and any items that have not been packed.</returns>
        [NodeCategory("Create")]
        [MultiReturn(new[] { packedItemsOutputPort2D, indicesOutputPort2D, remainingItemsOutputPort2D })]
        public static Dictionary<string, object> Pack2D(
            List<Rectangle> rects,
            List<Rectangle> bins,
            PlacementMethods placementMethod)
        {
            // we need to keep track of packed items across bins
            // and then aggregate results, hence the lists external to BinPacker objects
            var remainingRects = new List<Rectangle>(rects);
            var packedRects = new List<List<Rectangle>>();
            var packIndices = new List<List<int>>();

            for (var i = 0; i < bins.Count; i++)
            {
                Rectangle bin = bins[i];

                BinPacker2D packResult = PackRectanglesInBin(remainingRects, bin, placementMethod);
                packedRects.Add(packResult.PackedRectangles);
                packIndices.Add(packResult.PackedIndices);

                // update remaining rects
                remainingRects = new List<Rectangle>(packResult.RemainRectangles);
            }

            return new Dictionary<string, object>
            {
                {packedItemsOutputPort2D, packedRects},
                {indicesOutputPort2D, packIndices},
                {remainingItemsOutputPort2D, remainingRects}
            };
        }

        public static PlacementMethods BestShortSideFits => PlacementMethods.BestShortSideFits;
        public static PlacementMethods BestLongSideFits => PlacementMethods.BestLongSideFits;
        public static PlacementMethods BestAreaFits => PlacementMethods.BestAreaFits;
    }
}
