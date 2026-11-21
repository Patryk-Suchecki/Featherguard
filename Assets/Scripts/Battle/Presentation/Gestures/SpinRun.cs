using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public sealed class SpinRun : GestureRun
    {
        public const float Radius = 200f;
        public const float MinRadius = 60f;
        public const float PerfectShare = 0.65f;
        const int ArcSegments = 48;

        readonly int turns;
        readonly float timeLimit;
        Vector2? previous;
        float total;

        public SpinRun(int turns, float timeLimit)
        {
            this.turns = turns;
            this.timeLimit = timeLimit;
        }

        float Goal => turns * 360f;

        public float Turned => Mathf.Abs(total) / 360f;

        protected override void Step(in GestureSample input)
        {
            if (input.Held && !input.FromKey && input.Position.magnitude >= MinRadius)
            {
                if (previous.HasValue && !input.Pressed)
                    total += Vector2.SignedAngle(previous.Value, input.Position);
                previous = input.Position;
            }
            else
            {
                previous = null;
            }

            if (Mathf.Abs(total) >= Goal) Finish(Time <= timeLimit * PerfectShare ? TimingResult.Perfect : TimingResult.Good);
            else if (Time > timeLimit) Finish(TimingResult.Miss);
        }

        public override void Draw(GestureSketch sketch)
        {
            sketch.Circle(Vector2.zero, Radius, SketchTone.Guide);
            sketch.Circle(Vector2.zero, MinRadius * 0.5f, SketchTone.Target, true);
            float fraction = Mathf.Clamp01(Mathf.Abs(total) / Goal);
            int segments = Mathf.CeilToInt(ArcSegments * fraction);
            for (int i = 0; i < segments; i++)
                sketch.Line(OnCircle(i / (float)ArcSegments), OnCircle(Mathf.Min(fraction, (i + 1) / (float)ArcSegments)), 22f, SketchTone.Hit);
            sketch.Label(Vector2.zero, $"{Mathf.Min(turns, Mathf.FloorToInt(Turned))}/{turns}", 40, SketchTone.Cue);
            sketch.Bar(new Vector2(-300f, -270f), new Vector2(300f, -270f), 16f, 1f - Time / timeLimit, SketchTone.Faint, SketchTone.Target);
        }

        static Vector2 OnCircle(float fraction)
        {
            float angle = Mathf.PI * 0.5f - fraction * Mathf.PI * 2f;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (Radius + 26f);
        }
    }
}
