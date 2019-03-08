using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Geometry;

namespace Buildings
{
    public static class BuildingCreation
    {
        /// <summary>
        /// Generates a building mass
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>c, d</returns>
        /// <search>addition,multiplication,math</search>
        [MultiReturn(new[] { "Addition", "Subtraction" })]
        public static Dictionary<string, object> BuildingGenerator(double a, double b)
        {
            double c = a + b;
            double d = a * b;

            // return a dictionary
            return new Dictionary<string, object>
            {
                {"Addition", d},
                {"Subtraction", c}
            };
        }

        /// <summary>
        /// Generates a building core
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>c, d</returns>
        /// <search>addition,multiplication,math</search>
        [MultiReturn(new[] { "Addition", "Subtraction" })]
        public static Dictionary<string, object> CoreGenerator(double a, double b)
        {
            double c = a + b;
            double d = a * b;

            // return a dictionary
            return new Dictionary<string, object>
            {
                {"Addition", d},
                {"Subtraction", c}
            };
        }
    }
}
