using Autodesk.DesignScript.Runtime;
using DSCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodesk.GenerativeToolkit.Explore
{
    class ColorRange
    {
        // Colors from https://www.materialpalette.com/colors
        [IsVisibleInDynamoLibrary(true)]
        public static List<Color> ContrastyColorRange(int amount, double correctionFactor)
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
                    double green = randomColor.Value[1];
                    double blue = randomColor.Value[2];

                    Color color = DSCore.Color.ByARGB(r: randomColor.Value[0], g: randomColor.Value[1], b: randomColor.Value[2]);
                    if (!usedColors.Contains(randomColor.Key))
                    {
                        usedColors.Add(randomColor.Key);
                        if (correctionFactor <= 0)
                        {
                            red *= 1 + correctionFactor;
                            green *= 1 + correctionFactor;
                            blue *= 1 + correctionFactor;
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
