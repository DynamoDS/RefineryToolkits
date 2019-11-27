using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.RefineryToolkits.Core.Geometry;
using Autodesk.RefineryToolkits.Core.Utillites;
using MIConvexHull;
using System;
using System.Collections.Generic;
using System.Linq;
using GTGeom = Autodesk.RefineryToolkits.Core.Geometry;

namespace Autodesk.RefineryToolkits.SpacePlanning.Analyze
{
    public static class Adjacency
    {
        /// <summary>
        /// Returns the geometric median point of a list of points, 
        /// which is the point minimizing the sum of distances to all supplied points.
        /// </summary>
        /// <param name="points">List of points to compute geometric median for.</param>
        /// <returns>Point that minimizes the distance to all other points</returns>
        public static Point GeometricMedianOfPoints(List<Point> points)
        {
            var n = points.Count;

            // If 3 points return either the vertex at the angle >= 120
            // or the Fermat point
            if (n == 3)
            {
                var vertexAtAngle = VertexAtAngle(points[0], points[1], points[2]);
                return vertexAtAngle ?? FermatPoint(points);
            }
            // If 4 points return 
            // the midpoint when points are colinear
            // or either the point inside the convex hull
            // or the crossing point of the diagonals of the quadrilateral
            if (n == 4)
            {
                // when points are colinear, return the mid point of the line formed by all points
                if (points.AreColinear())
                {
                    Point min = GTGeom.Points.MinimumPoint(points);
                    Point max = GTGeom.Points.MaximumPoint(points);
                    Point mid = GTGeom.Points.MidPoint(min, max);
                    min.Dispose();
                    max.Dispose();

                    return mid;
                }

                List<Point> convexHull = ConvexHull(points);
                if (convexHull.Count == 3)
                {
                    return points
                        .First(p => !convexHull.Any(p2 => p2.X == p.X));
                }
                else
                {
                    var lineAC = Line.ByStartPointEndPoint(convexHull[0], convexHull[2]);
                    var lineBD = Line.ByStartPointEndPoint(convexHull[1], convexHull[3]);

                    var point = lineAC.Intersect(lineBD).First() as Point;
                    lineAC.Dispose();
                    lineBD.Dispose();

                    return point;
                }
            }

            // Else return the point that minimizes the distance to the sample points
            // https://www.geeksforgeeks.org/geometric-median/
            var testPoints = new List<Point>
            {
                Point.ByCoordinates(-1.0,0.0),
                Point.ByCoordinates(0.0,1.0),
                Point.ByCoordinates(1.0,0.0),
                Point.ByCoordinates(0.0,-1.0)
            };

            Point commonPoint = CommonPointByPoints(testPoints, points, n);
            testPoints.ForEach(x => x.Dispose());

            return commonPoint;
        }

        #region Private Methods

