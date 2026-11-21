using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public sealed class RingRun : GestureRun
    {
        public const float Radius = 85f;
        public const float Growth = 2.2f;

        readonly TimingJudge judge;
        readonly float closeTime;

        public RingRun(TimingJudge judge, float closeTime)
        {
            this.judge = judge;
            this.closeTime = closeTime;
        }

        public override bool AtTarget => true;

        protected override void Step(in GestureSample input)
        {
            if (input.Pressed) Finish(judge.Judge(Time - closeTime));
            else if (Time > closeTime + judge.GoodWindow) Finish(TimingResult.Miss);
        }

        public override void Draw(GestureSketch sketch)
        {
            sketch.Circle(Vector2.zero, Radius, SketchTone.Target);
            sketch.Circle(Vector2.zero, Radius * (1f + Growth * Mathf.Clamp01(1f - Time / closeTime)), SketchTone.Cue);
        }
    }
}
