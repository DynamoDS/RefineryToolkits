using Autodesk.DesignScript.Geometry;
using CromulentBisgetti.ContainerPacking.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Autodesk.RefineryToolkits.SpacePlanning.Generate
{
    public class BinPacker3D
    {
        #region Properties & constants

        private const int PACKING_ALGORITHM_ID = 1;
        private const string PackingFailed = "Could not pack items in container.";
        private const string BinNotInitialised = "Bin has not been initialised";
        private Container bin;

        public List<Cuboid> PackedCuboids { get; private set; }

        public List<int> PackedIndices { get; private set; }

        public List<Cuboid> RemainingCuboids { get; private set; }

        public double PercentContainerVolumePacked { get; private set; }

        public double PercentItemVolumePacked { get; private set; }

        #endregion

        public BinPacker3D()
        {
            this.PackedCuboids = new List<Cuboid>();
            this.PackedIndices = new List<int>();
            this.RemainingCuboids = new List<Cuboid>();
            this.bin = null;
        }

        public BinPacker3D(Cuboid bin, int id) :this()
        {
            if (bin is null)
                throw new ArgumentNullException(nameof(bin));

            this.bin = ContainerFromCuboid(bin, id);
        }

        private void PackItems(List<Item> items)
        {
            if (this.bin == null) throw new InvalidOperationException(BinNotInitialised);
            if (items.Count == 0) throw new ArgumentNullException(nameof(items));

            var algorithm = new List<int> { PACKING_ALGORITHM_ID };

            var containerPackingResult = CromulentBisgetti.ContainerPacking.PackingService.Pack(new List<Container> { this.bin }, items, algorithm);
            if (containerPackingResult == null || containerPackingResult.Count == 0) throw new InvalidOperationException(PackingFailed);

            // we are only packing in 1 container at a time so there will only ever be 1 packing result
            var packingResult = containerPackingResult.FirstOrDefault().AlgorithmPackingResults.FirstOrDefault();
            if (packingResult == null) throw new InvalidOperationException(PackingFailed);

            // record results in this packer instance
            this.PackedCuboids = CuboidsFromItems(packingResult.PackedItems);
            this.RemainingCuboids = CuboidsFromItems(packingResult.UnpackedItems);
            this.PackedIndices = IdsFromItems(packingResult.PackedItems);
            this.PercentContainerVolumePacked = decimal.ToDouble(packingResult.PercentContainerVolumePacked);
            this.PercentItemVolumePacked = decimal.ToDouble(packingResult.PercentItemVolumePacked);
        }

        public void PackCuboids(List<Cuboid> cuboids)
        {
            if (cuboids.Count == 0) throw new ArgumentNullException(nameof(cuboids));

            // convert cuboids to items and containers so packing library can process them
            var itemsToPack = ItemsFromCuboids(cuboids);

            this.PackItems(itemsToPack);
        }

        public static List<BinPacker3D> PackCuboidsAcrossBins(
            List<Cuboid> bins,
            List<Cuboid> cuboids)
        {
            if (bins == null || bins.Count == 0)
                throw new ArgumentNullException(nameof(bins));

            if (cuboids == null || cuboids.Count == 0)
                throw new ArgumentNullException(nameof(cuboids));

            // we need to keep track of packed items across bins
            // and then aggregate results, hence the lists external to BinPacker object
            var remainingItems = new List<Item>(ItemsFromCuboids(cuboids));
            var packers = new List<BinPacker3D>();

            for (var i = 0; i < bins.Count; i++)
            {
                // pack items
                var currentBin = bins[i];
                var packer = new BinPacker3D(currentBin, i);
                packer.PackItems(remainingItems);

                // record results
                packers.Add(packer);

                // update list of remaining items to pack
                RemovePackedItemsById(remainingItems, packer);
            }
            return packers;
        }

        #region Helpers

        private static void RemovePackedItemsById(List<Item> items, BinPacker3D packer)
        {
            items.RemoveAll(x => packer.PackedIndices.Contains(x.ID));
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

        #endregion

        #region DesignScript <> CromulentBisgetti conversions

        private static Container ContainerFromCuboid(Cuboid cuboid, int id)
        {
            var length = Convert.ToDecimal(cuboid.Length);
            var width = Convert.ToDecimal(cuboid.Width);
            var height = Convert.ToDecimal(cuboid.Height);
            return new Container(id, length, width, height);
        }

        /// <summary>
        /// Creates a DesignScript cuboid from a bin-packing Item.
        /// </summary>
        /// <param name="item">The item to create Cuboid from.</param>
        /// <returns>The cuboid that represents the Item.</returns>
        private static Cuboid CuboidFromItem(Item item)
        {
            var lowPoint = Point.ByCoordinates(Convert.ToDouble(item.CoordX), Convert.ToDouble(item.CoordZ), Convert.ToDouble(item.CoordY));
            Point highPoint = lowPoint.Add(Vector.ByCoordinates(Convert.ToDouble(item.Dim1), Convert.ToDouble(item.Dim2), Convert.ToDouble(item.Dim3)));
            var cuboid = Cuboid.ByCorners(lowPoint, highPoint);

            // Dispose redundant intermediate geometry
            lowPoint.Dispose();
            highPoint.Dispose();

            return cuboid;
        }

        private static List<Cuboid> CuboidsFromItems(List<Item> items)
        {
            var cuboids = new List<Cuboid>();
            for (var i = 0; i < items.Count; i++)
            {
                cuboids.Add(CuboidFromItem(items[i]));
            }
            return cuboids;
        }

        /// <summary>
        /// Creates a bin-packing compatible Item from a DesignScript cuboid.
        /// </summary>
        /// <param name="cuboid">The cuboid to create item from.</param>
        /// <param name="id">The id of the new item.</param>
        /// <returns>The bin-packing compatible item.</returns>
        private static Item ItemFromCuboid(Cuboid cuboid, int id)
        {
            if (cuboid == null) return null;

            var length = Convert.ToDecimal(cuboid.Length);
            var width = Convert.ToDecimal(cuboid.Width);
            var height = Convert.ToDecimal(cuboid.Height);

            return new Item(id, length, width, height, 1);
        }

        private static List<Item> ItemsFromCuboids(List<Cuboid> cuboids)
        {
            var items = new List<Item>();
            for (var i = 0; i < cuboids.Count; i++)
            {
                items.Add(ItemFromCuboid(cuboids[i], i));
            }
            return items;
        }

        #endregion
    }
}
