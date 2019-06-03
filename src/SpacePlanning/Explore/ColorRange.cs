using DSCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Autodesk.RefineryToolkits.SpacePlanning.Explore
{
    public static class Colors
    {
        private const int MaxColorAmount = 19;

        /// <summary>
        /// Creates a given amount of distinct colors which can be used in a color range
        /// </summary>
        /// <param name="amount">Number of colors (max. 19)</param>
        /// <param name="brightness">Correction factor from 0-1</param>
        /// <param name="seed">Seed</param>
        /// <returns>distinct colors</returns>
        public static List<Color> ContrastyColorRange(
            int amount = 19,
            double brightness = 0,
            int seed = 1)
        {
            if (amount > MaxColorAmount)
            {
                throw new System.ArgumentException(string.Format("Maximum number of colours supported right now is {0}", MaxColorAmount));
            }
            Random rnd = new Random(seed);
            List<int> indexRanges = Enumerable.Range(0, MaxColorAmount).ToList();
            List<int> shuffledIndexes = indexRanges.OrderBy(x => rnd.Next()).ToList();

            List<Color> colors = new List<Color>();
            foreach (int i in shuffledIndexes.GetRange(0, amount))
            {
                Color color = ColorPalette[i];
                int red;
                int green;
                int blue;

                if (brightness <= 0)
                {
                    red = Convert.ToInt32(color.Red * (1 + brightness));
                    green = Convert.ToInt32(color.Green * (1 + brightness));
                    blue = Convert.ToInt32(color.Blue * (1 + brightness));
                }
                else
                {
                    red = Convert.ToInt32((255 - color.Red) * brightness + color.Red);
                    green = Convert.ToInt32((255 - color.Green) * brightness + color.Green);
                    blue = Convert.ToInt32((255 - color.Blue) * brightness + color.Blue);
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
