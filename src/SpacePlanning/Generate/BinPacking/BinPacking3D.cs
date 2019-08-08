using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using CromulentBisgetti.ContainerPacking.Entities;
using Dynamo.Graph.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Autodesk.RefineryToolkits.SpacePlanning.Generate
{
    public static partial class BinPacking
    {
        private const string packedItemsOutputPort3D = "Packed Items";
        private const string indicesOutputPort3D = "Packed Indices";
        private const string remainingItemsOutputPort3D = "Remaining Items";

        /// <summary>
        /// Packs a sample list of Cuboids in a bin Cuboid
        /// </summary>
        /// <param name="bin">Cuboid to pack sample Cuboids into</param>
        /// <param name="items">List of Cuboids to pack</param>
        /// <returns name="packedItems">Packed Cuboids</returns>
        /// <returns name="packedIndices">Indices of packed items</returns>
        /// <returns name="remainItems">Cubiods that didn't get packed</returns>
        [MultiReturn(new[] { packedItemsOutputPort3D, indicesOutputPort3D, remainingItemsOutputPort3D })]
        public static Dictionary<string, object> Pack3D(
            Cuboid bin,
            List<Cuboid> items)
        {
            if (bin == null)
                throw new ArgumentNullException(nameof(bin));

            if (items == null || items.Count == 0)
                throw new ArgumentNullException(nameof(items));

            var length = Convert.ToDecimal(bin.Length);
            var width = Convert.ToDecimal(bin.Width);
            var height = Convert.ToDecimal(bin.Height);
            var container = new List<Container> { new Container(1, length, width, height) };

            List<Item> itemsToPack = ItemsFromCuboids(items);

            var algorithm = new List<int> { 1 };

            ContainerPackingResult packingResult = CromulentBisgetti.ContainerPacking.PackingService.Pack(container, itemsToPack, algorithm).FirstOrDefault();
            AlgorithmPackingResult algorithmPackingResult = packingResult.AlgorithmPackingResults.FirstOrDefault();

            List<Cuboid> packedCuboids = CuboidsFromItems(algorithmPackingResult.PackedItems);
            List<Cuboid> unpackedCuboids = CuboidsFromItems(algorithmPackingResult.UnpackedItems);
            List<int> packedIndices = IdsFromItems(algorithmPackingResult.PackedItems);

            return new Dictionary<string, object>
            {
                { packedItemsOutputPort3D, packedCuboids},
                { indicesOutputPort3D, packedIndices},
                { remainingItemsOutputPort3D, unpackedCuboids}
            };
        }

        #region DesignScript <> CromulentBisgetti conversions

        private static List<Item> ItemsFromCuboids(List<Cuboid> cuboids)
        {
            var items = new List<Item>();
            for (var i = 0; i < cuboids.Count; i++)
            {
                items.Add(cuboids[i].ToItem(i));
            }
            return items;
        }

        private static List<int> IdsFromItems(List<Item> items)
        {
            var ids = new List<int>();
            for (var i = 0; i < items.Count; i++)
            {
                ids.Add(items[i].ID);
            }
            return ids;
        }

        private static List<Cuboid> CuboidsFromItems(List<Item> items)
        {
            var cuboids = new List<Cuboid>();
            for (var i = 0; i < items.Count; i++)
            {
                cuboids.Add(items[i].ToCuboid());
            }
            return cuboids;
        }

        /// <summary>
        /// Creates a bin-packing compatible Item from a DesignScript cuboid.
        /// </summary>
        /// <param name="cuboid">The cuboid to create item from.</param>
        /// <param name="id">The id of the new item.</param>
        /// <returns>The bin-packing compatible item.</returns>
        private static Item ToItem(this Cuboid cuboid, int id)
        {
            var length = Convert.ToDecimal(cuboid.Length);
            var width = Convert.ToDecimal(cuboid.Width);
            var height = Convert.ToDecimal(cuboid.Height);

            return new Item(id, length, width, height, 1);
        }

        /// <summary>
        /// Creates a DesignScript cuboid from a bin-packing Item.
        /// </summary>
        /// <param name="item">The item to create Cuboid from.</param>
        /// <returns>The cuboid that represents the Item.</returns>
        private static Cuboid ToCuboid(this Item item)
        {
            var lowPoint = Point.ByCoordinates(Convert.ToDouble(item.CoordX), Convert.ToDouble(item.CoordZ), Convert.ToDouble(item.CoordY));
            var highPoint = lowPoint.Add(Vector.ByCoordinates(Convert.ToDouble(item.PackDimX), Convert.ToDouble(item.PackDimZ), Convert.ToDouble(item.PackDimY)));
            var cuboid = Cuboid.ByCorners(lowPoint, highPoint);

            // Dispose redundant intermediate geometry
            lowPoint.Dispose();
            highPoint.Dispose();

            return cuboid;
        }

        #endregion
    }
}
