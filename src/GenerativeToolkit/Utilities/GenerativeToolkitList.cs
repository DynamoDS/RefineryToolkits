using System;
using System.Collections.Generic;
using System.Linq;
using DSCore;
using DSCore.Properties;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

namespace Autodesk.GenerativeToolkit.Utilities
{
    [IsVisibleInDynamoLibrary(false)]
    internal static class List
    {

        #region RunningTotals
        /// <summary>
        /// Calculates the running totals from a list of numbers
        /// </summary>
        /// <param name="numbers"></param>
        /// <search></search>
        public static List<double> RunningTotals(this List<double> numbers)
        {
            double count = 0;
            List<double> runningTotals = new List<double>();
            foreach (double n in numbers)
            {
                count = count + n;
                runningTotals.Add(count);
            }
            return runningTotals;
        }
        #endregion
    }
}
