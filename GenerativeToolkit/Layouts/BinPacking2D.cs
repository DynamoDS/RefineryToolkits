using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;

namespace GenerativeToolkit.Layouts
{
    public class BinPacking2D
    {
        internal BinPacking2D()
        {

        }

        internal class Node
        {
            public double width;
            public double height;
            public double xPos;
            public double yPos;
            public Node leftNode;
            public Node topNode;
            public bool isOccupied;
        }

        private static Node rootNode;

        public static List<Geometry> Packer2DCustomOrder(List<Rectangle> rects, Rectangle bin)
        {
            // Initialize root node
            rootNode = new Node
            {
                xPos = bin.StartPoint.X,
                yPos = bin.StartPoint.Y,
                height = bin.Height,
                width = bin.Width
            };

            return Pack(rects);
        }

        public static List<Geometry> Packer2DDescendingOrder(List<Rectangle> rects, Rectangle bin)
        {
            // Sort rectangles into descending order based on area
            rects = rects.OrderByDescending(rect => (rect.Height * rect.Width)).ToList();
            
            // Initialize root node
            rootNode = new Node {
                xPos = bin.StartPoint.X,
                yPos = bin.StartPoint.Y,
                height = bin.Height,
                width = bin.Width
            };

            return Pack(rects);
        }

        public static List<Geometry> Packer2DAescendingOrder(List<Rectangle> rects, Rectangle bin)
        {
            // Sort rectangles into descending order based on area
            rects = rects.OrderByDescending(rect => (rect.Height * rect.Width)).Reverse().ToList();

            // Initialize root node
            rootNode = new Node
            {
                xPos = bin.StartPoint.X,
                yPos = bin.StartPoint.Y,
                height = bin.Height,
                width = bin.Width
            };

            return Pack(rects);
        }

        private static List<Geometry> Pack(List<Rectangle> sortedRects)
        {
            List<Geometry> packedRectangles = new List<Geometry>();

            foreach (var rect in sortedRects)
            {
                CoordinateSystem originCS = CoordinateSystem.ByOrigin(rect.StartPoint);

                var node = FindNode(rootNode, rect.Width, rect.Height);
                if (node != null)
                {
                    // Split rectangles
                    Node position = SplitNode(node, rect.Width, rect.Height);
                    // Re-posistion rectangles
                    CoordinateSystem newCS = CoordinateSystem.ByOrigin(position.xPos, position.yPos);
                    packedRectangles.Add(rect.Transform(originCS, newCS));
                    newCS.Dispose();
                }
                originCS.Dispose();
            }
            return packedRectangles;
        }

        private static Node FindNode(Node node, double rectWidth, double rectHeight)
        {
            if (node.isOccupied)
            {
                var nextNode = FindNode(node.topNode, rectWidth, rectHeight);

                if (nextNode == null)
                {
                    nextNode = FindNode(node.leftNode, rectWidth, rectHeight);
                }

                return nextNode;
            }
            else if (rectWidth <= node.width && rectHeight <= node.height)
            {
                return node;
            }
            else
            {
                return null;
            }
        }

        private static Node SplitNode(Node node, double rectWidth, double rectHeight)
        {
            node.isOccupied = true;
            node.topNode = new Node {
                yPos = node.yPos + rectHeight,
                xPos = node.xPos,
                height = node.height - rectHeight,
                width = node.width
            };
            node.leftNode = new Node {
                yPos = node.yPos,
                xPos = node.xPos - rectWidth,
                height = rectHeight,
                width = node.width - rectWidth
            };
            return node;
        }
    }
}
