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
        [MultiReturn(new[] { "packedItems", "remainItems" })]
        public static Dictionary<string, List<Cuboid>> Pack(Cuboid bin, List<Cuboid> items)
        {
            decimal length = Convert.ToDecimal(bin.Length);
            decimal width = Convert.ToDecimal(bin.Width);
            decimal height = Convert.ToDecimal(bin.Height);
            List<Container> container = new List<Container> { new Container(1, length, width, height) };

            List<Item> itemsToPack = ItemsFromCuboids(items);

            List<int> algorithm = new List<int> { 1 };

            ContainerPackingResult packingResult = CromulentBisgetti.ContainerPacking.PackingService.Pack(container, itemsToPack, algorithm).FirstOrDefault();
            AlgorithmPackingResult algorithmPackingResult = packingResult.AlgorithmPackingResults.FirstOrDefault();

            List<Item> packedItems = algorithmPackingResult.PackedItems;
            List<Cuboid> packedCuboids = CuboidsFromItems(packedItems);

            List<Item> unpackedItems = algorithmPackingResult.UnpackedItems;
            List<Cuboid> unpackedCuboids = CuboidsFromItems(unpackedItems);

            Dictionary<string, List<Cuboid>> newOutput;
            newOutput = new Dictionary<string, List<Cuboid>>
            {
                {"packedItems",packedCuboids},
                {"remainItems",unpackedCuboids}
            };

            return newOutput;
        }

        private static List<Item> ItemsFromCuboids(List<Cuboid> cuboids)
        {
            List<Item> items = new List<Item>();
            for (int i = 0; i < cuboids.Count; i++)
            {
                decimal length = Convert.ToDecimal(cuboids[i].Length);
                decimal width = Convert.ToDecimal(cuboids[i].Width);
                decimal height = Convert.ToDecimal(cuboids[i].Height);

                Item item = new Item(i, length, width, height, 1);
                items.Add(item);
            }
            return items;

        }
        private static List<Cuboid> CuboidsFromItems(List<Item> items)
        {
            List<Cuboid> cuboids = new List<Cuboid>();

            foreach (Item item in items)
            {
                Point lowPoint = Point.ByCoordinates(Convert.ToDouble(item.CoordX), Convert.ToDouble(item.CoordZ), Convert.ToDouble(item.CoordY));
                Point highPoint = lowPoint.Add(Vector.ByCoordinates(Convert.ToDouble(item.PackDimX), Convert.ToDouble(item.PackDimZ), Convert.ToDouble(item.PackDimY)));
                Cuboid cuboid = Cuboid.ByCorners(lowPoint, highPoint);
                cuboids.Add(cuboid);

                //Dispose redundant Geometry
                lowPoint.Dispose();
                highPoint.Dispose();
            }
            return cuboids;
        }
    }
}
