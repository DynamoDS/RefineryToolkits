using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
using Autodesk.DesignScript.Geometry;
using GraphicalDynamo.Graphs;

namespace GenerativeToolkit.Visibility
{
    [IsVisibleInDynamoLibrary(false)]
    public class Isovist
    {
        /// <summary>
        /// Returns a surface representing the Isovist area visible from 
        /// the given point.
        /// </summary>
        /// <param name="baseGraph">Base Graph</param>
        /// <param name="point">Origin point</param>
        /// <returns name="isovist">Surface representing the isovist area</returns>
        [NodeCategory("Actions")]
        [IsVisibleInDynamoLibrary(true)]
        public static Surface IsovistFromPoint(BaseGraph baseGraph, Point point)
        {
            return BaseGraph.IsovistFromPoint(baseGraph, point);
        }
    }
}