        /// <summary>
        /// returns the geometric median of a list of points bigger than 4 and smaller than 3
        /// </summary>
        /// <returns>geometric median of a list of points bigger than 4 and smaller than 3</returns>
        private static Point CommonPointByPoints(
            List<Point> testPoints,
            List<Point> points,
            int n)
        {
            // Assume test_distance to be 1000 
            double testDistance = 1000;

            double currentX = 0;
            double currentY = 0;
            //Point currentPoint = Point.ByCoordinates(0,0);

            for (var i = 0; i < n; i++)
            {
                currentX += points[i].X;
                currentY += points[i].Y;
            }

            // Here current_point becomes the 
            // Geographic MidPoint 
            // Or Center of Gravity of equal 
            // discrete mass distributions 
            currentX /= n;
            currentY /= n;

            // minimum_distance becomes sum of 
            // all distances from MidPoint to 
            // all given points 
            var minimumDistance = DistanceSum(Point.ByCoordinates(currentX, currentY), points, n);

            var k = 0;
            while (k < n)
            {
                for (var i = 0; i < n; i++)
                {
                    if (i != k)
                    {
                        var newDistance = DistanceSum(points[i], points, n);
                        if (newDistance < minimumDistance)
                        {
                            minimumDistance = newDistance;
                            currentX = points[i].X;
                            currentX = points[i].Y;
                        }
                    }
                }
                k++;
            }

            var flag = 0;

            // Lowest Limit till which we are going 
            // to run the main while loop 
            // Lower the Limit higher the accuracy 
            const double lowerLimit = 0.001;
            // Test loop for approximation starts here 
            while (testDistance > lowerLimit)
            {
                flag = 0;

                // Loop for iterating over all 4 neighbours 
                for (var i = 0; i < 4; i++)
                {
                    // Finding Neighbours done 
                    var newX = currentX + (testDistance * testPoints[i].X);
                    var newY = currentY + (testDistance * testPoints[i].Y);

                    // New sum of Euclidean distances 
                    // from the neighbor to the given 
                    // data points 
                    var newDistance = DistanceSum(Point.ByCoordinates(newX, newY), points, n);

                    if (newDistance < minimumDistance)
                    {
                        // Approximating and changing 
                        // current_point 
                        minimumDistance = newDistance;
                        currentX = newX;
                        currentY = newY;
                        flag = 1;
                        break;
                    }
                }

                // This means none of the 4 neighbours 
                // has the new minimum distance, hence 
                // we divide by 2 and reiterate while 
                // loop for better approximation 
                if (flag == 0)
                {
                    testDistance /= 2;
                }
            }
            return Point.ByCoordinates(currentX, currentY);
        }

        /// <summary>
        /// Calculates the sum of Euclidean Distances 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="points"></param>
        /// <param name="n"></param>
        /// <returns>sum of Euclidean Distances</returns>
        private static double DistanceSum(
            Point point,
            List<Point> points,
            int n)
        {
            double sum = 0;
            for (var i = 0; i < n; i++)
            {
                var distx = Math.Abs(points[i].X - point.X);
                var disty = Math.Abs(points[i].Y - point.Y);
                sum += Math.Sqrt((distx * distx) + (disty * disty));
            }

            return sum;
        }

        /// <summary>
        /// If points are triangle with angle bigger than or equal to 120, return vertex at that angle
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="C"></param>
        /// <returns>Vertex at angle than or equal to 120</returns>
        private static Point VertexAtAngle(
            Point A,
            Point B,
            Point C)
        {
            // Square of lengths be a2, b2, c2 
            var a2 = LengthSquare(B, C);
            var b2 = LengthSquare(A, C);
            var c2 = LengthSquare(A, B);

            // lenght of sides be a, b, c 
            var a = Math.Sqrt(a2);
            var b = Math.Sqrt(b2);
            var c = Math.Sqrt(c2);

            // From Cosine law 
            var alpha = Math.Acos((b2 + c2 - a2) / (2 * b * c));
            var betta = Math.Acos((a2 + c2 - b2) / (2 * a * c));
            var gamma = Math.Acos((a2 + b2 - c2) / (2 * a * b));

            // Converting to degree 
            alpha = Math.Ceiling(alpha * 180 / Math.PI);
            betta = Math.Ceiling(betta * 180 / Math.PI);
            gamma = Math.Ceiling(gamma * 180 / Math.PI);

            if (alpha >= 120)
            {
                return A;
            }
            if (betta >= 120)
            {
                return B;
            }
            if (gamma >= 120)
            {
                return C;
            }

            return null;
        }

        /// <summary>
        /// returns square of distance b/w two points 
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <returns>distance b/w two points</returns>
        private static double LengthSquare(
            Point pt1,
            Point pt2)
        {
            var xDiff = pt1.X - pt2.X;
            var yDiff = pt1.Y - pt2.Y;
            return (xDiff * xDiff) + (yDiff * yDiff);
        }

