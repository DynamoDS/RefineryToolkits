using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;
using DSCore;


namespace GenerativeToolkit.Utilities
{
    [IsVisibleInDynamoLibrary(false)]
    public class ColorRange
    {
        [IsVisibleInDynamoLibrary(true)]
        public static List<Color> RandomColorRange(int amount)
        {
            List<DSCore.Color> randomColors = new List<DSCore.Color>();
            List<List<int>> rgbList = RandomRGB(amount);
            foreach (var item in rgbList)
            {
                Color color = DSCore.Color.ByARGB(r: item[0], g: item[1], b: item[2]);
                randomColors.Add(color);
            }
            return randomColors;
        }

        private static List<List<int>> RandomRGB(int n)
        {
            List<List<int>> rgb = new List<List<int>>();
            Random rnd = new Random();
            int r = (int)(rnd.NextDouble() * 256);
            int g = (int)(rnd.NextDouble() * 256);
            int b = (int)(rnd.NextDouble() * 256);
            double step = 256 / n;
            for (int i = 0; i < n; i++)
            {
                r = (int)(r + step) % 256;
                g = (int)(g + step) % 256;
                b = (int)(b + step) % 256;
                rgb.Add(new List<int> { r, g, b });
            }
            return rgb;
        }

        [IsVisibleInDynamoLibrary(true)]
        public static List<Color> ContrastyColorRange(int amount)
        {
            if (amount <= 15)
            {
                Random rnd = new Random();
                List<string> usedColors = new List<string>();
                List<Color> colors = new List<Color>();

                Dictionary<string, List<int>> distinctUniqeColors = new Dictionary<string, List<int>>
                {
                    ["Black"] = new List<int>() { 0, 0, 0 },
                    ["Maroon"] = new List<int>() { 128, 0, 0 },
                    ["Green"] = new List<int>() { 0, 128, 0 },
                    ["Olive"] = new List<int>() { 128, 128, 0 },
                    ["Navy"] = new List<int>() { 0, 0, 128 },
                    ["Purple"] = new List<int>() { 128, 0, 128 },
                    ["Teal"] = new List<int>() { 0, 128, 128 },
                    ["Silver"] = new List<int>() { 192, 192, 192 },
                    ["Grey"] = new List<int>() { 128, 128, 128 },
                    ["Red"] = new List<int>() { 255, 0, 0 },
                    ["Lime"] = new List<int>() { 0, 255, 0 },
                    ["Yellow"] = new List<int>() { 255, 255, 0 },
                    ["Blue"] = new List<int>() { 0, 0, 255 },
                    ["Fuchsia"] = new List<int>() { 255, 0, 255 },
                    ["Aqua"] = new List<int>() { 0, 255, 255 }
                };

                for (int i = 0; i < amount; i++)
                {
                    var randomColor = distinctUniqeColors.ElementAt(rnd.Next(0, distinctUniqeColors.Count));

                    Color color = DSCore.Color.ByARGB(r: randomColor.Value[0], g: randomColor.Value[1], b: randomColor.Value[2]);
                    if (!usedColors.Contains(randomColor.Key))
                    {
                        usedColors.Add(randomColor.Key);
                        colors.Add(color);
                    }
                    else
                    {
                        i -= 1;
                    }

                }
                return colors;
            }
            else
            {
                return null;
            }
        }

        [IsVisibleInDynamoLibrary(true)]
        public static List<Color> RandomColors(int amount, double correctionFactor)
        {
            if (amount <= 19)
            {
                Random rnd = new Random();
                List<string> usedColors = new List<string>();
                List<Color> colors = new List<Color>();

                Dictionary<string, List<int>> colorDict = new Dictionary<string, List<int>>
                {
                    ["Red"] = new List<int>() { 244, 67, 54 },
                    ["Pink"] = new List<int>() { 233, 30, 99 },
                    ["Purple"] = new List<int>() { 156, 39, 176 },
                    ["Deep Purple"] = new List<int>() { 103, 58, 183 },
                    ["Indigo"] = new List<int>() { 63, 81, 181 },
                    ["Blue"] = new List<int>() { 33, 150, 243 },
                    ["Light Blue"] = new List<int>() { 3, 169, 244 },
                    ["Cyan"] = new List<int>() { 0, 188, 212 },
                    ["Teal"] = new List<int>() { 0, 150, 136 },
                    ["Green"] = new List<int>() { 76, 175, 80 },
                    ["Light Green"] = new List<int>() { 139, 195, 74 },
                    ["Lime"] = new List<int>() { 80, 86, 22 },
                    ["Yellow"] = new List<int>() { 255, 235, 59 },
                    ["Amber"] = new List<int>() { 255, 193, 7 },
                    ["Orange"] = new List<int>() { 255, 152, 0 },
                    ["Deep Orange"] = new List<int>() { 255, 87, 34 },
                    ["Brown"] = new List<int>() { 121, 85, 72 },
                    ["Grey"] = new List<int>() { 158, 158, 158 },
                    ["Blue Grey"] = new List<int>() { 96, 125, 139 }
                };

                for (int i = 0; i < amount; i++)
                {
                    var randomColor = colorDict.ElementAt(rnd.Next(0, colorDict.Count));
                    double red = randomColor.Value[0];
                    double green = randomColor.Value[0];
                    double blue = randomColor.Value[0];

                    Color color = DSCore.Color.ByARGB(r: randomColor.Value[0], g: randomColor.Value[1], b: randomColor.Value[2]);
                    if (!usedColors.Contains(randomColor.Key))
                    {
                        usedColors.Add(randomColor.Key);
                        if (correctionFactor < 0)
                        {
                            correctionFactor = 1 + correctionFactor;
                            red *= correctionFactor;
                            green *= correctionFactor;
                            blue *= correctionFactor;
                        }
                        else
                        {
                            red = (255 - red) * correctionFactor + red;
                            green = (255 - green) * correctionFactor + green;
                            blue = (255 - blue) * correctionFactor + blue;
                        }

                        colors.Add(Color.ByARGB(r: (int)red, g: (int)green, b: (int)blue));
                    }
                    else
                    {
                        i -= 1;
                    }

                }
                return colors;
            }
            else
            {
                return null;
            }
        }
    }
}