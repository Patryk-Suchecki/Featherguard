using System;

namespace Featherguard.Battle
{
    public sealed class TimingJudge
    {
        readonly float perfectWindow;

        public TimingJudge(float perfectWindow, float goodWindow)
        {
            this.perfectWindow = perfectWindow;
            GoodWindow = Math.Max(perfectWindow, goodWindow);
        }

        public float GoodWindow { get; }

        public TimingResult Judge(float offset)
        {
            float distance = Math.Abs(offset);
            if (distance <= perfectWindow) return TimingResult.Perfect;
            return distance <= GoodWindow ? TimingResult.Good : TimingResult.Miss;
        }
    }
}
