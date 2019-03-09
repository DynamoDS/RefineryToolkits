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
        /// <param name="Floors"></param>
        /// <returns></returns>
        /// <search>refinery</search>
        [MultiReturn(new[] { "FloorElement" })]
        public static Dictionary<string, object> CreateRevitFloors(double Floors)
        {
            List<Surface> elements = null;

            // return a dictionary
            return new Dictionary<string, object>
            {
                {"FloorElement", elements},
            };
        }

        /// <summary>
        /// Creates a Revit mass as a direct shape from building masser
        /// </summary>
        /// <param name="Mass">A polysurface mass</param>
        /// <param name="Category">A category for the mass</param>
        /// <returns></returns>
        /// <search>refinery</search>
        [MultiReturn(new[] { "MassElement" })]
        public static Dictionary<string, object> CreateRevitMass(double Mass, string Category)
        {
            List<Surface> elements = null;

            // return a dictionary
            return new Dictionary<string, object>
            {
                {"MassElement", elements},
            };
        }
    }
}
