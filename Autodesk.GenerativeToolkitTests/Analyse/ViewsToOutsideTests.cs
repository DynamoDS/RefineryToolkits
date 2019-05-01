using NUnit.Framework;
using Autodesk.GenerativeToolkit.Analyse;
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
    public class ViewsToOutsideTests : GeometricTestBase
    {
        /// <summary>
        /// Checks if views to outside returns the right value in a layout with no obstacles
        /// </summary>
        [Test]
        public void ViewToOutside360()
        {
            // Create boundary polygon with no obstacles
            Rectangle boundaryRect = Rectangle.ByWidthLength(20, 20);
            List<Curve> lines = boundaryRect.Explode().Cast<Curve>().ToList();            
            List<Point> stPoints = new List<Point>();
            List<Point> endPoints = new List<Point>();
            foreach (Curve line in lines)
            {
                stPoints.Add(line.StartPoint);
                endPoints.Add(line.EndPoint);
            }
            List<Polygon> boundaryPoly = new List<Polygon> { Polygon.ByPoints(stPoints) };

            // Create origin point 
            Point origin = Point.ByCoordinates(0, 0);

            // Result of ViewsToOutside.ByLineSegments
            var result = ViewsToOutside.ByLineSegments(lines,origin,boundaryPoly,new List<Polygon> { });

            // Check if output of node is a Dictionary that contains both the
            // "score" and "segments" key
            Assert.IsTrue(result.Keys.Contains("score"));
            Assert.IsTrue(result.Keys.Contains("segments"));

            // Check if the score output is 1.0
            // as there are no obstacles blocking the views to outside
            var viewScore = (double)result["score"];
            Assert.AreEqual(1.0,viewScore);
        }
    }
}