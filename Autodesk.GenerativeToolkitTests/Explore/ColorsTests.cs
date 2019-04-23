using NUnit.Framework;
using Autodesk.GenerativeToolkit.Explore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSCore;

namespace Autodesk.GenerativeToolkit.Explore.Tests
{
    [TestFixture]
    public class ColorsTests
    {
        [Test]
        public void ContrastyColorRangeMaxNumberOfColors()
        {
            List<Color> colors = Colors.ContrastyColorRange(19);
            Assert.AreEqual(19, colors.Count);
            List<Color> colorsSeed = Colors.ContrastyColorRange(19,0,2);
            Assert.AreNotEqual(colors, colorsSeed);
        }

        [Test]
        public void ExceptionOverMaxColors()
        {
            var ex = Assert.Throws<ArgumentException>(() => Colors.ContrastyColorRange(20));
            Assert.That(ex.Message, Is.EqualTo("Maximum number of colours supported right now is 19"));
        }
    }
}