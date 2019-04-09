#region namespaces
using Autodesk.DesignScript.Runtime;
using System;
using System.Collections;
using System.Linq;
#endregion

namespace Autodesk.GenerativeToolkit.Rank
{
    [IsVisibleInDynamoLibrary(false)]
    public class ListShuffle
    {
        [IsVisibleInDynamoLibrary(true)]
        public static IList SeededShuffle(IList list, int seed)
        {
            var rng = new Random(seed);
            return list.Cast<object>().OrderBy(_ => rng.Next()).ToList();
        }
    }
}
