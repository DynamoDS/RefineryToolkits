using NUnit.Framework;
using Autodesk.GenerativeToolkit.Rank;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodesk.GenerativeToolkit.Rank.Tests
{
    [TestFixture()]
    public class ListShuffleTests
    {
        // Checks if the input lists values are suffled in a different way
        // when seed changes
        [Test()]
        public void SeededShuffleTest()
        {
            List<int> sampleList = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var seedOne = ListShuffle.SeededShuffle(sampleList);
            Assert.AreNotEqual(sampleList, seedOne);

            var seedTwo = ListShuffle.SeededShuffle(sampleList, 2);
            Assert.AreNotEqual(seedOne, seedTwo);

        }
    }
}