using System;
using System.Linq;
using Featherguard.Core;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    [Serializable]
    public abstract class TimingGesture
    {
        [Tooltip("Scales the perfect and good windows from the battle rules (below 1 is harder).")]
        [Min(0.1f)] public float windowScale = 1f;

        public GestureRun Start(TimingJudge judge) => Create(new TimingJudge(judge.PerfectWindow * windowScale, judge.GoodWindow * windowScale));

        protected abstract GestureRun Create(TimingJudge judge);
    }

    [Serializable]
    public sealed class RingGesture : TimingGesture
    {
        [Tooltip("Seconds until the ring meets the target circle.")]
        [Min(0.1f)] public float closeTime = 0.8f;

        protected override GestureRun Create(TimingJudge judge) => new RingRun(judge, closeTime);
    }

    [Serializable]
    public sealed class SeriesGesture : TimingGesture
    {
        [Min(1)] public int count = 3;
        [Tooltip("Seconds between the target moments of two rings.")]
        [Min(0.1f)] public float interval = 0.4f;
        [Tooltip("Seconds each ring takes to close.")]
        [Min(0.1f)] public float approach = 0.8f;

        protected override GestureRun Create(TimingJudge judge) => new SeriesRun(judge, count, interval, approach);
    }

    [Serializable]
    public sealed class SlashGesture : TimingGesture
    {
        [Tooltip("Direction of the cut in degrees: 0 is right, 90 is up.")]
        public float angle;
        [Tooltip("Seconds until the gate closes.")]
        [Min(0.1f)] public float closeTime = 0.9f;
        [Tooltip("Largest angle between the swipe and the arrow that still counts.")]
        [Range(5f, 90f)] public float tolerance = 40f;

        protected override GestureRun Create(TimingJudge judge) => new SlashRun(judge, angle, closeTime, tolerance);
    }

    [Serializable]
    public sealed class ChargeGesture : TimingGesture
    {
        [Tooltip("Seconds of holding to fill the bar.")]
        [Min(0.2f)] public float fillTime = 1.1f;
        [Tooltip("Where on the bar to release, from 0 to 1.")]
        [Range(0.3f, 0.95f)] public float zone = 0.8f;

        protected override GestureRun Create(TimingJudge judge) => new ChargeRun(judge, fillTime, zone);
    }

    [Serializable]
    public sealed class TraceGesture : TimingGesture
    {
        public GestureShape shape = GestureShape.Thorns;
        [Min(50f)] public float size = 300f;
        [Min(0.5f)] public float timeLimit = 2.5f;
        [Tooltip("How far from the line a finger may stray, in screen units of a 1080-high screen.")]
        [Min(10f)] public float tolerance = 55f;

        protected override GestureRun Create(TimingJudge judge) => new TraceRun(GestureShapes.Points(shape, size), timeLimit, tolerance);
    }

    [Serializable]
    public sealed class GuideGesture : TimingGesture
    {
        public GestureShape shape = GestureShape.Wave;
        [Min(50f)] public float size = 320f;
        [Tooltip("Seconds the light takes to travel the path.")]
        [Min(0.3f)] public float travelTime = 1.5f;
        [Min(10f)] public float tolerance = 70f;

        protected override GestureRun Create(TimingJudge judge) => new GuideRun(GestureShapes.Points(shape, size), travelTime, tolerance);
    }

    [Serializable]
    public sealed class SpinGesture : TimingGesture
    {
        [Min(1)] public int turns = 2;
        [Min(0.5f)] public float timeLimit = 2f;

        protected override GestureRun Create(TimingJudge judge) => new SpinRun(turns, timeLimit);
    }

    [Serializable]
    public sealed class SignGesture : TimingGesture
    {
        public GestureShape shape = GestureShape.Circle;
        [Min(50f)] public float size = 230f;
        [Tooltip("Seconds the sign is shown before it must be drawn from memory.")]
        [Min(0.2f)] public float showTime = 0.9f;
        [Min(0.5f)] public float drawTime = 2.5f;
        [Range(0f, 1f)] public float goodScore = 0.8f;
        [Range(0f, 1f)] public float perfectScore = 0.9f;

        protected override GestureRun Create(TimingJudge judge) =>
            new SignRun(GestureShapes.Points(shape, size), GestureShapes.IsClosed(shape), showTime, drawTime, goodScore, perfectScore);
    }

    [Serializable]
    public sealed class SequenceGesture : TimingGesture
    {
        [SerializeReference, SubclassSelector] public TimingGesture[] steps = new TimingGesture[0];

        protected override GestureRun Create(TimingJudge judge) =>
            new SequenceRun(steps.Where(s => s != null).Select(s => (Func<GestureRun>)(() => s.Start(judge))).ToList());
    }
}
