using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Autodesk.RefineryToolkits.SpacePlanning.Generate
{
    public static partial class BinPacking
    {
        private const string packedItemsOutputPort2D = "Packed Rectangles";
        private const string indicesOutputPort2D = "Packed Indices";
        private const string remainingItemsOutputPort2D = "Remaining Rectangles";

        #region Private classes

        private class FreeRectangle
        {
            public double width;
            public double height;
            public double xPos;
            public double yPos;
            public double score;
            public bool rotate;
            public double area()
            {
                return this.width * this.height;
            }
        }

        private class PackResult
        {
            public List<FreeRectangle> FreeRectangles;
            public List<Rectangle> PackedRectangles;
            public List<Rectangle> RemainRectangles;
            public List<int> PackedIndices;

            public PackResult()
            {
                this.FreeRectangles = new List<FreeRectangle>();
                this.PackedRectangles = new List<Rectangle>();
                this.RemainRectangles = new List<Rectangle>();
                this.PackedIndices = new List<int>();
            }
        }

        #endregion

        #region Public Methods

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
            // and then aggregate results, hence the lists external to PackResult objects
            var remainingRects = new List<Rectangle>(rects);
            var packedRects = new List<List<Rectangle>>();
            var packIndices = new List<List<int>>();

            for (var i = 0; i < bins.Count; i++)
            {
                Rectangle bin = bins[i];

                PackResult packResult = PackRectanglesInBin(remainingRects, bin, placementMethod);
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

        /// <summary>
        /// Placement methods for packing rectangles in a bin.
        /// </summary>
        [IsVisibleInDynamoLibrary(false)]
        public enum PlacementMethods
        {
            /// <summary>
            /// Best Short Side Fits:
            /// Packs next rectangle into the free area where the length of the longer leftover side is minimized. 
            /// </summary>
            BestShortSideFits,

            /// <summary>
            /// Best Long Side Fits:
            /// Packs next rectangle into the free area where the length of the shorter leftover side is minimized.  
            /// </summary>
            BestLongSideFits,

            /// <summary>
            /// Best Area Fits:
            /// Picks the free area that is smallest in area to place the next rectangle into.
            /// </summary>
            BestAreaFits
        }

        #endregion

        #region Private Methods
        private static PackResult PackRectanglesInBin(
            List<Rectangle> rects,
            Rectangle bin,
            PlacementMethods placementMethod)
        {
            var packResult = new PackResult();

            // Initialize freeRectangle
            packResult.FreeRectangles.Add(new FreeRectangle
            {
                width = bin.Width,
                height = bin.Height,
                xPos = bin.StartPoint.X - bin.Width,
                yPos = bin.StartPoint.Y
            });

            var idx = 0;
            foreach (Rectangle rect in rects)
            {
                PlaceItem(packResult, rect, placementMethod, idx);
                idx++;
            }

            return packResult;
        }

        /// <summary>
        /// Find best freerectangle and place next rectangle
        /// </summary>
        /// <param name="item"></param>
        /// <param name="placementMethod"></param>
        /// <param name="idx"></param>
        private static PackResult PlaceItem(
            PackResult packResult,
            Rectangle item,
            PlacementMethods placementMethod,
            int idx)
        {
            FreeRectangle f = BestFreeRect(item, placementMethod, packResult);

            if (f != null)
            {
                if (f.rotate)
                {
                    item = Rotate(item);
                }
                var newCS = CoordinateSystem.ByOrigin(f.xPos, f.yPos);
                var originCS = CoordinateSystem.ByOrigin(item.StartPoint.X - item.Width, item.StartPoint.Y);
                var placedRect = (Rectangle)item.Transform(originCS, newCS);
                packResult.PackedRectangles.Add(placedRect);
                SplitFreeRectangle(packResult, f, placedRect);
                packResult.PackedIndices.Add(idx);
                packResult.FreeRectangles.Remove(f);

                List<double> itemBounds = RectBounds(placedRect);
                RemoveOverlaps(packResult, itemBounds);

                // Dispose Dynamo geometry
                newCS.Dispose();
                originCS.Dispose();
            }
            else
            {
                packResult.RemainRectangles.Add(item);
            }
            return packResult;
        }

        /// <summary>
        /// Chooses the best free rectangle for an item based on the placement method. 
        /// Rectangles are formed from the leftover free space in the bin.
        /// </summary>
        /// <param name="item">The rectangle to place</param>
        /// <param name="placementMethod">The placement method</param>
        /// <param name="packResult">The bin packing result to investigate.</param>
        /// <returns>The free rectangle with best score</returns>
        private static FreeRectangle BestFreeRect(
            Rectangle item,
            PlacementMethods placementMethod,
            PackResult packResult)
        {
            var fRects = new List<FreeRectangle>();
            foreach (FreeRectangle fRect in packResult.FreeRectangles)
            {
                FreeRectangle chosenFreeRect;
                var fitsItem = new List<FreeRectangle>();
                if (ItemFits(fRect, item, false))
                {
                    var newFree = new FreeRectangle
                    {
                        xPos = fRect.xPos,
                        yPos = fRect.yPos,
                        height = fRect.height,
                        width = fRect.width,
                        rotate = false,
                    };
                    if (placementMethod == PlacementMethods.BestShortSideFits)
                    {
                        newFree.score = BSSF_Score(newFree, item);
                    }
                    else if (placementMethod == PlacementMethods.BestLongSideFits)
                    {
                        newFree.score = BLSF_Score(newFree, item);
                    }
                    else if (placementMethod == PlacementMethods.BestAreaFits)
                    {
                        newFree.score = BAF_Score(newFree, item);
                    }
                    fitsItem.Add(newFree);
                }
                if (ItemFits(fRect, item, true))
                {
                    var newFree = new FreeRectangle
                    {
                        xPos = fRect.xPos,
                        yPos = fRect.yPos,
                        height = fRect.height,
                        width = fRect.width,
                        rotate = true,
                    };
                    if (placementMethod == PlacementMethods.BestShortSideFits)
                    {
                        newFree.score = BSSF_Score(newFree, item);
                    }
                    else if (placementMethod == PlacementMethods.BestLongSideFits)
                    {
                        newFree.score = BLSF_Score(newFree, item);
                    }
                    else if (placementMethod == PlacementMethods.BestAreaFits)
                    {
                        newFree.score = BAF_Score(newFree, item);
                    }
                    fitsItem.Add(newFree);

                }
                if (fitsItem.Count == 1)
                {
                    chosenFreeRect = fitsItem[0];
                    fRect.score = chosenFreeRect.score;
                    fRect.rotate = chosenFreeRect.rotate;
                    fRects.Add(fRect);
                }
                else if (fitsItem.Count == 2)
                {
                    // Choose free rect with smallest score
                    chosenFreeRect = fitsItem.Aggregate((f1, f2) => f1.score < f2.score ? f1 : f2);
                    fRect.score = chosenFreeRect.score;
                    fRect.rotate = chosenFreeRect.rotate;
                    fRects.Add(fRect);

                }
                else
                {
                    continue;
                }

            }
            if (fRects.Any())
            {
                // Choose free rect with smallest score
                return fRects.Aggregate((f1, f2) => f1.score < f2.score ? f1 : f2);

            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// Checks if the rectangle fits in the freeRectangle
        /// </summary>
        /// <param name="f"></param>
        /// <param name="rectangle"></param>
        /// <param name="rotate"></param>
        /// <returns>boolean</returns>
        private static bool ItemFits(
            FreeRectangle f,
            Rectangle rectangle,
            bool rotate)
        {
            if (rotate == false && rectangle.Width <= f.width && rectangle.Height <= f.height)
            {
                return true;
            }
            if (rotate == true && rectangle.Height <= f.width && rectangle.Width <= f.height)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Rotates the item rectangle
        /// </summary>
        /// <param name="item"></param>
        /// <returns>rotated rectangle</returns>
        private static Rectangle Rotate(Rectangle item)
        {
            return Rectangle.ByWidthLength(item.Height, item.Width);
        }

        /// <summary>
        /// Scoring Method for best short side fits
        /// </summary>
        /// <param name="f"></param>
        /// <param name="item"></param>
        /// <returns>Score of Best Short Side Fits</returns>
        private static double BSSF_Score(
            FreeRectangle f,
            Rectangle item)
        {
            double widthDifference;
            double heightDifference;
            if (f.rotate)
            {
                widthDifference = f.width - item.Height;
                heightDifference = f.height - item.Width;
            }
            else
            {
                widthDifference = f.width - item.Width;
                heightDifference = f.height - item.Height;
            }

            return new List<double> { widthDifference, heightDifference }.Min();
        }

        /// <summary>
        /// Scoring Method for Best Long Side Fits
        /// </summary>
        /// <param name="f"></param>
        /// <param name="item"></param>
        /// <returns>Score of Best Long Side Fits</returns>
        private static double BLSF_Score(
            FreeRectangle f,
            Rectangle item)
        {
            double widthDifference;
            double heightDifference;
            if (f.rotate)
            {
                widthDifference = f.width - item.Height;
                heightDifference = f.height - item.Width;
            }
            else
            {
                widthDifference = f.width - item.Width;
                heightDifference = f.height - item.Height;
            }

            return new List<double> { widthDifference, heightDifference }.Max();
        }

        /// <summary>
        /// Scoring Method for Best Area Fits
        /// </summary>
        /// <param name="f"></param>
        /// <param name="item"></param>
        /// <returns>Score of Best Area Fits</returns>
        private static double BAF_Score(
            FreeRectangle f,
            Rectangle item)
        {
            var freeFArea = f.area();
            var rectArea = item.Width * item.Height;
            return freeFArea - rectArea;
        }

        /// <summary>
        /// Split the free area into free rectangles
        /// </summary>
        /// <param name="fRect"></param>
        /// <param name="item"></param>
        private static void SplitFreeRectangle(
            PackResult packResult,
            FreeRectangle fRect,
            Rectangle item)
        {
            if (item.Width < fRect.width)
            {
                var fW = fRect.width - item.Width;
                var fH = fRect.height;
                var fX = fRect.xPos + item.Width;
                var fY = fRect.yPos;
                packResult.FreeRectangles.Add(new FreeRectangle
                {
                    width = fW,
                    height = fH,
                    xPos = fX,
                    yPos = fY
                });
            }
            if (item.Height < fRect.height)
            {
                var fW = fRect.width;
                var fH = fRect.height - item.Height;
                var fX = fRect.xPos;
                var fY = fRect.yPos + item.Height;
                packResult.FreeRectangles.Add(new FreeRectangle
                {
                    width = fW,
                    height = fH,
                    xPos = fX,
                    yPos = fY
                });
            }
        }

        /// <summary>
        /// Gets the boundary points of a rectangle
        /// </summary>
        /// <param name="rect"></param>
        /// <returns>rectangle boundary points</returns>
        private static List<double> RectBounds(Rectangle rect)
        {
            var BottomLeftX = rect.StartPoint.X - rect.Width;
            var BottomLeftY = rect.StartPoint.Y;
            var TopRightX = rect.StartPoint.X;
            var TopRightY = rect.StartPoint.Y + rect.Height;
            return new List<double> { BottomLeftX, BottomLeftY, TopRightX, TopRightY };
        }

        /// <summary>
        /// Remove overlap of free areas
        /// </summary>
        /// <param name="itemBounds"></param>
        private static void RemoveOverlaps(PackResult packResult, List<double> itemBounds)
        {
            var freeRects = new List<FreeRectangle>();
            foreach (var rect in packResult.FreeRectangles)
            {
                if (RectangleOverlaps(rect, itemBounds))
                {
                    List<double> overlapBound = OverlapBound(rect, itemBounds);
                    List<FreeRectangle> newRects = ClipOverlap(rect, overlapBound);
                    freeRects.AddRange(newRects);
                }
                else
                {
                    freeRects.Add(rect);
                }
            }
            packResult.FreeRectangles = freeRects;
            RemoveEncapsulated(packResult);
        }

        /// <summary>
        /// Checks if two rectangles overlaps
        /// https://www.geeksforgeeks.org/find-two-rectangles-overlap/
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="rectBounds"></param>
        /// <returns>boolean</returns>
        private static bool RectangleOverlaps(
            FreeRectangle f1,
            List<double> rectBounds)
        {
            var f1BottomLeftX = f1.xPos;
            var f1BottomLeftY = f1.yPos;
            var f1TopRightX = f1.xPos + f1.width;
            var f1TopRightY = f1.yPos + f1.height;

            // If one rectangle is on left side of other  
            if (f1TopRightY <= rectBounds[1] || f1BottomLeftY >= rectBounds[3])
            {
                return false;
            }

            // If one rectangle is above other  
            if (f1TopRightX <= rectBounds[0] || f1BottomLeftX >= rectBounds[2])
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get the min/max points of the overlap BoundingBox
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="rectBounds"></param>
        /// <returns>overlapping boundary</returns>
        private static List<double> OverlapBound(
            FreeRectangle f1,
            List<double> rectBounds)
        {
            // return bottom left x,y and top left x,y

            var x1 = f1.xPos;
            var y1 = f1.yPos;
            var x2 = f1.xPos + f1.width;
            var y2 = f1.yPos + f1.height;
            var x3 = rectBounds[0];
            var y3 = rectBounds[1];
            var x4 = rectBounds[2];
            var y4 = rectBounds[3];

            var overlapBotLeftX = x1 > x3 ? x1 : x3;
            var overlapBotLeftY = y1 > y3 ? y1 : y3;
            var overlapTopRightX = x2 < x4 ? x2 : x4;
            var overlapTopRightY = y2 < y4 ? y2 : y4;

            return new List<double> { overlapBotLeftX, overlapBotLeftY, overlapTopRightX, overlapTopRightY };
        }

        /// <summary>
        /// Clip overlap of overlapping free areas
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="overlapBound"></param>
        /// <returns>free rectangles after clipping the overlap</returns>
        private static List<FreeRectangle> ClipOverlap(
            FreeRectangle f1,
            List<double> overlapBound)
        {
            var F1x = f1.xPos;
            var F1y = f1.yPos;

            // Bottom left x,y and top right x,y of overlap
            var overlapBotLeftX = overlapBound[0];
            var overlapBotLeftY = overlapBound[1];
            var overlapTopRightX = overlapBound[2];
            var overlapTopRightY = overlapBound[3];

            var newFreeRects = new List<FreeRectangle>();
            // Left side
            if (overlapBotLeftX > F1x)
            {
                newFreeRects.Add(new FreeRectangle
                {
                    width = overlapBotLeftX - F1x,
                    height = f1.height,
                    xPos = F1x,
                    yPos = F1y
                });
            }
            // Right side
            if (overlapTopRightX < F1x + f1.width)
            {
                newFreeRects.Add(new FreeRectangle
                {
                    width = (F1x + f1.width) - overlapTopRightX,
                    height = f1.height,
                    xPos = overlapTopRightX,
                    yPos = F1y
                });
            }
            // Bottom Side
            if (overlapBotLeftY > F1y)
            {
                newFreeRects.Add(new FreeRectangle
                {
                    width = f1.width,
                    height = overlapBotLeftY - F1y,
                    xPos = F1x,
                    yPos = F1y
                });
            }
            // Top Side
            if (overlapTopRightY < F1y + f1.height)
            {
                newFreeRects.Add(new FreeRectangle
                {
                    width = f1.width,
                    height = (F1y + f1.height) - overlapTopRightY,
                    xPos = F1x,
                    yPos = overlapTopRightY
                });
            }
            return newFreeRects;


        }

        /// <summary>
        /// check if FreeRectangle is fully incapsulated in another
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <returns>boolean</returns>
        private static bool IsEncapsulated(
            FreeRectangle f1,
            FreeRectangle f2)
        {
            var precsion = 2;
            if (Math.Round(f2.xPos, precsion) < Math.Round(f1.xPos, precsion) || Math.Round(f2.xPos, precsion) > Math.Round(f1.xPos + f1.width, precsion))
            {
                return false;
            }
            if (Math.Round((f2.xPos + f2.width), precsion) > Math.Round((f1.xPos + f1.width), precsion))
            {
                return false;
            }
            if (Math.Round(f2.yPos, precsion) < Math.Round(f1.yPos, precsion) || Math.Round(f2.yPos, precsion) > Math.Round((f1.yPos + f1.height), precsion))
            {
                return false;
            }
            if (Math.Round((f2.yPos + f2.height), precsion) > Math.Round((f1.yPos + f1.height), precsion))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// If FreeRectangle is fully encapsulated, remove it.
        /// </summary>
        private static void RemoveEncapsulated(PackResult packResult)
        {
            for (var i = 0; i < packResult.FreeRectangles.Count; i++)
            {
                for (var j = i + 1; j < packResult.FreeRectangles.Count; j++)
                {
                    if (IsEncapsulated(packResult.FreeRectangles[j], packResult.FreeRectangles[i]))
                    {
                        packResult.FreeRectangles.Remove(packResult.FreeRectangles[i]);
                        i -= 1;
                        break;
                    }
                    if (IsEncapsulated(packResult.FreeRectangles[i], packResult.FreeRectangles[j]))
                    {
                        packResult.FreeRectangles.Remove(packResult.FreeRectangles[j]);
                        j -= 1;
                    }
                }
            }
        }

        #endregion
    }
}
