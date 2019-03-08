using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Geometry;

namespace Revit
{
    public static class ElementCreation
    {
        /// <summary>
        /// Creates a Revit floors from building masser
        /// </summary>
        /// <param name="floors"></param>
        /// <param name="b"></param>
        /// <returns>FloorElement</returns>
        /// <search>addition,multiplication,math</search>
        [MultiReturn(new[] { "FloorElement" })]
        public static Dictionary<string, object> CreateFloors(double floors, double b)
        {
            double c = 0;
            double d = 0;

            // return a dictionary
            return new Dictionary<string, object>
            {
                {"FloorElement", d},
                {"Subtraction", c}
            };
        }
    }
}
