using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodesk.GenerativeToolkit.Generate.Tests
{
    [TestFixture]
    public class BinPacking2DTests
    {
        [Test]
        public void PackTest()
        {
            Assert.Fail();
        }

        [Test]
        public void PlacementPropertyBLSFTest()
        {
            string blsf = BinPacking2D.BLSF;
            Assert.True(blsf == "BLSF");
        }

        [Test]
        public void PlacementPropertyBSSFTest()
        {
            string bssf = BinPacking2D.BSSF;
            Assert.True(bssf == "BSSF");
        }

        [Test]
        public void PlacementPropertyBAFTest()
        {
            string baf = BinPacking2D.BAF;
            Assert.True(baf == "BAF");
        }
    }
}