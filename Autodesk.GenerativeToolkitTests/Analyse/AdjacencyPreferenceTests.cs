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
        // Geometric median of an Obtuse Triangle is the vertex at the angle > 120 degrees
        [Test]
        public void GeometricMedianOnObtuseTriangleTest()
        {           
            List<Point> samplePoints = new List<Point>
            {
                Point.ByCoordinates(0,0),
                Point.ByCoordinates(10,5),
                // Vertex at angle > 120
                Point.ByCoordinates(5,0)
            };

            Point vertexOnObtuseAngle = AdjacencyPreference.GeometricMedian(samplePoints);

            Assert.AreEqual(samplePoints[2].X, vertexOnObtuseAngle.X);
            Assert.AreEqual(samplePoints[2].Y, vertexOnObtuseAngle.Y);

            samplePoints.ForEach(p => p.Dispose());
            vertexOnObtuseAngle.Dispose();

        }

        // Geometric median of an Acute Triangle is the point inside the triangle
        // which subtends an angle of 120° to each three pairs of triangle vertices
        [Test]
        public void FermatPointTest()
        {
            List<Point> samplePoints = new List<Point>
            {
                Point.ByCoordinates(5,5),
                Point.ByCoordinates(9.5,4.2),
                Point.ByCoordinates(8,3)
            };

            Point fermatPoint = AdjacencyPreference.GeometricMedian(samplePoints);

            List<Vector> vectors = new List<Vector>
            {
                Vector.ByTwoPoints(fermatPoint, samplePoints[0]),
                Vector.ByTwoPoints(fermatPoint, samplePoints[1]),
            };

            double testAngle = vectors[0].AngleWithVector(vectors[1]);

            Assert.AreEqual(120.0, Math.Round(testAngle,1));

            samplePoints.ForEach(p => p.Dispose());
            fermatPoint.Dispose();
            vectors.ForEach(p => p.Dispose());
        }

        // Geometric median of an Concave Quadrilateral is the point
        // inside the triangle formed by remaining 3 points
        [Test]
        public void ConcaveQuadrilateralTest()
        {
            List<Point> samplePoints = new List<Point>
            {
                //Point inside triangle formed by remaining 3 points
                Point.ByCoordinates(7,8),
                //Remaining point
                Point.ByCoordinates(9.5,7.2),
                Point.ByCoordinates(8,4.8),
                Point.ByCoordinates(6,9)
            };

            Point geometricMedianPoint = AdjacencyPreference.GeometricMedian(samplePoints);

            Assert.AreEqual(samplePoints[0].X, geometricMedianPoint.X);
            Assert.AreEqual(samplePoints[0].Y, geometricMedianPoint.Y);

            samplePoints.ForEach(p => p.Dispose());
            geometricMedianPoint.Dispose();
        }

    }
}