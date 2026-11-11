using Featherguard.Battle;
using NUnit.Framework;

namespace Featherguard.Tests
{
    public class TimingJudgeTests
    {
        readonly TimingJudge judge = new(0.06f, 0.15f);

        [TestCase(0f, TimingResult.Perfect)]
        [TestCase(-0.05f, TimingResult.Perfect)]
        [TestCase(0.05f, TimingResult.Perfect)]
        [TestCase(-0.1f, TimingResult.Good)]
        [TestCase(0.14f, TimingResult.Good)]
        [TestCase(-0.2f, TimingResult.Miss)]
        [TestCase(0.16f, TimingResult.Miss)]
        public void JudgesTheOffsetFromTheTargetMoment(float offset, TimingResult expected) =>
            Assert.AreEqual(expected, judge.Judge(offset));

        [Test]
        public void ReadsTheWindowsFromTheRules()
        {
            var rules = UnityEngine.ScriptableObject.CreateInstance<BattleRules>();
            rules.perfectWindow = 0.02f;
            rules.goodWindow = 0.1f;
            var fromRules = new TimingJudge(rules);

            Assert.AreEqual(TimingResult.Good, fromRules.Judge(0.05f));
            Assert.AreEqual(0.1f, fromRules.GoodWindow);
        }

        [Test]
        public void GoodWindowIsNeverNarrowerThanPerfect()
        {
            var narrow = new TimingJudge(0.1f, 0.05f);
            Assert.AreEqual(0.1f, narrow.GoodWindow);
            Assert.AreEqual(TimingResult.Perfect, narrow.Judge(0.08f));
        }
    }
}
