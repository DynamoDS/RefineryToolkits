using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Geometry;

namespace ProvingGround.RefineryNodes
{
    public static class nodeSample
    {
        /// <summary>
        /// Example Single Output Node
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>c</returns>
        /// <search>addition,math</search>
        public static double SingleOutputNode(double a, double b)
        {
            double c = a + b;

            return c;
        }


        /// <summary>
        /// Example MultiReturn Node
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>c, d</returns>
        /// <search>addition,multiplication,math</search>
        [MultiReturn(new[] { "Addition", "Subtraction" })]
        public static Dictionary<string, object> MultiReturnNode(double a, double b)
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
