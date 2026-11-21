using System;
using System.Collections.Generic;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public sealed class SequenceRun : GestureRun
    {
        public const float Pause = 0.25f;

        readonly IReadOnlyList<Func<GestureRun>> steps;
        readonly List<TimingResult> results = new();
        GestureRun current;
        float pauseUntil;
        float lastStep;

        public SequenceRun(IReadOnlyList<Func<GestureRun>> steps)
        {
            this.steps = steps;
            current = steps.Count > 0 ? steps[0]() : null;
        }

        public IReadOnlyList<TimingResult> Parts => results;

        protected override void Step(in GestureSample input)
        {
            if (current == null)
            {
                Finish(Aggregate(results));
                return;
            }
            float elapsed = Time - lastStep;
            lastStep = Time;
            if (Time < pauseUntil) return;

            current.Tick(elapsed, input);
            if (!current.Finished) return;

            results.Add(current.Result);
            Cue(current.Result);
            if (results.Count >= steps.Count)
            {
                current = null;
                Finish(Aggregate(results));
                return;
            }
            current = steps[results.Count]();
            pauseUntil = Time + Pause;
        }

        public override void Draw(GestureSketch sketch)
        {
            if (current == null || Time < pauseUntil) return;
            current.Draw(sketch);
            sketch.Label(new Vector2(0f, -330f), $"{results.Count + 1}/{steps.Count}", 36, SketchTone.Cue);
        }
    }
}
