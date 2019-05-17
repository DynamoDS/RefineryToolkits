using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using CromulentBisgetti.ContainerPacking.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Autodesk.GenerativeToolkit.Generate
{
    public static class BinPacking3D
    {
        private const string packedItemsOutputPort = "packedItems";
        private const string indicesOutputPort = "packedIndices";
        private const string remainingItemsOutputPort = "remainItems";

        /// <summary>
        /// Packs a sample list of Cuboids in a bin Cuboid
        /// </summary>
        /// <param name="bin">Cuboid to pack sample Cuboids into</param>
        /// <param name="items">List of Cuboids to pack</param>
        /// <returns name="packedItems">Packed Cuboids</returns>
        /// <returns name="packedIndices">Indices of packed items</returns>
        /// <returns name="remainItems">Cubiods that didn't get packed</returns>
        [MultiReturn(new[] { packedItemsOutputPort, indicesOutputPort, remainingItemsOutputPort })]
        public static Dictionary<string, object> Pack(
            Cuboid bin,
            List<Cuboid> items)
        {
            decimal length = Convert.ToDecimal(bin.Length);
            decimal width = Convert.ToDecimal(bin.Width);
            decimal height = Convert.ToDecimal(bin.Height);
            List<Container> container = new List<Container> { new Container(1, length, width, height) };

            List<Item> itemsToPack = ItemsFromCuboids(items);

            //Dictionary to map Item Id with Cuboid index
            Dictionary<int, int> idDict = IdFromItem(itemsToPack).Zip(Enumerable.Range(0, itemsToPack.Count), (k, v) => new { Key = k, Value = v }).ToDictionary(x => x.Key, x => x.Value);

            List<int> algorithm = new List<int> { 1 };

            ContainerPackingResult packingResult = CromulentBisgetti.ContainerPacking.PackingService.Pack(container, itemsToPack, algorithm).FirstOrDefault();
            AlgorithmPackingResult algorithmPackingResult = packingResult.AlgorithmPackingResults.FirstOrDefault();

            List<Item> packedItems = algorithmPackingResult.PackedItems;
            List<Cuboid> packedCuboids = CuboidsFromItems(packedItems);

            List<Item> unpackedItems = algorithmPackingResult.UnpackedItems;
            List<Cuboid> unpackedCuboids = CuboidsFromItems(unpackedItems);

            List<int> packedIndices = IdFromItem(packedItems).Where(idDict.ContainsKey)
                     .Select(x => idDict[x])
                     .ToList();

            Dictionary<string, object> newOutput;
            newOutput = new Dictionary<string, object>
            {
                { BinPacking3D.packedItemsOutputPort, packedCuboids},
                { indicesOutputPort, packedIndices},
                { remainingItemsOutputPort, unpackedCuboids}
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

        private static List<int> IdFromItem(List<Item> items)
        {
            List<int> ids = new List<int>();
            foreach (Item item in items)
            {
                ids.Add(item.ID);
            }
            return ids;
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
