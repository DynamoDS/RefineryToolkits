using NUnit.Framework;
using Autodesk.RefineryToolkits.SpacePlanning.Generate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestServices;
using Autodesk.DesignScript.Geometry;
using DSCore;

namespace Autodesk.RefineryToolkits.SpacePlanning.Generate.Tests
{
    [TestFixture]
    public class SurfaceDivision2DTests : GeometricTestBase
    {
        private Surface surface;
        private List<double> uParams;
        private List<double> vParams;

        [SetUp]
        public void BeforeTest()
        {
            surface = Surface.ByPatch(PolyCurve.ByPoints(new List<Point>
            {
                Point.ByCoordinates(0, 0, 0),
                Point.ByCoordinates(27.129, 25.08, 0),
                Point.ByCoordinates(10.318, 36.351, 0),
                Point.ByCoordinates(-7.419, 16.573, 0)
            }, true));

            List<double> newRange = DSCore.Math.RemapRange(new List<double> { 0,1,2,3,4,5,6,7,8,9}, 0, 1) as List<double>;
            uParams = newRange;
            vParams = newRange;
        }

        /// <summary>
        /// Checks if the output of the Surface Divsion is the correct type
        /// </summary>
        [Test]
        public void DivideSurfaceReturnsListOfSurfacesTest()
        {
            var result = SurfaceDivision2D.DivideSurface(surface,uParams,vParams);

            Assert.IsTrue(result is List<Geometry>);
        }

        /// <summary>
        /// Check if output returns correct number of surfaces
        /// </summary>
        [Test]
        public void SurfacesCreatedIsCorrectTest()
        {
            var result = SurfaceDivision2D.DivideSurface(surface, uParams, vParams);
            Assert.IsTrue(result.Count == 56);
        }
    }
}