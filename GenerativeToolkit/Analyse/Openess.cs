using Autodesk.DesignScript.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;

namespace Analyse
{
    [IsVisibleInDynamoLibrary(false)]
    public class Openess
    {
        #region Public Methods
        [IsVisibleInDynamoLibrary(true)]
        public static double FromPoint(Point origin, List<Polygon> obstacles, double objectWidth, double objectLength, double rotation)
        {
            
            List<Autodesk.DesignScript.Geometry.Line> intersectionLines = IntersectionLine(origin, objectWidth, objectLength, rotation);
            double openessScore = OpenessScore(intersectionLines, obstacles);
            intersectionLines.ForEach(line => line.Dispose());

            return openessScore;
        }
        #endregion

        #region Private Methods
        private static List<Autodesk.DesignScript.Geometry.Line> IntersectionLine(Point origin, double width, double length, double rotation)
        {
            Vector x = Vector.XAxis().Rotate(Vector.ZAxis(), -rotation);
            Vector y = Vector.YAxis().Rotate(Vector.ZAxis(), -rotation);

            Autodesk.DesignScript.Geometry.Line rightLine = Autodesk.DesignScript.Geometry.Line.ByStartPointDirectionLength(origin, x, width/2);
            Autodesk.DesignScript.Geometry.Line topLine = Autodesk.DesignScript.Geometry.Line.ByStartPointDirectionLength(origin, y, length / 2);
            Autodesk.DesignScript.Geometry.Line leftLine = Autodesk.DesignScript.Geometry.Line.ByStartPointDirectionLength(origin, x, -width / 2);
            Autodesk.DesignScript.Geometry.Line bottomLine = Autodesk.DesignScript.Geometry.Line.ByStartPointDirectionLength(origin, y, -length / 2);

            List<Autodesk.DesignScript.Geometry.Line> lines = new List<Autodesk.DesignScript.Geometry.Line> { rightLine, topLine, leftLine, bottomLine };
            return lines;
        }

        private static double OpenessScore(List<Autodesk.DesignScript.Geometry.Line> lines, List<Polygon> polygons)
        {
            double score = 0.0;
            foreach (var line in lines)
            {
                foreach (var polygon in polygons)
                {
                    if (line.DoesIntersect(polygon))
                    {
                        score += 0.25;
                    }
                }
            }
            return score;
        }
        #endregion
    }
}
