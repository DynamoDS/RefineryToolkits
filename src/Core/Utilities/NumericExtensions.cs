using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodesk.RefineryToolkits.Core.Utillites
{
    /// <summary>
    /// Extensions methods for numerical objects
    /// </summary>
    public static class NumericExtensions
    {

        #region Constants
        const double EPS = 1e-5;
        #endregion

        /// <summary>
        /// Double extension method.
        /// Maps a double value from a given range to a new one.
        /// </summary>
        /// <param name="value">Value to map</param>
        /// <param name="min">Original range minimum value</param>
        /// <param name="max">Original range maximum value</param>
        /// <param name="newMin">Target range minimum value</param>
        /// <param name="newMax">Target range maximum value</param>
        /// <returns name="mapped">Mapped value to target range</returns>
        public static double Map(this double value, double min, double max, double newMin, double newMax)
        {
            double normal = (value - min) / (max - min);
            return (normal * (newMax - newMin)) + newMin;
        }

        /// <summary>
        /// Determines if the difference between two values is less or equal to
        /// a constant decimal value of 1e-5
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public static bool AlmostEqualTo(this double value1, double value2)
        {
            return Math.Abs(value1 - value2) <= EPS;
        }

        /// <summary>
        /// Rounds a value to a given number of decimals.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="decimals">Default 6 decimals</param>
        /// <returns></returns>
        public static double Round(this double value, int decimals = 6)
        {
            return Math.Round(value, decimals);
        }

        /// <summary>
        /// Converts a radian angle to degrees
        /// </summary>
        /// <param name="radians"></param>
        /// <returns></returns>
        public static double ToDegrees(this double radians)
        {
            return radians * (180 / Math.PI);
        }

        /// <summary>
        /// Converts a degree angle to radians
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static double ToRadians(this double degrees)
        {
            return degrees * (Math.PI / 180);
        }

    }
}
