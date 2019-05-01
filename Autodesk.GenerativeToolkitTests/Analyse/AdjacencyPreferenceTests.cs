using Autodesk.GenerativeToolkit.Analyse;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestServices;
using Autodesk.DesignScript.Geometry;

namespace Autodesk.GenerativeToolkit.Analyse.Tests
{
    [TestFixture]
    public class AdjacencyPreferenceTests : GeometricTestBase
    {
        /// <summary>
        /// Geometric median of an Obtuse Triangle is the vertex at the angle > 120 degrees
        /// </summary>
        [Test]
        public void GeometricMedianReturnVertexAtAngleEqualToOrAbove120OnObtuseTriangleTest()
        { 
            // Create sample points that makes a Obtuse triangle
            List<Point> samplePoints = new List<Point>
            {
                Point.ByCoordinates(0,0),
                Point.ByCoordinates(10,5),
                // Vertex at angle > 120
                Point.ByCoordinates(5,0)
            };

            // Get the geometric median point
            Point vertexOnObtuseAngle = AdjacencyPreference.GeometricMedian(samplePoints);

            // Check if both X and Y of the geometric median is the same as X and Y of the vertex at angle > 120 in the obtuse triangle.
            Assert.AreEqual(samplePoints[2].X, vertexOnObtuseAngle.X);
            Assert.AreEqual(samplePoints[2].Y, vertexOnObtuseAngle.Y);

            // Dispose unused geometry
            samplePoints.ForEach(p => p.Dispose());
            vertexOnObtuseAngle.Dispose();

        }


        /// <summary>
        /// Geometric median of an Acute Triangle is the point inside the triangle
        /// which subtends an angle of 120° to each three pairs of triangle vertices
        /// </summary>
        [Test]
        public void GeometricMedianReturnsFermatPointOnAcuteTriangleTest()
        {
            // Create sample points that makes a Acute triangle
            List<Point> samplePoints = new List<Point>
            {
                Point.ByCoordinates(5,5),
                Point.ByCoordinates(9.5,4.2),
                Point.ByCoordinates(8,3)
            };

            // Get the geometric median of the Acute triangles sample points
            // Also known as the Fermat Point
            Point fermatPoint = AdjacencyPreference.GeometricMedian(samplePoints);

            // Create vectors from the fermat point to two arbitrary points in the sample points list.
            List<Vector> vectors = new List<Vector>
            {
                Vector.ByTwoPoints(fermatPoint, samplePoints[0]),
                Vector.ByTwoPoints(fermatPoint, samplePoints[1]),
            };

            // Getting the angle between the two vectors just created.
            // This angle should always be 120 degrees in a Acute triangle.
            double testAngle = vectors[0].AngleWithVector(vectors[1]);

            // Checking if the angle is equal to 120 degrees.
            Assert.AreEqual(120.0, Math.Round(testAngle,1));

            // Dispose unused geometry
            samplePoints.ForEach(p => p.Dispose());
            fermatPoint.Dispose();
            vectors.ForEach(p => p.Dispose());
        }

        /// <summary>
        /// Geometric median of an Concave Quadrilateral is the point
        /// inside the triangle formed by remaining 3 points
        /// </summary>
        [Test]
        public void GeometricMedianReturnsThePointInsideTriangleFormedByRemainingPointsOnConcaveQuadrilateralTest()
        {
            // Create sample points that makes a Concave Quadrilateral
            List<Point> samplePoints = new List<Point>
            {
                //Point inside triangle formed by remaining 3 points
                Point.ByCoordinates(7,8),
                //Remaining point
                Point.ByCoordinates(9.5,7.2),
                Point.ByCoordinates(8,4.8),
                Point.ByCoordinates(6,9)
            };

            // Get the geometric median of the Concave Quadrilateral sample points
            // which is the point inside the triangle formed by remaining 3 points
            Point geometricMedianPoint = AdjacencyPreference.GeometricMedian(samplePoints);

            // Check if both X and Y of the geometric median is the same as X and Y of the 
            // point inside the triangle formed by the remaining points (samplePoints[0]).
            Assert.AreEqual(samplePoints[0].X, geometricMedianPoint.X);
            Assert.AreEqual(samplePoints[0].Y, geometricMedianPoint.Y);

            // Dispose unused geometry.
            samplePoints.ForEach(p => p.Dispose());
            geometricMedianPoint.Dispose();
        }

    }
}