        /// <summary>
        /// finds the common point of a triangle with angles bigger than 120
        /// </summary>
        /// <param name="points"></param>
        /// <returns>common point of a triangle with angles bigger than 120</returns>
        private static Point FermatPoint(List<Point> points)
        {
            var sortedPoints = points
                .OrderBy(pt => pt.Y)
                .ThenBy(pt => pt.X)
                .Reverse()
                .ToList();

            Point pointA = sortedPoints[0];
            Point pointB = sortedPoints[1];
            Point pointC = sortedPoints[2];

            var orient = GTGeom.Vertex.Orientation(pointA.ToVertex(), pointB.ToVertex(), pointC.ToVertex());
            var angleForRotation = orient == -1 ? 90 : 270;

            DesignScript.Geometry.Vector vectorAB = DesignScript.Geometry.Vector.ByTwoPoints(pointA, pointB).Rotate(DesignScript.Geometry.Vector.ZAxis(), angleForRotation);
            DesignScript.Geometry.Vector vectorAC = DesignScript.Geometry.Vector.ByTwoPoints(pointC, pointA).Rotate(DesignScript.Geometry.Vector.ZAxis(), angleForRotation);

            var midpointAB = Point.ByCoordinates((pointA.X + pointB.X) / 2, (pointA.Y + pointB.Y) / 2);
            var midpointAC = Point.ByCoordinates((pointA.X + pointC.X) / 2, (pointA.Y + pointC.Y) / 2);

            var sideABLength = Math.Sqrt(Math.Pow(pointA.X - pointB.X, 2) + Math.Pow(pointA.Y - pointB.Y, 2));
            var sideACLength = Math.Sqrt(Math.Pow(pointA.X - pointC.X, 2) + Math.Pow(pointA.Y - pointC.Y, 2));

            var equilateralLengthAB = sideABLength * Math.Sqrt(3) / 2;
            var equilateralLengthAC = sideACLength * Math.Sqrt(3) / 2;

            var equilateralTopPointAB = midpointAB.Translate(vectorAB, equilateralLengthAB) as Point;
            var equilateralTopPointAC = midpointAC.Translate(vectorAC, equilateralLengthAC) as Point;

            // we clumsily handle which point to draw median line to
            var lineABtoC = Line.ByStartPointEndPoint(equilateralTopPointAB, pointC);
            var lineACtoB = Line.ByStartPointEndPoint(equilateralTopPointAC, pointB);

            var lineABtoB = Line.ByStartPointEndPoint(equilateralTopPointAB, pointB);
            var lineACtoC = Line.ByStartPointEndPoint(equilateralTopPointAC, pointC);

            var fermatPt = lineABtoC
                .Intersect(lineACtoB)
                .FirstOrDefault() as Point;

            var fermat2 = lineABtoB
                .Intersect(lineACtoC)
                .FirstOrDefault() as Point;

            // dispose of all intermediate Dynamo geometry
            equilateralTopPointAB.Dispose();
            equilateralTopPointAC.Dispose();
            midpointAB.Dispose();
            midpointAC.Dispose();
            lineABtoC.Dispose();
            lineACtoB.Dispose();

            return fermatPt ?? fermat2;
        }

        /// <summary>
        /// returns the Convex Hull of a list of points
        /// </summary>
        /// <param name="points"></param>
        /// <returns>Convex Hull</returns>
        private static List<Point> ConvexHull(List<Point> points)
        {
            var vertices = new Vertex[points.Count];
            for (var i = 0; i < points.Count; i++)
            {
                vertices[i] = new Vertex(points[i].X, points[i].Y);
            }

            ConvexHullCreationResult<Vertex, DefaultConvexFace<Vertex>> convexHull = MIConvexHull.ConvexHull.Create(vertices);
            if (convexHull.Result == null) throw new ArgumentOutOfRangeException("Could not create convex hull, check your points are not co-linear.");
            var hullPoints = convexHull.Result.Points.ToList();

            var convexHullPoints = new List<Point>();
            foreach (Vertex vertex in hullPoints)
            {
                convexHullPoints.Add(Point.ByCoordinates(vertex.Position[0], vertex.Position[1]));
            }

            return convexHullPoints;
        }

        #endregion

        [IsVisibleInDynamoLibrary(false)]
        private class Vertex : IVertex
        {
            public Vertex(double x, double y)
            {
                this.Position = new double[2] { x, y };
            }

            public double[] Position { get; set; }
        }
    }
}
