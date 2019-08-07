using Autodesk.DesignScript.Runtime;

namespace Autodesk.RefineryToolkits.SpacePlanning.Generate
{
    /// <summary>
    /// Placement methods for packing rectangles in a bin.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public enum PlacementMethods
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
