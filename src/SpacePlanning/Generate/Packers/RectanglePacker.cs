using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Autodesk.RefineryToolkits.SpacePlanning.Generate.Packers
{
    /// <summary>
    /// Represents an algorithm capable of packing rectangles inside an outer rectangle.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class RectanglePacker : IPacker<Rectangle, Rectangle>
    {
        #region Properties & constants
        private const RectanglePackingStrategy DEFAULT_PACKING_STRATEGY = RectanglePackingStrategy.BestAreaFits;
        private List<FreeRectangle> FreeRectangles;

        /// <summary>
        /// The rectangles that have been packed into the bin.
        /// </summary>
        public List<Rectangle> PackedItems { get; }

        /// <summary>
        /// The rectangles that could not be packed and are outside the bin.
        /// </summary>
        public List<int> RemainingIndices { get; }

        /// <summary>
        /// The indices of the rectangles that have been packed from the input list of rectangles.
        /// </summary>
        public List<int> PackedIndices { get; }
        #endregion

        #region Constructors

        /// <summary>
        /// Construct an empty 2D bin packer
        /// </summary>
        public RectanglePacker()
        {
            this.FreeRectangles = new List<FreeRectangle>();
            this.PackedItems = new List<Rectangle>();
            this.RemainingIndices = new List<int>();
            this.PackedIndices = new List<int>();
        }

        /// <summary>
        /// Construct a 2D bin packer for the specified bin
        /// </summary>
        /// <param name="bin">The rectangle to use as a bin when packing.</param>
        public RectanglePacker(Rectangle bin) : this()
        {
            this.InitialiseBinFromRectangle(bin);
        }

        #endregion

        #region Packing methods

        /// <summary>
        /// Pack a list of supplied rectangles into the supplied bin.
        /// </summary>
        /// <param name="items">The rectangles to pack.</param>
        /// <param name="container">The bin to pack into, clearing any previously initialised bin.</param>
        public void PackOneContainer(
            List<Rectangle> items,
            Rectangle container)
        {
            this.PackOneContainer(items, container, DEFAULT_PACKING_STRATEGY);
        }

        /// <summary>
        /// Pack a list of supplied rectangles into the supplied bin.
        /// </summary>
        /// <param name="items">The rectangles to pack.</param>
        /// <param name="container">The container to pack into, clearing any previously initialised bin.</param>
        /// <param name="packingStrategy">The method to use when packing.</param>
        public void PackOneContainer(
            List<Rectangle> items,
            Rectangle container,
            RectanglePackingStrategy packingStrategy)
        {
            this.InitialiseBinFromRectangle(container);

            if (this.FreeRectangles.Count == 0) throw new InvalidOperationException("Bin has not been initialised");
            if (items.Count == 0) throw new ArgumentNullException(nameof(items));

            for (int i = 0; i < items.Count; i++)
            {
                // skip already placed rectangles
                var rect = items[i];
                if (rect == null) continue;

                var placed = this.PlaceItem(rect, packingStrategy, i);
                if (placed) items[i] = null;
            }
        }

        /// <summary>
        /// Pack a list of supplied rectangles into the supplied containers.
        /// </summary>
        /// <param name="items">The rectangles to pack.</param>
        /// <param name="containers">The containers to pack into, clearing any previously initialised bin.</param>
        /// <returns>The list of packing results for each container.</returns>
        public List<IPacker<Rectangle, Rectangle>> PackMultipleContainers(
            List<Rectangle> items,
            List<Rectangle> containers)
        {
            return this.PackMultipleContainers(items, containers, DEFAULT_PACKING_STRATEGY);
        }

        /// <summary>
        /// Pack a list of supplied rectangles into the supplied containers.
        /// </summary>
        /// <param name="items">The rectangles to pack.</param>
        /// <param name="containers">The containers to pack into, clearing any previously initialised bin.</param>
        /// <param name="packingStrategy">The method to use when packing.</param>
        /// <returns>The list of packing results for each container.</returns>
        public List<IPacker<Rectangle,Rectangle>> PackMultipleContainers(
            List<Rectangle> items,
            List<Rectangle> containers,
            RectanglePackingStrategy packingStrategy)
        {
            // we need to keep track of packed items across containers
            // and then aggregate results, hence the lists external to BinPacker object
            var remainingRects = new List<Rectangle>(items);
            var packers = new List<IPacker<Rectangle, Rectangle>>();

            for (var i = 0; i < containers.Count; i++)
            {
                var packer = new RectanglePacker();

                packer.PackOneContainer(remainingRects, containers[i], packingStrategy);
                packers.Add(packer as IPacker<Rectangle,Rectangle>);
            }
            return packers;
        }

        #endregion

        #region Packing helper methods

        private void InitialiseBinFromRectangle(Rectangle bin)
        {
            this.FreeRectangles.Add(new FreeRectangle
            {
                width = bin.Width,
                height = bin.Height,
                xPos = bin.StartPoint.X - bin.Width,
                yPos = bin.StartPoint.Y
            });
        }

        /// <summary>
        /// Find best free rectangle and place next rectangle
        /// </summary>
        /// <param name="item"></param>
        /// <param name="placementMethod"></param>
        /// <param name="itemIndex"></param>
        private bool PlaceItem(
            Rectangle item,
            RectanglePackingStrategy placementMethod,
            int itemIndex)
        {
            FreeRectangle freeRect = this.GetBestFreeRectangle(item, placementMethod);

            if (freeRect == null)
            {
                this.RemainingIndices.Add(itemIndex);
                return false;
            }

            // translate rectangle to correct orientation and position
            if (freeRect.rotate) item = Rotate(item);
            var newCS = CoordinateSystem.ByOrigin(freeRect.xPos, freeRect.yPos);
            var originCS = CoordinateSystem.ByOrigin(item.StartPoint.X - item.Width, item.StartPoint.Y);
            var placedRect = (Rectangle)item.Transform(originCS, newCS);

            // place rectangle and update 
            this.PackedItems.Add(placedRect);
            this.PackedIndices.Add(itemIndex);
            this.FreeRectangles.Remove(freeRect);

            // update remaining free space
            this.SplitFreeRectangle(freeRect, placedRect);

            List<double> itemBounds = RectBounds(placedRect);
            this.RemoveOverlaps(itemBounds);

            // Dispose Dynamo geometry
            newCS.Dispose();
            originCS.Dispose();

            return true;
        }

        /// <summary>
        /// Chooses the best free rectangle for an item based on the placement method. 
        /// Rectangles are formed from the leftover free space in the bin.
        /// </summary>
        /// <param name="item">The rectangle to place</param>
        /// <param name="placementMethod">The placement method</param>
        /// <param name="this">The bin packing result to investigate.</param>
        /// <returns>The free rectangle with best score</returns>
        private FreeRectangle GetBestFreeRectangle(
            Rectangle item,
            RectanglePackingStrategy placementMethod)
        {
            var freeRectangles = new List<FreeRectangle>();
            for (int i = 0; i < this.FreeRectangles.Count; i++)
            {
                var fRect = this.FreeRectangles[i];

                FreeRectangle chosenFreeRect;
                var fitsItem = new List<FreeRectangle>
                {
                    ScoreRectanglePlacementInBin(item, placementMethod, fRect, true),
                    ScoreRectanglePlacementInBin(item, placementMethod, fRect, false)
                };
                fitsItem.RemoveAll(x => x == null);

                if (fitsItem.Count == 1)
                {
                    chosenFreeRect = fitsItem[0];
                    fRect.score = chosenFreeRect.score;
                    fRect.rotate = chosenFreeRect.rotate;
                    freeRectangles.Add(fRect);
                }
                else if (fitsItem.Count == 2)
                {
                    // Choose free rect with smallest score
                    chosenFreeRect = fitsItem.Aggregate((f1, f2) => f1.score < f2.score ? f1 : f2);
                    fRect.score = chosenFreeRect.score;
                    fRect.rotate = chosenFreeRect.rotate;
                    freeRectangles.Add(fRect);
                }
            }
            if (freeRectangles.Count > 0)
            {
                // Choose free rect with smallest score
                return freeRectangles.Aggregate((f1, f2) => f1.score < f2.score ? f1 : f2);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Split the free area into free rectangles
        /// </summary>
        /// <param name="fRect"></param>
        /// <param name="item"></param>
        private void SplitFreeRectangle(
            FreeRectangle fRect,
            Rectangle item)
        {
            if (item.Width < fRect.width)
            {
                var fW = fRect.width - item.Width;
                var fH = fRect.height;
                var fX = fRect.xPos + item.Width;
                var fY = fRect.yPos;
                this.FreeRectangles.Add(new FreeRectangle
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
                this.FreeRectangles.Add(new FreeRectangle
                {
                    width = fW,
                    height = fH,
                    xPos = fX,
                    yPos = fY
                });
            }
        }

        /// <summary>
        /// If FreeRectangle is fully encapsulated, remove it.
        /// </summary>
        private void RemoveEncapsulated()
        {
            for (var i = 0; i < this.FreeRectangles.Count; i++)
            {
                for (var j = i + 1; j < this.FreeRectangles.Count; j++)
                {
                    if (IsEncapsulated(this.FreeRectangles[j], this.FreeRectangles[i]))
                    {
                        this.FreeRectangles.Remove(this.FreeRectangles[i]);
                        i--;
                        break;
                    }
                    if (IsEncapsulated(this.FreeRectangles[i], this.FreeRectangles[j]))
                    {
                        this.FreeRectangles.Remove(this.FreeRectangles[j]);
                        j--;
                    }
                }
            }
        }

        /// <summary>
        /// Remove overlap of free areas
        /// </summary>
        /// <param name="itemBounds"></param>
        private void RemoveOverlaps(List<double> itemBounds)
        {
            var freeRects = new List<FreeRectangle>();
            foreach (FreeRectangle rect in this.FreeRectangles)
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
            this.FreeRectangles = freeRects;
            this.RemoveEncapsulated();
        }

        private static FreeRectangle ScoreRectanglePlacementInBin(Rectangle item, RectanglePackingStrategy placementMethod, FreeRectangle fRect, bool rotate)
        {
            if (!ItemFits(fRect, item, rotate)) return null;

            // TODO : we're doing un-necessary allocations in some cases, duplicating the fRect
            var newFree = new FreeRectangle
            {
                xPos = fRect.xPos,
                yPos = fRect.yPos,
                height = fRect.height,
                width = fRect.width,
                rotate = rotate,
            };
            newFree.score = Score(newFree, item, placementMethod);

            return newFree;
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
                    width = F1x + f1.width - overlapTopRightX,
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
                    height = F1y + f1.height - overlapTopRightY,
                    xPos = F1x,
                    yPos = overlapTopRightY
                });
            }

            return newFreeRects;
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
            if (!rotate && rectangle.Width <= f.width && rectangle.Height <= f.height)
            {
                return true;
            }
            if (rotate && rectangle.Height <= f.width && rectangle.Width <= f.height)
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
        /// check if FreeRectangle is fully incapsulated in another
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <returns>boolean</returns>
        private static bool IsEncapsulated(
            FreeRectangle f1,
            FreeRectangle f2)
        {
            const int precision = 2;
            if (Math.Round(f2.xPos, precision) < Math.Round(f1.xPos, precision) || Math.Round(f2.xPos, precision) > Math.Round(f1.xPos + f1.width, precision))
            {
                return false;
            }
            if (Math.Round(f2.xPos + f2.width, precision) > Math.Round(f1.xPos + f1.width, precision))
            {
                return false;
            }
            if (Math.Round(f2.yPos, precision) < Math.Round(f1.yPos, precision) || Math.Round(f2.yPos, precision) > Math.Round(f1.yPos + f1.height, precision))
            {
                return false;
            }
            if (Math.Round(f2.yPos + f2.height, precision) > Math.Round(f1.yPos + f1.height, precision))
            {
                return false;
            }
            return true;
        }

        #endregion

        #region Scoring methods

        private static double Score(
            FreeRectangle freeRect,
            Rectangle item,
            RectanglePackingStrategy placementMethod)
        {
            if (placementMethod == RectanglePackingStrategy.BestShortSideFits)
            {
                return BSSF_Score(freeRect, item);
            }
            else if (placementMethod == RectanglePackingStrategy.BestLongSideFits)
            {
                return BLSF_Score(freeRect, item);
            }
            else if (placementMethod == RectanglePackingStrategy.BestAreaFits)
            {
                return BAF_Score(freeRect, item);
            }
            return 0;
        }

        /// <summary>
        /// Scoring Method for best short side fits
        /// </summary>
        /// <param name="freeRect"></param>
        /// <param name="item"></param>
        /// <returns>Score of Best Short Side Fits</returns>
        private static double BSSF_Score(
            FreeRectangle freeRect,
            Rectangle item)
        {
            double widthDifference;
            double heightDifference;
            if (freeRect.rotate)
            {
                widthDifference = freeRect.width - item.Height;
                heightDifference = freeRect.height - item.Width;
            }
            else
            {
                widthDifference = freeRect.width - item.Width;
                heightDifference = freeRect.height - item.Height;
            }

            return new List<double> { widthDifference, heightDifference }.Min();
        }

        /// <summary>
        /// Scoring Method for Best Long Side Fits
        /// </summary>
        /// <param name="freeRect"></param>
        /// <param name="item"></param>
        /// <returns>Score of Best Long Side Fits</returns>
        private static double BLSF_Score(
            FreeRectangle freeRect,
            Rectangle item)
        {
            double widthDifference;
            double heightDifference;
            if (freeRect.rotate)
            {
                widthDifference = freeRect.width - item.Height;
                heightDifference = freeRect.height - item.Width;
            }
            else
            {
                widthDifference = freeRect.width - item.Width;
                heightDifference = freeRect.height - item.Height;
            }

            return new List<double> { widthDifference, heightDifference }.Max();
        }

        /// <summary>
        /// Scoring Method for Best Area Fits
        /// </summary>
        /// <param name="freeRect"></param>
        /// <param name="item"></param>
        /// <returns>Score of Best Area Fits</returns>
        private static double BAF_Score(
            FreeRectangle freeRect,
            Rectangle item)
        {
            var freeFArea = freeRect.area();
            var rectArea = item.Width * item.Height;
            return freeFArea - rectArea;
        }

        #endregion

        #region Helper classes
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
        #endregion
    }

    /// <summary>
    /// Placement methods for packing rectangles in a bin.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public enum RectanglePackingStrategy
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

}
