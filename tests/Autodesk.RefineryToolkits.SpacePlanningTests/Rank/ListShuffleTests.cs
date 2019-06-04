using NUnit.Framework;
using Autodesk.RefineryToolkits.SpacePlanning.Rank;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodesk.RefineryToolkits.SpacePlanning.Rank.Tests
{
    [TestFixture()]
    public class ListShuffleTests
    {
        private List<int> sampleList;

        [SetUp]
        public void BeforeTest()
        {
            sampleList = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        }

        /// <summary>
        /// Checks if the input lists values are suffled in a different way
        /// when seed changes
        /// </summary>
        [Test()]
        public void ShuffledListIsDifferentFromOriginalListTest()
        {
            var listShuffle1 = ListShuffle.SeededShuffle(sampleList);
            Assert.AreNotEqual(sampleList, listShuffle1);     
        }

        /// <summary>
        /// Checks if the same list shuffled with two different seeds are different from each other
        /// </summary>
        [Test]
        public void DifferentSeedReturnsDifferentListTest()
        {
            var listShuffle1 = ListShuffle.SeededShuffle(sampleList, 1);
            var listShuffle2 = ListShuffle.SeededShuffle(sampleList, 2);
            Assert.AreNotEqual(listShuffle1, listShuffle2);
        }

        /// <summary>
        /// Checks if the same list shuffled with the same seed returns the same
        /// </summary>
        [Test]
        public void SameSeedReturnsSameListTest()
        {
            var listShuffle1 = ListShuffle.SeededShuffle(sampleList, 3);
            var listShuffle2 = ListShuffle.SeededShuffle(sampleList, 3);
            Assert.AreEqual(listShuffle1, listShuffle2);
        }
    }
}