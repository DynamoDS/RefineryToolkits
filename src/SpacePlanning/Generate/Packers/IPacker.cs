using System.Collections.Generic;

namespace Autodesk.RefineryToolkits.SpacePlanning.Generate.Packers
{
    public interface IPacker<TItem, TContainer>
    {
        List<int> PackedIndices { get; }
        List<TItem> PackedItems { get; }
        List<int> RemainingIndices { get; }

        void PackOneContainer(
            List<TItem> items,
            TContainer container);

        List<IPacker<TItem, TContainer>> PackMultipleContainers(
            List<TItem> items,
            List<TContainer> containers);
    }
}