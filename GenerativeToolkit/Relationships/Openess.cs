using Autodesk.DesignScript.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;
using Revit.Elements;
using Autodesk.Revit.DB;

namespace GenerativeToolkit.Relationships
{
    [IsVisibleInDynamoLibrary(false)]
    public class Openess
    {
        #region Public Methods
        [IsVisibleInDynamoLibrary(true)]
        public static List<double> DeskOpeness(List<Room> rooms, List<Revit.Elements.FamilyInstance> elements, List<double> deskWidth, List<double> deskLength, double offset)
        {
            List<Polygon> roomPolygons = HelperFunctions.DeskFunctions.PolygonsFromRooms(rooms);
            List<double> openessScore = new List<double>();
            for (int i = 0; i < elements.Count; i++)
            {
                List<Autodesk.DesignScript.Geometry.Line> intersectionLines = IntersectionLine(elements[i], deskWidth[i], deskLength[i], offset);
                openessScore.Add(OpenessScore(intersectionLines, roomPolygons));
                intersectionLines.ForEach(line => line.Dispose());
            }
            roomPolygons.ForEach(poly => poly.Dispose());

            return openessScore;
        }
        #endregion

        #region Private Methods
        private static List<Autodesk.DesignScript.Geometry.Line> IntersectionLine(Revit.Elements.FamilyInstance element, double deskWidth, double deskLength, double offset)
        {
            Autodesk.DesignScript.Geometry.Point locationPoint = (Autodesk.DesignScript.Geometry.Point) element.GetLocation();
            double width = deskWidth + offset;
            double length = deskLength + offset;

            double rotation = ElementRoation(element);
            Vector x = Vector.XAxis().Rotate(Vector.ZAxis(), -rotation);
            Vector y = Vector.YAxis().Rotate(Vector.ZAxis(), -rotation);

            Autodesk.DesignScript.Geometry.Line rightLine = Autodesk.DesignScript.Geometry.Line.ByStartPointDirectionLength(locationPoint, x, width/2);
            Autodesk.DesignScript.Geometry.Line topLine = Autodesk.DesignScript.Geometry.Line.ByStartPointDirectionLength(locationPoint, y, length / 2);
            Autodesk.DesignScript.Geometry.Line leftLine = Autodesk.DesignScript.Geometry.Line.ByStartPointDirectionLength(locationPoint, x, -width / 2);
            Autodesk.DesignScript.Geometry.Line bottomLine = Autodesk.DesignScript.Geometry.Line.ByStartPointDirectionLength(locationPoint, y, -length / 2);

            List<Autodesk.DesignScript.Geometry.Line> lines = new List<Autodesk.DesignScript.Geometry.Line> { rightLine, topLine, leftLine, bottomLine };
            return lines;
        }

        private static double ElementRoation(Revit.Elements.FamilyInstance element)
        {
            Autodesk.Revit.DB.FamilyInstance unwrappedElement = (Autodesk.Revit.DB.FamilyInstance) element.InternalElement;
            double x = unwrappedElement.HandOrientation.X;
            double y = unwrappedElement.HandOrientation.Y;

            double rotation;
            if (y >= 0)
            {
                rotation = Math.Acos(x) * (180 / Math.PI);
            }
            else
            {
                rotation = (Math.Acos(x) * (180 / Math.PI)) + (2 * (180 - (Math.Acos(x) * (180 / Math.PI))));
            }

            if (rotation == 0)
            {
                rotation = 0;
            }
            else
            {
                rotation = 360 - rotation;
            }
            return rotation;
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
