/***************************************************************************************
* This code is originally created by Alvaro Alvaro Ortega Pickmans, and is available
* in his Graphical Packages
* Title: Graphical
* Author: Alvaro Ortega Pickmans
* Date: 2017
* Availability: https://github.com/alvpickmans/Graphical
*
***************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.RefineryToolkits.Core.Utillites;

namespace Autodesk.RefineryToolkits.Core.Geometry
{
    public static class Points
    {
        //Vector-plane intersection https://math.stackexchange.com/questions/100439/determine-where-a-vector-will-intersect-a-plane

        #region Public Methods

        /// <summary>
        /// Returns the mid point between two points.
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        public static Point MidPoint(Point point1, Point point2)
        {
            Vertex midVertex = Vertex.MidVertex(ToVertex(point1), ToVertex(point2));
            return ToPoint(midVertex);
        }

        /// <summary>
        /// Returns the minimum point from a list of points. Minimum is 
        /// evaluated by the point with minimum Y, then X and finally Z coordinate.
        /// </summary>
        /// <param name="points">List of points</param>
        /// <returns name="minPoint">Minimum Point</returns>
        public static Point MinimumPoint(List<Point> points)
        {
            //TODO: Implement a better way of selecting the minimum point.
            return points
                .OrderBy(p => p.Y)
                .ThenBy(p => p.X)
                .ThenBy(p => p.Z)
                .ToList()
                .First();
        }

        /// <summary>
        /// Returns the maximum point from a list of points. Maximum is 
        /// evaluated by the point with maximum Y, then X and finally Z coordinate.
        /// </summary>
        /// <param name="points">List of points</param>
        /// <returns name="minPoint">Maximum Point</returns>
        public static Point MaximumPoint(List<Point> points)
        {
            //TODO: Implement a better way of selecting the maximum point.
            return points
                .OrderBy(p => p.Y)
                .ThenBy(p => p.X)
                .ThenBy(p => p.Z)
                .ToList()
                .Last();
        }

        /// <summary>
        /// Order the list of points by the radian angle from a centre point. If angle is equal, closer to centre will be first.
        /// </summary>
        /// <param name="centre">Centre point</param>
        /// <param name="points">Points to order</param>
        /// <returns name="points">Ordered points</returns>
        public static List<Point> OrderByRadianAndDistance(
            [DefaultArgument("Autodesk.DesignScript.Geometry.Point.ByCoordinates(0,0,0);")]Point centre,
            List<Point> points)
        {
            //TODO: Add angle offet input
            return points
                .OrderBy(p => RadAngle(centre, p))
                .ThenBy(p => centre.DistanceTo(p))
                .ToList();
        }

        /// <summary>
        /// Radian angle between two points from a centre.
        /// </summary>
        /// <param name="centre"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static double StartEndRadiansFromCentre(Point centre, Point start, Point end)
        {
            Vertex c = ToVertex(centre);
            Vertex s = ToVertex(start);
            Vertex e = ToVertex(end);
            return Vertex.ArcRadAngle(c, s, e);
        }

        #endregion

        #region Internal Methods
        public static Vertex ToVertex(this Point point)
        {
            return Vertex.ByCoordinates(point.X, point.Y, point.Z);
        }

        public static Point ToPoint(this Vertex vertex)
        {
            return Point.ByCoordinates(vertex.X, vertex.Y, vertex.Z);
        }

        internal static bool AreEqual(Point point1, Point point2)
        {
            return point1.X.AlmostEqualTo(point2.X) &&
                point1.Y.AlmostEqualTo(point2.Y) &&
                point1.Z.AlmostEqualTo(point2.Z);
        }


        /// <summary>
        /// Radian angle from a centre to another point
        /// </summary>
        /// <param name="centre"></param>
        /// <param name="point"></param>
        /// <returns name="rad">Radians</returns>
        internal static double RadAngle(Point centre, Point point)
        {
            Vertex v1 = Vertex.ByCoordinates(centre.X, centre.Y, centre.Z);
            Vertex v2 = Vertex.ByCoordinates(point.X, point.Y, point.Z);
            return Vertex.RadAngle(v1, v2);
        }

        #endregion
    }
}
