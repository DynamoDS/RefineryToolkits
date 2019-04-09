#region namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using CromulentBisgetti.ContainerPacking.Algorithms;
using CromulentBisgetti.ContainerPacking.Entities;
#endregion

namespace Autodesk.GenerativeToolkit.Generate
{
    public static class BinPacking3D
    {
        [IsVisibleInDynamoLibrary(true)]
        public static Container Bin(double length, double width, double height, int ID)
        {
            decimal dlength = Convert.ToDecimal(length);
            decimal dwidth = Convert.ToDecimal(width);
            decimal dheight = Convert.ToDecimal(height);

            Container container = new Container(ID, dlength, dwidth, dheight);
            return container;
        }

        [IsVisibleInDynamoLibrary(true)]
        public static Item Item(double length, double width, double height, int ID, int amount)
        {
            decimal dlength = Convert.ToDecimal(length);
            decimal dwidth = Convert.ToDecimal(width);
            decimal dheight = Convert.ToDecimal(height);

           Item item = new Item(ID, dlength, dwidth, dheight, amount);
            return item;
        }

        [IsVisibleInDynamoLibrary(true)]
        [MultiReturn(new[] { "packedItems", "remainItems" })]
        public static Dictionary<string, List<Item>> Pack(Container bin, List<Item> items)
        {
            List<Container> containers = new List<Container>{ bin };
            List<int> algorithm = new List<int> { 1 };

            ContainerPackingResult packingResult = CromulentBisgetti.ContainerPacking.PackingService.Pack(containers, items, algorithm).FirstOrDefault();
            AlgorithmPackingResult algorithmPackingResult = packingResult.AlgorithmPackingResults.FirstOrDefault();

            List<Item> packedItems = algorithmPackingResult.PackedItems;
            List<Item> unpackedItems = algorithmPackingResult.UnpackedItems;

            Dictionary<string, List<Item>> newOutput;
            newOutput = new Dictionary<string, List<Item>>
            {
                {"packedItems",packedItems},
                {"remainItems",unpackedItems}
            };

            return newOutput;
        }

        [IsVisibleInDynamoLibrary(true)]
        public static List<Geometry> GeometryByItems(List<Item> items)
        {
            List<Geometry> geometry = new List<Geometry>();

            foreach (Item item in items)
            {
                Point lowPoint = Point.ByCoordinates(Convert.ToDouble(item.CoordX), Convert.ToDouble(item.CoordZ), Convert.ToDouble(item.CoordY) );
                Point highPoint = lowPoint.Add(Vector.ByCoordinates(Convert.ToDouble(item.PackDimX), Convert.ToDouble(item.PackDimZ), Convert.ToDouble(item.PackDimY)));
                Cuboid cuboid = Cuboid.ByCorners(lowPoint, highPoint);
                geometry.Add(cuboid);

                //Dispose redundant Geometry
                lowPoint.Dispose();
                highPoint.Dispose();
            }

            return geometry;
        }
    }
}
