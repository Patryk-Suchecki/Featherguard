using System.Collections.Generic;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public sealed class SignRun : GestureRun
    {
        public const float MinSize = 90f;
        const float MinStep = 6f;

        readonly List<Vector2> sign;
        readonly bool closed;
        readonly float showTime;
        readonly float drawTime;
        readonly float goodScore;
        readonly float perfectScore;
        readonly List<Vector2> stroke = new();
        bool drawing;

        public SignRun(List<Vector2> sign, bool closed, float showTime, float drawTime, float goodScore, float perfectScore)
        {
            this.sign = sign;
            this.closed = closed;
            this.showTime = showTime;
            this.drawTime = drawTime;
            this.goodScore = goodScore;
            this.perfectScore = perfectScore;
        }

        public float LastScore { get; private set; }

        bool Showing => Time < showTime;

        protected override void Step(in GestureSample input)
        {
            if (Showing) return;
            if (!drawing)
            {
                if (input.Pressed && !input.FromKey)
                {
                    drawing = true;
                    stroke.Add(input.Position);
                }
                else if (Time > showTime + drawTime) Finish(TimingResult.Miss);
                return;
            }

            if (input.Held && Vector2.Distance(stroke[stroke.Count - 1], input.Position) >= MinStep)
                stroke.Add(input.Position);
            if (!input.Held || Time > showTime + drawTime)
                Finish(Judge(stroke));
        }

        public TimingResult Judge(IReadOnlyList<Vector2> drawn)
        {
            if (drawn.Count < 8 || Extent(drawn) < MinSize) return TimingResult.Miss;
            LastScore = StrokeMatcher.Score(drawn, sign, closed);
            if (LastScore >= perfectScore) return TimingResult.Perfect;
            return LastScore >= goodScore ? TimingResult.Good : TimingResult.Miss;
        }

        public override void Draw(GestureSketch sketch)
        {
            if (Showing)
            {
                sketch.Path(sign, 22f, SketchTone.Target);
                sketch.Label(new Vector2(0f, 290f), "REMEMBER THE SIGN", 40, SketchTone.Cue);
                return;
            }
            sketch.Path(stroke, 16f, SketchTone.Cue);
            if (!drawing) sketch.Label(new Vector2(0f, 290f), "DRAW IT", 40, SketchTone.Cue);
            sketch.Bar(new Vector2(-300f, -290f), new Vector2(300f, -290f), 16f, 1f - (Time - showTime) / drawTime, SketchTone.Faint, SketchTone.Target);
        }

        static float Extent(IReadOnlyList<Vector2> points)
        {
            var min = points[0];
            var max = points[0];
            foreach (var p in points)
            {
                min = Vector2.Min(min, p);
                max = Vector2.Max(max, p);
            }
            return Mathf.Max(max.x - min.x, max.y - min.y);
        }
    }
}
