using Autodesk.DesignScript.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Autodesk.GenerativeToolkit.Analyse
{
    public static class AdjacencyPreference
    {
        //Todo if points form convex quadrilateral the geometric median is the crossing point of the diagonals of the quadrilateral
        //Radon Point
        public static Point CommonPointByPoints(List<Point> points)
        {
            if (points.Count == 3)
            {
                if (VertexAtAngle(points[0], points[1], points[2]) != null)
                {
                    return VertexAtAngle(points[0], points[1], points[2]);
                }
                else
                {
                    return FermatPoint(points);
                }
                
            }

            else
            {
                List<Point> testPoints = new List<Point>
                {
                    Point.ByCoordinates(-1.0,0.0),
                    Point.ByCoordinates(0.0,1.0),
                    Point.ByCoordinates(1.0,0.0),
                    Point.ByCoordinates(0.0,-1.0)
                };

                int n = points.Count;
                Point commonPoint = GeometricMedian(testPoints, points, n);

                return commonPoint;
            }
            
        }

        private static double DistanceSum(Point point, List<Point> points, int n)
        {
            double sum = 0;
            for (int i = 0; i < n; i++)
            {
                double distx = Math.Abs(points[i].X - point.X);
                double disty = Math.Abs(points[i].Y - point.Y);
                sum += Math.Sqrt((distx * distx) + (disty * disty));
            }
            // Return the sum of Euclidean Distances 
            return sum;
        }

        private static Point GeometricMedian(List<Point> testPoints, List<Point> points, int n)
        {
            double currentX = 0;
            double currentY = 0;
            //Point currentPoint = Point.ByCoordinates(0,0);

            for (int i = 0; i < n; i++)
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
            double minimumDistance = DistanceSum(Point.ByCoordinates(currentX,currentY), points, n);

            int k = 0;
            while (k < n) 
            {
                for (int i = 0; i < n ; i++)
                {
                    if (i != k)
                    {
                        double newDistance = DistanceSum(points[i], points, n);
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

            // Assume test_distance to be 1000 
            double testDistance = 1000;
            int flag = 0;

            // Lowest Limit till which we are going 
            // to run the main while loop 
            // Lower the Limit higher the accuracy 
            double lowerLimit = 0.001;
            // Test loop for approximation starts here 
            while (testDistance > lowerLimit)
            {
                flag = 0;

                // Loop for iterating over all 4 neighbours 
                for (int i = 0; i < 4; i++)
                {
                    // Finding Neighbours done 
                    double newX = currentX + (double)testDistance * testPoints[i].X;
                    double newY = currentY + (double)testDistance * testPoints[i].Y;

                    // New sum of Euclidean distances 
                    // from the neighbor to the given 
                    // data points 
                    double newDistance = DistanceSum(Point.ByCoordinates(newX, newY), points, n);

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

        // if points are triangle with angel > 120, return vertex at that angle
        private static Point VertexAtAngle(Point A, Point B, Point C)
        {
            // Square of lengths be a2, b2, c2 
            double a2 = LengthSquare(B, C);
            double b2 = LengthSquare(A, C);
            double c2 = LengthSquare(A, B);

            // lenght of sides be a, b, c 
            double a = Math.Sqrt(a2);
            double b = Math.Sqrt(b2);
            double c = Math.Sqrt(c2);

            // From Cosine law 
            double alpha = Math.Acos((b2 + c2 - a2) / (2 * b * c));
            double betta = Math.Acos((a2 + c2 - b2) / (2 * a * c));
            double gamma = Math.Acos((a2 + b2 - c2) / (2 * a * b));

            // Converting to degree 
            alpha = Math.Ceiling(alpha * 180 / Math.PI);
            betta = Math.Ceiling(betta * 180 / Math.PI);
            gamma = Math.Ceiling(gamma * 180 / Math.PI);
            

            if(alpha >= 120)
            {
                return A;
            }
            if(betta >= 120)
            {
                return B;
            }
            if(gamma >= 120)
            {
                return C;
            }

            return null;
        }

        // returns square of distance b/w two points 
        private static double LengthSquare(Point pt1, Point pt2)
        {
            double xDiff = pt1.X - pt2.X;
            double yDiff = pt1.Y - pt2.Y;
            return xDiff * xDiff + yDiff * yDiff;
        }

        private static Point FermatPoint(List<Point> points)
        {
            List<Point> sortedPoints = points.OrderBy(pt => pt.Y).Reverse().ToList();

            Vector vectorAB = Vector.ByTwoPoints(sortedPoints[0], sortedPoints[1]).Rotate(Vector.ZAxis(), 90);
            Vector vectorAC = Vector.ByTwoPoints(sortedPoints[2], sortedPoints[0]).Rotate(Vector.ZAxis(), 90);
          
            Point midpointAB = Point.ByCoordinates((sortedPoints[0].X + sortedPoints[1].X) / 2, (sortedPoints[0].Y + sortedPoints[1].Y) / 2);
            Point midpointAC = Point.ByCoordinates((sortedPoints[0].X + sortedPoints[2].X) / 2, (sortedPoints[0].Y + sortedPoints[2].Y) / 2);

            double sideABLength = Math.Sqrt(Math.Pow((sortedPoints[0].X - sortedPoints[1].X), 2) + Math.Pow((sortedPoints[0].Y - sortedPoints[1].Y), 2));
            double sideACLength = Math.Sqrt(Math.Pow((sortedPoints[0].X - sortedPoints[2].X), 2) + Math.Pow((sortedPoints[0].Y - sortedPoints[2].Y), 2));

            double equilateralLengthAB = (sideABLength * Math.Sqrt(3)) / 2;
            double equilateralLengthAC = (sideACLength * Math.Sqrt(3)) / 2;

            Point equilateralTopPointAB = midpointAB.Translate(vectorAB, equilateralLengthAB) as Point;
            Point equilateralTopPointAC = midpointAC.Translate(vectorAC, equilateralLengthAC) as Point;

            Line lineABC = Line.ByStartPointEndPoint(equilateralTopPointAB, sortedPoints[2]);
            Line lineACB = Line.ByStartPointEndPoint(equilateralTopPointAC, sortedPoints[1]);

            Point fermatPt = lineABC.Intersect(lineACB)[0] as Point;

            return fermatPt;
        }

    }
}
