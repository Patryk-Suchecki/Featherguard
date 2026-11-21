using System.Collections.Generic;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public sealed class SeriesRun : GestureRun
    {
        const float Radius = 70f;
        const float TapReach = 1.6f;
        const float ResultShown = 0.3f;

        readonly TimingJudge judge;
        readonly float interval;
        readonly float approach;
        readonly List<Vector2> positions = new();
        readonly List<TimingResult> results = new();
        readonly List<float> judgedAt = new();

        public SeriesRun(TimingJudge judge, int count, float interval, float approach)
        {
            this.judge = judge;
            this.interval = interval;
            this.approach = approach;
            for (int i = 0; i < count; i++)
                positions.Add(Layout(i, count));
        }

        public IReadOnlyList<Vector2> Positions => positions;

        public float TargetTime(int index) => approach + index * interval;

        protected override void Step(in GestureSample input)
        {
            int current = results.Count;
            float target = TargetTime(current);
            if (input.Pressed && Time >= target - approach && (input.FromKey || Vector2.Distance(input.Position, positions[current]) <= Radius * TapReach))
                Judge(judge.Judge(Time - target));
            else if (Time > target + judge.GoodWindow)
                Judge(TimingResult.Miss);

            if (results.Count == positions.Count)
                Finish(Aggregate(results));
        }

        void Judge(TimingResult result)
        {
            results.Add(result);
            judgedAt.Add(Time);
            Cue(result);
        }

        public override void Draw(GestureSketch sketch)
        {
            for (int i = 0; i < results.Count; i++)
                if (Time - judgedAt[i] < ResultShown)
                    sketch.Circle(positions[i], Radius, ToneOf(results[i]), true);

            for (int i = positions.Count - 1; i >= results.Count; i--)
            {
                float target = TargetTime(i);
                if (Time < target - approach) continue;
                sketch.Circle(positions[i], Radius, SketchTone.Target);
                sketch.Label(positions[i], (i + 1).ToString(), 44, SketchTone.Target);
                sketch.Circle(positions[i], Radius * (1f + RingRun.Growth * Mathf.Clamp01((target - Time) / approach)), i == results.Count ? SketchTone.Cue : SketchTone.Faint);
            }
        }

        static Vector2 Layout(int index, int count)
        {
            if (count == 1) return Vector2.zero;
            float x = Mathf.Lerp(-320f, 320f, index / (count - 1f));
            return new Vector2(x, index % 2 == 0 ? 70f : -90f);
        }
    }
}
