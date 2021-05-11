using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using CromulentBisgetti.ContainerPacking.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Autodesk.RefineryToolkits.SpacePlanning.Generate.Packers
{
    [IsVisibleInDynamoLibrary(false)]
    public class CuboidPacker : IPacker<Cuboid, Cuboid>
    {
        #region Properties & constants

        private const int PACKING_ALGORITHM_ID = 1;
        private const string PackingFailed = "Could not pack items in container.";
        private const string BinNotInitialised = "Bin has not been initialised";
        private Container bin;
        private Cuboid containerCuboid;

        public List<Cuboid> PackedItems { get; private set; }

        public List<int> PackedIndices { get; private set; }

        public List<int> RemainingIndices { get; private set; }

        public double PercentContainerVolumePacked { get; private set; }

        public double PercentItemVolumePacked { get; private set; }

        #endregion

        #region Constructors

        public CuboidPacker()
        {
            this.PackedItems = new List<Cuboid>();
            this.PackedIndices = new List<int>();
            this.RemainingIndices = new List<int>();
            this.bin = null;
        }

        public CuboidPacker(Cuboid bin, int id) : this()
        {
            if (bin is null)
                throw new ArgumentNullException(nameof(bin));

            this.containerCuboid = bin;
            this.bin = ContainerFromCuboid(bin, id);
        }

        #endregion

        #region Packing methods

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
            
            // if the container was not used, we return a blank list
            if (!packingResult.PackedItems.Any())
            {
                this.PackedItems = new List<Cuboid>();
                this.RemainingIndices = IdsFromItems(packingResult.UnpackedItems);
                this.PackedIndices = new List<int>();
                this.PercentContainerVolumePacked = decimal.ToDouble(packingResult.PercentContainerVolumePacked);
                this.PercentItemVolumePacked = decimal.ToDouble(packingResult.PercentItemVolumePacked);
                return;
            }

            var packedCuboids = CuboidsFromItems(packingResult.PackedItems);
            this.PackedItems = TransformPackedCuboidsToContainerCuboidCoords(this.containerCuboid, packedCuboids);
            this.RemainingIndices = IdsFromItems(packingResult.UnpackedItems);
            this.PackedIndices = IdsFromItems(packingResult.PackedItems);
            this.PercentContainerVolumePacked = decimal.ToDouble(packingResult.PercentContainerVolumePacked);
            this.PercentItemVolumePacked = decimal.ToDouble(packingResult.PercentItemVolumePacked);
        }

        public void PackOneContainer(List<Cuboid> items, Cuboid container)
        {
            if (items.Count == 0)
                throw new ArgumentNullException(nameof(items));
            if (container is null)
                throw new ArgumentNullException(nameof(container));

            this.bin = ContainerFromCuboid(container, 0);

            // convert cuboids to items and containers so packing library can process them
            var itemsToPack = ItemsFromCuboids(items);

            this.PackItems(itemsToPack);
        }

        public List<IPacker<Cuboid, Cuboid>> PackMultipleContainers(
            List<Cuboid> items,
            List<Cuboid> containers)
        {
            return this.PackMultipleContainersWithStats(items, containers)
                .Select(x=>x as IPacker<Cuboid,Cuboid>)
                .ToList();
        }

        /// <summary>
        /// Packs the supplied items across the supplied containers and returns packing results and expanded statistics about packing performance.
        /// </summary>
        /// <param name="items">The items to pack.</param>
        /// <param name="containers">The containers to pack into.</param>
        /// <returns>Packing results and expanded statistics about packing performance.</returns>
        public List<CuboidPacker> PackMultipleContainersWithStats(
            List<Cuboid> items,
            List<Cuboid> containers)
        {
            if (containers == null || containers.Count == 0)
                throw new ArgumentNullException(nameof(containers));

            if (items == null || items.Count == 0)
                throw new ArgumentNullException(nameof(items));

            // we need to keep track of packed items across bins
            // and then aggregate results, hence the lists external to BinPacker object
            var remainingItems = new List<Item>(ItemsFromCuboids(items));
            var packers = new List<CuboidPacker>();

            for (var i = 0; i < containers.Count; i++)
            {
                //this moves on if the pack is complete and there are containers left
                if (remainingItems.Count == 0)
                    continue;
                
                // pack items
                var currentBin = containers[i];
                var packer = new CuboidPacker(currentBin, i);
                packer.PackItems(remainingItems);

                // update list of remaining items to pack
                RemovePackedItemsById(remainingItems, packer);

                // record results
                packers.Add(packer);
            }

            return packers;
        }

        #endregion

        #region Helpers

        private static void RemovePackedItemsById(List<Item> items, CuboidPacker packer)
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
            var length = Convert.ToDecimal(cuboid.Width);
            var width = Convert.ToDecimal(cuboid.Length);
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
            Point highPoint = lowPoint.Add(Vector.ByCoordinates(Convert.ToDouble(item.PackDimX), Convert.ToDouble(item.PackDimZ), Convert.ToDouble(item.PackDimY)));
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

        private static List<Cuboid> TransformPackedCuboidsToContainerCuboidCoords(Cuboid container, List<Cuboid> packedItems)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            if (packedItems == null || packedItems.Count == 0)
                throw new ArgumentNullException(nameof(packedItems));

            CoordinateSystem fromCS = CoordinateSystem.ByOrigin(BoundingBox.ByGeometry(packedItems).MinPoint);
            CoordinateSystem toCS = CoordinateSystem.ByOrigin(BoundingBox.ByGeometry(new List<Cuboid> { container }).MinPoint);
            var transformedCuboids = new List<Cuboid>();
            for (var i = 0; i < packedItems.Count; i++)
            {
                transformedCuboids.Add(packedItems[i].Transform(fromCS, toCS) as Cuboid);
            }
            fromCS.Dispose();
            toCS.Dispose();
            return transformedCuboids;

        }

        #endregion
    }
}
