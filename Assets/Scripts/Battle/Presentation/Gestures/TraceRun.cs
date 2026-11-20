using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public sealed class TraceRun : GestureRun
    {
        public const float StartReach = 80f;
        const float MinStep = 6f;
        const int CoverageSamples = 48;
        const float PerfectCoverage = 0.95f;
        const float GoodCoverage = 0.75f;
        const float PerfectError = 0.45f;
        const float GoodError = 0.8f;

        readonly List<Vector2> path;
        readonly float timeLimit;
        readonly float tolerance;
        readonly List<Vector2> stroke = new();
        bool drawing;

        public TraceRun(List<Vector2> path, float timeLimit, float tolerance)
        {
            this.path = path;
            this.timeLimit = timeLimit;
            this.tolerance = tolerance;
        }

        public IReadOnlyList<Vector2> Path => path;

        protected override void Step(in GestureSample input)
        {
            if (!drawing)
            {
                if (input.Pressed && !input.FromKey && Vector2.Distance(input.Position, path[0]) <= StartReach)
                {
                    drawing = true;
                    stroke.Add(input.Position);
                }
                else if (Time > timeLimit) Finish(TimingResult.Miss);
                return;
            }

            if (input.Held && Vector2.Distance(stroke[stroke.Count - 1], input.Position) >= MinStep)
                stroke.Add(input.Position);
            if (!input.Held || Time > timeLimit)
                Finish(Evaluate(path, stroke, tolerance));
        }

        public static TimingResult Evaluate(IReadOnlyList<Vector2> path, IReadOnlyList<Vector2> stroke, float tolerance)
        {
            if (stroke.Count < 2) return TimingResult.Miss;
            var samples = StrokeMatcher.Resample(path, CoverageSamples);
            float coverage = samples.Count(s => StrokeMatcher.DistanceToPath(s, stroke) <= tolerance) / (float)samples.Count;
            float error = stroke.Average(p => StrokeMatcher.DistanceToPath(p, path));
            if (coverage >= PerfectCoverage && error <= tolerance * PerfectError) return TimingResult.Perfect;
            return coverage >= GoodCoverage && error <= tolerance * GoodError ? TimingResult.Good : TimingResult.Miss;
        }

        public override void Draw(GestureSketch sketch)
        {
            sketch.Path(path, tolerance * 0.9f, SketchTone.Faint);
            sketch.Path(path, 12f, SketchTone.Guide);
            sketch.Circle(path[0], 30f, SketchTone.Target, true);
            sketch.Circle(path[path.Count - 1], 30f, SketchTone.Target);
            sketch.Path(stroke, 14f, SketchTone.Cue);
            sketch.Bar(new Vector2(-300f, -270f), new Vector2(300f, -270f), 16f, 1f - Time / timeLimit, SketchTone.Faint, SketchTone.Target);
        }
    }
}
