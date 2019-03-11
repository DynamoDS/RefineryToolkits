using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using DSVector = Autodesk.DesignScript.Geometry.Vector;
using DSPoint = Autodesk.DesignScript.Geometry.Point;
using DSLine = Autodesk.DesignScript.Geometry.Line;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Graphical.Geometry;
using Graphical.Extensions;

namespace GenerativeToolkit.Geometry
{
    
    /// <summary>
    /// Static class extending Point functionality
    /// </summary>
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
        public static DSPoint MidPoint(DSPoint point1, DSPoint point2)
        {
            gVertex midVertex = gVertex.MidVertex(ToVertex(point1), ToVertex(point2));
            return ToPoint(midVertex);
        }

        /// <summary>
        /// Returns the minimum point from a list of points. Minimum is 
        /// evaulated by the point with minimum Y, then X and finally Z coordinate.
        /// </summary>
        /// <param name="points">List of points</param>
        /// <returns name="minPoint">Minimum Point</returns>
        public static DSPoint MinimumPoint(List<DSPoint> points)
        {
            //TODO: Implement a better way of selecting the minimum point.
            return points.OrderBy(p => p.Y).ThenBy(p => p.X).ThenBy(p => p.Z).ToList().First();
        }

        /// <summary>
        /// Order the list of points by the radian angle from a centre point. If angle is equal, closer to centre will be first.
        /// </summary>
        /// <param name="centre">Centre point</param>
        /// <param name="points">Points to order</param>
        /// <returns name="points">Ordered points</returns>
        public static List<DSPoint> OrderByRadianAndDistance(
            [DefaultArgument("Autodesk.DesignScript.Geometry.Point.ByCoordinates(0,0,0);")]DSPoint centre,
            List<DSPoint> points)
        {
            //TODO: Add angle offet input
            List<DSPoint> ordered = points.OrderBy(p => RadAngle(centre, p)).ThenBy(p => centre.DistanceTo(p)).ToList();
            return ordered;
        }

        /// <summary>
        /// Radian angle between two points from a centre.
        /// </summary>
        /// <param name="centre"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static double StartEndRadiansFromCentre(DSPoint centre, DSPoint start, DSPoint end)
        {
            gVertex c = ToVertex(centre);
            gVertex s = ToVertex(start);
            gVertex e = ToVertex(end);
            return gVertex.ArcRadAngle(c, s, e);
        }
        #endregion

        #region Internal Methods
        internal static gVertex ToVertex(this DSPoint point)
        {
            return gVertex.ByCoordinates(point.X, point.Y, point.Z);
        }

        internal static DSPoint ToPoint(this gVertex vertex)
        {
            return DSPoint.ByCoordinates(vertex.X, vertex.Y, vertex.Z);
        }

        internal static bool AreEqual(DSPoint point1, DSPoint point2)
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
        internal static double RadAngle(DSPoint centre, DSPoint point)
        {
            gVertex v1 = gVertex.ByCoordinates(centre.X, centre.Y, centre.Z);
            gVertex v2 = gVertex.ByCoordinates(point.X, point.Y, point.Z);
            return gVertex.RadAngle(v1, v2);
        }

        #endregion

    }
}
