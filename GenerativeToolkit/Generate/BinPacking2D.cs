#region namespaces
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace Autodesk.GenerativeToolkit.Generate
{
    public static class BinPacking2D
    {
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
                return width * height;
            }
        }

        #region Private Variables

        private static List<FreeRectangle> freeRectangles;
        private static List<Rectangle> placedRectangles;

        #endregion

        #region Public Methods
        /// <summary>
        /// Packs a sample list of Rectangles in a bin Rectangle
        /// </summary>
        /// <param name="rects"> list of Rectangles to Pack</param>
        /// <param name="bin"> Rectangle to pack into</param>
        /// <param name="placementMethod"> Method for choosing where to place the next rectangle</param>
        /// <returns>List of packed rectangles</returns>
        public static List<Rectangle> Pack(List<Rectangle> rects, Rectangle bin, string placementMethod = "BSSF")
        {
            freeRectangles = new List<FreeRectangle>();
            placedRectangles = new List<Rectangle>();

            // Initialize freeRectangles
            freeRectangles.Add(new FreeRectangle
            {
                width = bin.Width,
                height = bin.Height,
                xPos = bin.StartPoint.X - bin.Width,
                yPos = bin.StartPoint.Y
            });

            foreach (var rect in rects)
            {
                PlaceItem(rect, placementMethod);
            }
            return placedRectangles;
        }

        #region Placement Methods
        /// <summary>
        /// Packs next rectangle into the free area where the length of the longer leftover side is minimized. 
        /// </summary>
        public static string BSSF =>  "BSSF";

        /// <summary>
        /// Packs next rectangle into the free area where the length of the shorter leftover side is minimized.  
        /// </summary>
        public static string BLSF => "BLSF";

        /// <summary>
        /// Picks the free area that is smallest in area to place the next rectangle into.
        /// </summary>
        public static string BAF => "BAF";

        #endregion

        #endregion

        #region Private Methods

        #region Find best freerectangle and place next rectangle

        private static void PlaceItem(Rectangle item, string placementMethod)
        {
            FreeRectangle f = BestFreeRect(item, placementMethod);

            if (f != null)
            {
                if (f.rotate)
                {
                    item = Rotate(item);
                }
                CoordinateSystem newCS = CoordinateSystem.ByOrigin(f.xPos, f.yPos);
                CoordinateSystem originCS = CoordinateSystem.ByOrigin(item.StartPoint.X - item.Width, item.StartPoint.Y);
                Rectangle placedRect = (Rectangle)item.Transform(originCS, newCS);
                placedRectangles.Add(placedRect);
                SplitFreeRectangle(f, placedRect);
                freeRectangles.Remove(f);

                List<double> itemBounds = RectBounds(placedRect);
                RemoveOverlaps(itemBounds);

                // Dispose Dynamo geometry
                newCS.Dispose();
                originCS.Dispose();
            }
        }

        private static FreeRectangle BestFreeRect(Rectangle item, string placementMethod)
        {
            List<FreeRectangle> fRects = new List<FreeRectangle>();
            foreach (var fRect in freeRectangles)
            {
                FreeRectangle chosenFreeRect;
                List<FreeRectangle> fitsItem = new List<FreeRectangle>();
                if (ItemFits(fRect, item, false))
                {
                    FreeRectangle newFree = new FreeRectangle
                    {
                        xPos = fRect.xPos,
                        yPos = fRect.yPos,
                        height = fRect.height,
                        width = fRect.width,
                        rotate = false,
                    };
                    if (placementMethod == "BSSF")
                    {
                        newFree.score = BSSF_Score(newFree, item);
                    }
                    else if (placementMethod == "BLSF")
                    {
                        newFree.score = BLSF_Score(newFree, item);
                    }
                    else if (placementMethod == "BAF")
                    {
                        newFree.score = BAF_Score(newFree, item);
                    }
                    
                    fitsItem.Add(newFree);
                }
                if (ItemFits(fRect, item, true))
                {
                    FreeRectangle newFree = new FreeRectangle
                    {
                        xPos = fRect.xPos,
                        yPos = fRect.yPos,
                        height = fRect.height,
                        width = fRect.width,                      
                        rotate = true,
                    };
                    if (placementMethod == "BSSF")
                    {
                        newFree.score = BSSF_Score(newFree, item);
                    }
                    else if (placementMethod == "BLSF")
                    {
                        newFree.score = BLSF_Score(newFree, item);
                    }
                    else if (placementMethod == "BAF")
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

        private static bool ItemFits(FreeRectangle f, Rectangle rectangle, bool rotate)
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

        private static Rectangle Rotate(Rectangle item)
        {
            return Rectangle.ByWidthLength(item.Height, item.Width);
        }
        #endregion

        #region Scoring Methods

        // Best Short Side Fits
        private static double BSSF_Score(FreeRectangle f, Rectangle item)
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
        // Best Long Side Fits
        private static double BLSF_Score(FreeRectangle f, Rectangle item)
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
        // Best Area Fits
        private static double BAF_Score(FreeRectangle f, Rectangle item)
        {
            double freeFArea = f.area();
            double rectArea = item.Width * item.Height;
            return freeFArea - rectArea;
        }
        #endregion

        #region Split Free area

        private static void SplitFreeRectangle(FreeRectangle fRect, Rectangle item)
        {
            if (item.Width < fRect.width)
            {
                double fW = fRect.width - item.Width;
                double fH = fRect.height;
                double fX = fRect.xPos + item.Width;
                double fY = fRect.yPos;
                freeRectangles.Add(new FreeRectangle
                {
                    width = fW,
                    height = fH,
                    xPos = fX,
                    yPos = fY
                });
            }
            if (item.Height < fRect.height)
            {
                double fW = fRect.width;
                double fH = fRect.height - item.Height;
                double fX = fRect.xPos;
                double fY = fRect.yPos + item.Height;
                freeRectangles.Add(new FreeRectangle
                {
                    width = fW,
                    height = fH,
                    xPos = fX,
                    yPos = fY
                });
            }
        }
        #endregion

        #region Rectangle Boundary points

        private static List<double> RectBounds(Rectangle rect)
        {
            double BottomLeftX = rect.StartPoint.X - rect.Width;
            double BottomLeftY = rect.StartPoint.Y;
            double TopRightX = rect.StartPoint.X;
            double TopRightY = rect.StartPoint.Y + rect.Height;
            return new List<double> { BottomLeftX, BottomLeftY, TopRightX, TopRightY };
        }
        #endregion

        #region Remove overlaps and redundant rectangles

        private static void RemoveOverlaps(List<double> itemBounds)
        {
            List<FreeRectangle> freeRects = new List<FreeRectangle>();
            foreach (var rect in freeRectangles)
            {
                if (RectangleOverlaps(rect, itemBounds))
                {
                    List<double> overlapBound = Overlap_Bound(rect, itemBounds);
                    List<FreeRectangle> newRects = ClipOverlap(rect, overlapBound);
                    freeRects.AddRange(newRects);
                }
                else
                {
                    freeRects.Add(rect);
                }
            }
            freeRectangles = freeRects;
            RemoveEncapsulated();
        }

        // Check if two rectangles overlaps
        // https://www.geeksforgeeks.org/find-two-rectangles-overlap/
        private static bool RectangleOverlaps(FreeRectangle f1, List<double> rectBounds)
        {
            double f1BottomLeftX = f1.xPos;
            double f1BottomLeftY = f1.yPos;
            double f1TopRightX = f1.xPos + f1.width;
            double f1TopRightY = f1.yPos + f1.height;

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

        // Get the min/max points of the overlap BoundingBox
        private static List<double> Overlap_Bound(FreeRectangle f1, List<double> rectBounds)
        {
            // return bottom left x,y and top left x,y

            double x1 = f1.xPos;
            double y1 = f1.yPos;
            double x2 = f1.xPos + f1.width;
            double y2 = f1.yPos + f1.height;
            double x3 = rectBounds[0];
            double y3 = rectBounds[1];
            double x4 = rectBounds[2];
            double y4 = rectBounds[3];

            double overlapBotLeftX = x1 > x3 ? x1 : x3;
            double overlapBotLeftY = y1 > y3 ? y1 : y3;
            double overlapTopRightX = x2 < x4 ? x2 : x4;
            double overlapTopRightY = y2 < y4 ? y2 : y4;

            return new List<double> { overlapBotLeftX, overlapBotLeftY, overlapTopRightX, overlapTopRightY };
        }

        // Clip overlap
        private static List<FreeRectangle> ClipOverlap(FreeRectangle f1, List<double> overlapBound)
        {
            double F1x = f1.xPos;
            double F1y = f1.yPos;

            // Bottom left x,y and top right x,y of overlap
            double overlapBotLeftX = overlapBound[0];
            double overlapBotLeftY = overlapBound[1];
            double overlapTopRightX = overlapBound[2];
            double overlapTopRightY = overlapBound[3];

            List<FreeRectangle> newFreeRects = new List<FreeRectangle>();
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
                    height = overlapBotLeftY-F1y,
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
                    height = (F1y+f1.height) - overlapTopRightY,
                    xPos = F1x,
                    yPos = overlapTopRightY
                });
            }
            return newFreeRects;


        }

        // check if FreeRectangle is fully incapsulated in another
        private static bool IsEncapsulated(FreeRectangle f1, FreeRectangle f2)
        {
            int precsion = 2;
            if (Math.Round(f2.xPos, precsion) < Math.Round(f1.xPos, precsion) || Math.Round(f2.xPos, precsion) > Math.Round(f1.xPos+f1.width, precsion))
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

        // If FreeRectangle is fully encapsulated, remove it.
        private static void RemoveEncapsulated()
        {
            for (int i = 0; i < freeRectangles.Count; i++)
            {
                for (int j = i+1; j < freeRectangles.Count; j++)
                {
                    if (IsEncapsulated(freeRectangles[j], freeRectangles[i]))
                    {
                        freeRectangles.Remove(freeRectangles[i]);
                        i -= 1;
                        break;
                    }
                    if (IsEncapsulated(freeRectangles[i], freeRectangles[j]))
                    {
                        freeRectangles.Remove(freeRectangles[j]);
                        j -= 1;
                    }
                }
            }
        }
        #endregion

        #endregion
    }
}
