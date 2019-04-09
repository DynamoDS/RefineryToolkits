#region namespaces
using Autodesk.DesignScript.Runtime;
using DSCore;
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Autodesk.GenerativeToolkit.Explore
{
    public static class Colors
    {
        /// <summary>
        /// Creates a given amount of distinct colors which can be used in a color range
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="correctionFactor"></param>
        /// <returns>distinct colors</returns>
        public static List<Color> ContrastyColorRange(int amount, double correctionFactor)
        {
            if (amount > 19)
            {
                throw new Exception("Maximum number of colours supported right now is 19");
            }
            Random rnd = new Random();
            List<int> indexRanges = Enumerable.Range(0, amount).ToList();
            List<int> shuffledIndexes = indexRanges.OrderBy(x => rnd.Next()).ToList();

            List<Color> colors = new List<Color>();
            foreach (int i in shuffledIndexes)
            {
                Color color = ColorPalette[i];
                int red;
                int green;
                int blue;

                if (correctionFactor <= 0)
                {
                    red = Convert.ToInt32(color.Red * (1 + correctionFactor));
                    green = Convert.ToInt32(color.Green * (1 + correctionFactor));
                    blue = Convert.ToInt32(color.Blue * (1 + correctionFactor));
                }
                else
                {
                    red = Convert.ToInt32((255 - color.Red) * correctionFactor + color.Red);
                    green = Convert.ToInt32((255 - color.Green) * correctionFactor + color.Green);
                    blue = Convert.ToInt32((255 - color.Blue) * correctionFactor + color.Blue);
                }
                colors.Add(Color.ByARGB(r: red, g: green, b: blue));            
            }
            return colors;
        }

        // Colors from https://www.materialpalette.com/colors
        private static readonly List<Color> ColorPalette = new List<Color>()
        {
            //Red
            Color.ByARGB(0, 244, 67, 54),
            //Pink
            Color.ByARGB(0, 233, 30, 99),
            //Purple
            Color.ByARGB(0, 156, 39, 176),
            //Deep Purple
            Color.ByARGB(0, 103, 58, 183),
            //Indigo
            Color.ByARGB(0, 63, 81, 181),
            //Blue
            Color.ByARGB(0, 33, 150, 243),
            //Light Blue
            Color.ByARGB(0, 3, 169, 244),
            //Cyan
            Color.ByARGB(0, 0, 188, 212),
            //Teal
            Color.ByARGB(0, 0, 150, 136),
            //Green
            Color.ByARGB(0, 76, 175, 80),
            //Light Green
            Color.ByARGB(0, 139, 195, 74),
            //Lime
            Color.ByARGB(0, 80, 86, 22),
            //Yellow
            Color.ByARGB(0, 255, 235, 59),
            //Amber
            Color.ByARGB(0, 255, 193, 7),
            //Orange
            Color.ByARGB(0, 255, 152, 0),
            //Deep Orange
            Color.ByARGB(0, 255, 87, 34),
            //Brown
            Color.ByARGB(0, 121, 85, 72),
            //Grey
            Color.ByARGB(0, 158, 158, 158),
            //Blue Grey
            Color.ByARGB(0, 96, 125, 139)
        };
    }
}
