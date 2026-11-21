using System.Collections.Generic;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public sealed class GuideRun : GestureRun
    {
        public const float WaitLimit = 2f;
        const float BallRadius = 34f;
        const float PerfectShare = 0.94f;
        const float GoodShare = 0.7f;

        readonly List<Vector2> path;
        readonly float travelTime;
        readonly float tolerance;
        float? startedAt;
        float lastStep;
        float onPath;
        Vector2 pointer;

        public GuideRun(List<Vector2> path, float travelTime, float tolerance)
        {
            this.path = path;
            this.travelTime = travelTime;
            this.tolerance = tolerance;
        }

        public IReadOnlyList<Vector2> Path => path;

        float Progress => startedAt.HasValue ? Mathf.Clamp01((Time - startedAt.Value) / travelTime) : 0f;

        public Vector2 Ball => StrokeMatcher.PointAlong(path, Progress);

        protected override void Step(in GestureSample input)
        {
            pointer = input.Position;
            if (!startedAt.HasValue)
            {
                if (input.Pressed && !input.FromKey && Vector2.Distance(input.Position, path[0]) <= tolerance)
                {
                    startedAt = Time;
                    lastStep = Time;
                }
                else if (Time > WaitLimit) Finish(TimingResult.Miss);
                return;
            }

            float elapsed = Mathf.Min(Time, startedAt.Value + travelTime) - lastStep;
            lastStep = Time;
            if (input.Held && Vector2.Distance(input.Position, Ball) <= tolerance)
                onPath += Mathf.Max(0f, elapsed);

            if (!input.Held || Progress >= 1f)
            {
                float share = onPath / travelTime;
                Finish(share >= PerfectShare ? TimingResult.Perfect : share >= GoodShare ? TimingResult.Good : TimingResult.Miss);
            }
        }

        public override void Draw(GestureSketch sketch)
        {
            sketch.Path(path, 14f, SketchTone.Guide);
            var ball = Ball;
            sketch.Circle(ball, tolerance, SketchTone.Faint);
            sketch.Circle(ball, BallRadius, SketchTone.Target, true);
            if (startedAt.HasValue) sketch.Line(ball, pointer, 8f, Vector2.Distance(ball, pointer) <= tolerance ? SketchTone.Hit : SketchTone.Miss);
            else sketch.Label(path[0] + new Vector2(0f, 90f), "HOLD THE LIGHT", 36, SketchTone.Cue);
        }
    }
}
