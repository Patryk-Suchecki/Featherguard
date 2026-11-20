using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public sealed class SlashRun : GestureRun
    {
        public const float ArrowLength = 300f;
        public const float GateHalfLength = 230f;
        public const float MinRunUp = 50f;

        readonly TimingJudge judge;
        readonly float closeTime;
        readonly float tolerance;
        readonly Vector2 direction;
        readonly Vector2 across;
        bool stroking;
        Vector2 strokeStart;
        Vector2 last;

        public SlashRun(TimingJudge judge, float angle, float closeTime, float tolerance)
        {
            this.judge = judge;
            this.closeTime = closeTime;
            this.tolerance = tolerance;
            direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            across = new Vector2(-direction.y, direction.x);
        }

        public Vector2 Direction => direction;

        protected override void Step(in GestureSample input)
        {
            if (input.Pressed && !input.FromKey)
            {
                stroking = true;
                strokeStart = input.Position;
                last = input.Position;
            }
            else if (stroking && input.Held)
            {
                var position = input.Position;
                float before = Vector2.Dot(last, direction);
                float now = Vector2.Dot(position, direction);
                if (before < 0f && now >= 0f && Mathf.Abs(Vector2.Dot(position, across)) <= GateHalfLength && Vector2.Dot(strokeStart, direction) <= -MinRunUp)
                {
                    float error = Vector2.Angle(position - strokeStart, direction);
                    Finish(error <= tolerance ? judge.Judge(Time - closeTime) : TimingResult.Miss);
                    return;
                }
                last = position;
            }
            else
            {
                stroking = false;
            }

            if (Time > closeTime + judge.GoodWindow) Finish(TimingResult.Miss);
        }

        public override void Draw(GestureSketch sketch)
        {
            sketch.Arrow(-direction * ArrowLength, direction * ArrowLength, 18f, SketchTone.Guide);
            sketch.Line(-across * GateHalfLength, across * GateHalfLength, 12f, SketchTone.Target);
            sketch.Circle(Vector2.zero, RingRun.Radius * (1f + RingRun.Growth * Mathf.Clamp01(1f - Time / closeTime)), SketchTone.Cue);
            if (stroking) sketch.Line(strokeStart, last, 14f, SketchTone.Cue);
        }
    }
}
