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
    public class DeskLayoutTests : GeometricTestBase
    {
        private Surface surface;

        [SetUp]
        public void BeforeTest()
        {
            Rectangle layoutPolygon = Rectangle.ByWidthLength(500, 500);
            surface = Surface.ByPatch(layoutPolygon);
        }

        /// <summary>
        /// Checks if the output of the Desk Layout is the correct type
        /// </summary>
        [Test]
        public void DeskLayoutOutputTypeIsCorrectTest()
        {
            var result = DeskLayout.Create(surface, 50, 10, 50);
            Assert.IsTrue(result is List<Rectangle>);
        }

        /// <summary>
        /// Check if output returns correct number of Rectangles
        /// </summary>
        [Test]
        public void DesksCreatedIsCorrectTest()
        {
            List<Rectangle> result = DeskLayout.Create(surface, 50, 10, 50) as List<Rectangle>;
            Assert.IsTrue(result.Count == 112);
        }
    }
}