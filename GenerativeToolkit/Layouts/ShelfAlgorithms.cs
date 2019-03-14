using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeToolkit.Layouts
{
    [IsVisibleInDynamoLibrary(false)]
    public class ShelfAlgorithms
    {
        internal ShelfAlgorithms()
        {

        }

        internal class Shelf
        {
            public double xPos;
            public double yPos;
            public double width;
            public double height;
            public bool isInUse;
            public double remainBinHeight;
        }

        internal class Bin
        {
            public double width;
            public double height;
            public double xPos;
            public double yPos;
        }

        private static Bin bin;
        private static Shelf currentShelf;

        [IsVisibleInDynamoLibrary(false)]
        public static List<Rectangle> ShelfNF(List<Rectangle> rects, Rectangle binRect)
        {
            // Initialize bin
            bin = new Bin
            {
                width = binRect.Width,
                height = binRect.Height,
                xPos = binRect.StartPoint.X,
                yPos = binRect.StartPoint.Y
            
            };

            // Initialize root shelf
            currentShelf = new Shelf
            {
                xPos = binRect.StartPoint.X,
                yPos = binRect.StartPoint.Y,
                width = binRect.Width,
                remainBinHeight = binRect.Height
            };

            return ShelfNFPack(rects);
        }

        private static List<Rectangle> ShelfNFPack(List<Rectangle> sortedRects)
        {

            List<Rectangle> packedRectangles = new List<Rectangle>();

            foreach (var rect in sortedRects)
            {
                Rectangle rectangle = OrientRectangle(rect);
                if (rectangle != null)
                {
                    packedRectangles.Add(rectangle);
                }
                else
                {
                    break;
                }                          
            }
            return packedRectangles;
        }

        private static Rectangle OrientRectangle(Rectangle rectangle)
        {
            double shortSide = new List<double> { rectangle.Height, rectangle.Width }.Min(); ;
            double longSide = new List<double> { rectangle.Height, rectangle.Width }.Max(); ;
            double shelfHeight = currentShelf.height;

            if (currentShelf.isInUse)
            {
                CoordinateSystem newCS;
                if (longSide <= shelfHeight && shortSide <= currentShelf.width)
                {
                    newCS = CoordinateSystem.ByOrigin(currentShelf.xPos - (shortSide / 2), currentShelf.yPos + (longSide / 2));
                    rectangle = Rectangle.ByWidthLength(newCS, shortSide, longSide);
                    FillShelf(currentShelf, rectangle);
                }
                else if (shortSide <= shelfHeight && longSide <= currentShelf.width)
                {
                    newCS = CoordinateSystem.ByOrigin(currentShelf.xPos - (longSide / 2), currentShelf.yPos + (shortSide / 2));
                    rectangle = Rectangle.ByWidthLength(newCS, longSide, shortSide);
                    FillShelf(currentShelf, rectangle);
                }
                else
                {
                    rectangle = Rectangle.ByWidthLength(longSide, shortSide);
                    NewShelf(rectangle);
                    rectangle = OrientRectangle(rectangle);
                }
                
            }
            else
            {
                rectangle = Rectangle.ByWidthLength(CoordinateSystem.ByOrigin(currentShelf.xPos - (longSide / 2), currentShelf.yPos + (shortSide / 2)), longSide, shortSide);
                if (currentShelf.remainBinHeight < rectangle.Height)
                {
                    rectangle = null;
                }
                else
                {
                    currentShelf.height = rectangle.Height;
                    FillShelf(currentShelf, rectangle);
                }              
            }
            return rectangle;
        }

        private static void NewShelf(Rectangle rect)
        {
            currentShelf = new Shelf
            {
                height = rect.Height,
                width = bin.width,
                xPos = bin.xPos,
                yPos = currentShelf.yPos + currentShelf.height,
                isInUse = false,                
                remainBinHeight = currentShelf.remainBinHeight - currentShelf.height
            };
            
        }

        private static void FillShelf(Shelf shelf, Rectangle rect)
        {
            currentShelf.xPos = shelf.xPos - rect.Width;
            currentShelf.width = shelf.width - rect.Width;
            currentShelf.isInUse = true;           
        }
    }
}
