using Autodesk.DesignScript.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodesk.GenerativeToolkit.Analyse
{
    public static class AdjacencyPreference
    {
        public static Point CommonPointByPoints(List<Point> points)
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
            double lowerLimit = 0.01;
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
    }
}
