using NUnit.Framework;
using LexiGrid.Core;

public class ComboTrackerTests
{
    [Test] public void QuickPairAddsTwoSecondsAndConsumesPair()
    {
        var combo = new ComboTracker();
        Assert.AreEqual(0, combo.Accept(10, 30));
        Assert.AreEqual(2, combo.Accept(14.99f, 25));
        Assert.AreEqual(0, combo.Accept(15, 25));
        Assert.AreEqual(2, combo.Accept(16, 24));
    }
    [Test] public void ExactFiveSecondsDoesNotEarnBonus()
    {
        var combo = new ComboTracker(); combo.Accept(0, 30);
        Assert.AreEqual(0, combo.Accept(5, 25));
        Assert.AreEqual(2, combo.Accept(9, 21));
    }
    [Test] public void RejectionOrNewRoundResetBreaksPair()
    {
        var combo = new ComboTracker(); combo.Accept(0, 30); combo.Reset();
        Assert.AreEqual(0, combo.Accept(1, 29));
    }
    [TestCase(0)] [TestCase(-1)] public void ExpiredRoundCannotEarnTime(float remaining)
    {
        var combo = new ComboTracker(); combo.Accept(0, 1);
        Assert.AreEqual(0, combo.Accept(.5f, remaining));
        Assert.AreEqual(0, combo.Accept(1, 30));
    }
}
