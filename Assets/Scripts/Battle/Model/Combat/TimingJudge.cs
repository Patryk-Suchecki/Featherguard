using System;

namespace Featherguard.Battle
{
    public sealed class TimingJudge
    {
        public TimingJudge(float perfectWindow, float goodWindow)
        {
            PerfectWindow = perfectWindow;
            GoodWindow = Math.Max(perfectWindow, goodWindow);
        }

        public TimingJudge(BattleRules rules) : this(rules.perfectWindow, rules.goodWindow)
        {
        }

        public float PerfectWindow { get; }
        public float GoodWindow { get; }

        public TimingResult Judge(float offset)
        {
            float distance = Math.Abs(offset);
            if (distance <= PerfectWindow) return TimingResult.Perfect;
            return distance <= GoodWindow ? TimingResult.Good : TimingResult.Miss;
        }
    }
}
