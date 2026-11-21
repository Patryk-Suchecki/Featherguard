using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public sealed class ChargeRun : GestureRun
    {
        public const float WaitLimit = 2f;
        const float BarHalf = 320f;
        const float BarWidth = 54f;

        readonly TimingJudge judge;
        readonly float fillTime;
        readonly float zone;
        float? holdStart;

        public ChargeRun(TimingJudge judge, float fillTime, float zone)
        {
            this.judge = judge;
            this.fillTime = fillTime;
            this.zone = zone;
        }

        public float ReleaseTime => zone * fillTime;

        float Held => holdStart.HasValue ? Time - holdStart.Value : 0f;

        protected override void Step(in GestureSample input)
        {
            if (!holdStart.HasValue)
            {
                if (input.Pressed) holdStart = Time;
                else if (Time > WaitLimit) Finish(TimingResult.Miss);
                return;
            }
            if (input.Released || !input.Held) Finish(judge.Judge(Held - ReleaseTime));
            else if (Held > ReleaseTime + judge.GoodWindow) Finish(TimingResult.Miss);
        }

        public override void Draw(GestureSketch sketch)
        {
            var from = new Vector2(-BarHalf, 0f);
            var to = new Vector2(BarHalf, 0f);
            sketch.Bar(from, to, BarWidth, Held / fillTime, SketchTone.Faint, SketchTone.Cue);
            Zone(sketch, judge.GoodWindow, BarWidth + 24f, SketchTone.Target);
            Zone(sketch, judge.PerfectWindow, BarWidth + 40f, SketchTone.Hit);
            sketch.Label(new Vector2(0f, 110f), holdStart.HasValue ? "RELEASE IN THE GOLD" : "HOLD", 40, SketchTone.Cue);
        }

        void Zone(GestureSketch sketch, float window, float width, SketchTone tone)
        {
            float centre = Mathf.Lerp(-BarHalf, BarHalf, zone);
            float half = window / fillTime * BarHalf * 2f;
            sketch.Line(new Vector2(Mathf.Max(-BarHalf, centre - half), -width * 0.5f - 6f), new Vector2(Mathf.Min(BarHalf, centre + half), -width * 0.5f - 6f), 10f, tone);
        }
    }
}
