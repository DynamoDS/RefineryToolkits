using NUnit.Framework;
using Autodesk.RefineryToolkits.SpacePlanning.Explore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSCore;

namespace Autodesk.RefineryToolkits.SpacePlanning.Explore.Tests
{
    [TestFixture]
    public class ColorsTests
    {
        /// <summary>
        /// Test if maximum colors can be returned
        /// </summary>
        [Test]
        public void ContrastyColorRangeMaxNumberOfColors()
        {
            List<Color> colors = Colors.ContrastyColorRange(19);
            Assert.AreEqual(19, colors.Count);
        }

        /// <summary>
        /// Check if different seeds returns different outputs
        /// </summary>
        [Test]
        public void DifferentSeedsReturnsDifferentOutputsTest()
        {
            List<Color> colors = Colors.ContrastyColorRange(19);
            List<Color> colorsSeed = Colors.ContrastyColorRange(19, 0, 2);
            Assert.AreNotEqual(colors, colorsSeed);
        }

        /// <summary>
        /// Check if exception is thrown if amount is set over 19.
        /// </summary>
        [Test]
        public void ExceptionOverMaxColors()
        {
            var ex = Assert.Throws<ArgumentException>(() => Colors.ContrastyColorRange(20));
            Assert.That(ex.Message, Is.EqualTo("Maximum number of colours supported right now is 19"));
        }
    }
}