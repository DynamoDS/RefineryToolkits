using NUnit.Framework;
using Autodesk.RefineryToolkits.SpacePlanning.Generate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestServices;
using Autodesk.DesignScript.Geometry;

namespace Autodesk.RefineryToolkits.SpacePlanning.Generate.Tests
{
    [TestFixture]
    public class AmenitySpaceTests : GeometricTestBase
    {
        private Surface surface;

        [SetUp]
        public void BeforeTest()
        {
            Rectangle layoutPolygon = Rectangle.ByWidthLength(500, 500);
            surface = Surface.ByPatch(layoutPolygon);
        }

        /// <summary>
        /// Checks if the putput dictionary contains the necessary keys 
        /// </summary>
        [Test]
        public void AmenitySpaceOutputDictionaryTest()
        {
            var result = AmenitySpace.Create(surface, 100, 200);

            // Check if the result of the Shortest path is a dictionary containing the keys "path" and "length"
            Assert.IsTrue(result.Keys.Contains("amenitySrf"));
            Assert.IsTrue(result.Keys.Contains("remainSrf"));
        }

        /// <summary>
        /// Checks if the area of the area returned from the amenitySpace surface is correct
        /// </summary>
        [Test]
        public void AmenityAreaIsCorrect()
        {
            var result = AmenitySpace.Create(surface, 100, 200);

            double area = Math.Round(result["amenitySrf"].Area);

            Assert.AreEqual(60000, area);
        }

        [Test]
        public void RemainingSurfaceIsCorrect()
        {
            var result = AmenitySpace.Create(surface, 100, 200);

            double area = Math.Round(result["remainSrf"].Area);

            Assert.AreEqual(30000, area);
        }
    }
}