using Autodesk.DesignScript.Runtime;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodesk.GenerativeToolkit.Rank
{
    [IsVisibleInDynamoLibrary(false)]
    public class SeededShuffle
    {
        [IsVisibleInDynamoLibrary(true)]
        public static IList SeddedShuffle(IList list, int seed)
        {
            var rng = new Random(seed);
            return list.Cast<object>().OrderBy(_ => rng.Next()).ToList();
        }
    }
}
