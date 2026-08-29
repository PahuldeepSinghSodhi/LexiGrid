using NUnit.Framework;
using LexiGrid.Core;

namespace LexiGrid.Tests
{
    /// <summary>
    /// NUnit tests for the ScoringEngine.
    /// </summary>
    public class ScoringEngineTests
    {
        [Test]
        public void BaseScore_ThreeLetters_Returns100()
        {
            Assert.AreEqual(100, ScoringEngine.CalculateBaseScore(3));
        }

        [Test]
        public void BaseScore_FourLetters_Returns400()
        {
            Assert.AreEqual(400, ScoringEngine.CalculateBaseScore(4));
        }

        [Test]
        public void BaseScore_FiveLetters_Returns800()
        {
            Assert.AreEqual(800, ScoringEngine.CalculateBaseScore(5));
        }

        [Test]
        public void BaseScore_SixLetters_Returns1200()
        {
            Assert.AreEqual(1200, ScoringEngine.CalculateBaseScore(6));
        }

        [Test]
        public void BaseScore_SevenPlusLetters_Returns2000()
        {
            Assert.AreEqual(2000, ScoringEngine.CalculateBaseScore(7));
            Assert.AreEqual(2000, ScoringEngine.CalculateBaseScore(10));
        }

        [Test]
        public void BaseScore_BelowMinimum_ReturnsZero()
        {
            Assert.AreEqual(0, ScoringEngine.CalculateBaseScore(2));
            Assert.AreEqual(0, ScoringEngine.CalculateBaseScore(1));
            Assert.AreEqual(0, ScoringEngine.CalculateBaseScore(0));
        }

        [Test]
        public void TimeMultiplier_Fast_Returns1_5()
        {
            Assert.AreEqual(1.5f, ScoringEngine.GetTimeMultiplier(3.0f));
        }

        [Test]
        public void TimeMultiplier_Medium_Returns1_25()
        {
            Assert.AreEqual(1.25f, ScoringEngine.GetTimeMultiplier(7.0f));
        }

        [Test]
        public void TimeMultiplier_Slow_Returns1_0()
        {
            Assert.AreEqual(1.0f, ScoringEngine.GetTimeMultiplier(15.0f));
        }

        [Test]
        public void TimeMultiplier_ExactThreshold_Fast_ReturnsNextTier()
        {
            Assert.AreEqual(1.25f, ScoringEngine.GetTimeMultiplier(5.0f));
        }

        [Test]
        public void FinalScore_ThreeLetters_Fast()
        {
            Assert.AreEqual(150, ScoringEngine.CalculateFinalScore(3, 2.0f));
        }

        [Test]
        public void FinalScore_FiveLetters_Slow()
        {
            Assert.AreEqual(800, ScoringEngine.CalculateFinalScore(5, 20.0f));
        }

        [Test]
        public void FinalScore_BelowMinimum_AlwaysZero()
        {
            Assert.AreEqual(0, ScoringEngine.CalculateFinalScore(2, 1.0f));
        }

        [Test]
        public void NegativeWordLength_ReturnsZero()
        {
            Assert.AreEqual(0, ScoringEngine.CalculateBaseScore(-1));
        }
    }
}
