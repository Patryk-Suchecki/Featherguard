using System.Collections.Generic;
using System.Linq;
using Featherguard.Battle;
using Featherguard.Battle.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace Featherguard.Tests
{
    public class GestureTests
    {
        const float Step = 0.01f;
        static readonly TimingJudge Judge = new(0.06f, 0.15f);

        static void Wait(GestureRun run, float seconds, GestureSample sample)
        {
            for (float t = 0f; t < seconds - Step * 0.5f && !run.Finished; t += Step)
                run.Tick(Step, sample);
        }

        static void Idle(GestureRun run, float seconds) => Wait(run, seconds, GestureSample.Idle(Vector2.zero));

        static void Drag(GestureRun run, IReadOnlyList<Vector2> points, float seconds)
        {
            run.Tick(Step, GestureSample.Press(points[0]));
            int steps = Mathf.Max(1, Mathf.RoundToInt(seconds / Step));
            for (int i = 1; i <= steps && !run.Finished; i++)
                run.Tick(Step, GestureSample.Hold(StrokeMatcher.PointAlong(points, i / (float)steps)));
            if (!run.Finished) run.Tick(Step, GestureSample.Release(points[points.Count - 1]));
        }

        [TestCase(0.8f, TimingResult.Perfect)]
        [TestCase(0.9f, TimingResult.Good)]
        [TestCase(0.5f, TimingResult.Miss)]
        public void Ring_JudgesThePressAgainstTheCloseTime(float pressAt, TimingResult expected)
        {
            var run = new RingGesture { closeTime = 0.8f }.Start(Judge);
            Idle(run, pressAt - Step);
            run.Tick(Step, GestureSample.Key());

            Assert.AreEqual(expected, run.Result);
        }

        [Test]
        public void Ring_WithoutAPressIsAMiss()
        {
            var run = new RingGesture { closeTime = 0.8f }.Start(Judge);
            Idle(run, 2f);

            Assert.IsTrue(run.Finished);
            Assert.AreEqual(TimingResult.Miss, run.Result);
        }

        [Test]
        public void WindowScale_NarrowsTheWindows()
        {
            var run = new RingGesture { closeTime = 0.8f, windowScale = 0.5f }.Start(Judge);
            Idle(run, 0.9f - Step);
            run.Tick(Step, GestureSample.Key());

            Assert.AreEqual(TimingResult.Miss, run.Result);
        }

        [TestCase(0, TimingResult.Perfect)]
        [TestCase(1, TimingResult.Good)]
        [TestCase(2, TimingResult.Miss)]
        public void Series_JudgesEveryRingAndAggregates(int skipped, TimingResult expected)
        {
            var run = (SeriesRun)new SeriesGesture { count = 3, interval = 0.4f, approach = 0.8f }.Start(Judge);
            for (int i = 0; i < 3; i++)
            {
                float target = run.TargetTime(i);
                Idle(run, target - Step - Elapsed(run, i));
                if (i < skipped) continue;
                run.Tick(Step, GestureSample.Press(run.Positions[i]));
            }
            Idle(run, 1f);

            Assert.AreEqual(expected, run.Result);
        }

        static float Elapsed(SeriesRun run, int index) => index == 0 ? 0f : run.TargetTime(index - 1);

        [Test]
        public void Series_IgnoresTapsFarFromTheRing()
        {
            var run = (SeriesRun)new SeriesGesture { count = 1, approach = 0.8f }.Start(Judge);
            Idle(run, 0.8f - Step);
            run.Tick(Step, GestureSample.Press(new Vector2(400f, 300f)));
            Idle(run, 1f);

            Assert.AreEqual(TimingResult.Miss, run.Result);
        }

        [Test]
        public void Slash_CrossingTheGateInTheArrowDirectionOnTimeIsPerfect()
        {
            var run = new SlashGesture { angle = 0f, closeTime = 0.9f, tolerance = 40f }.Start(Judge);
            Idle(run, 0.7f);
            Drag(run, new[] { new Vector2(-200f, 0f), new Vector2(200f, 0f) }, 0.4f);

            Assert.AreEqual(TimingResult.Perfect, run.Result);
        }

        [Test]
        public void Slash_InTheWrongDirectionIsAMiss()
        {
            var run = new SlashGesture { angle = 0f, closeTime = 0.9f, tolerance = 40f }.Start(Judge);
            Idle(run, 0.7f);
            Drag(run, new[] { new Vector2(-100f, -200f), new Vector2(50f, 200f) }, 0.4f);

            Assert.AreEqual(TimingResult.Miss, run.Result);
        }

        [Test]
        public void Charge_ReleasedInTheZoneIsPerfectAndOverchargedIsAMiss()
        {
            var gesture = new ChargeGesture { fillTime = 1f, zone = 0.8f };
            var released = gesture.Start(Judge);
            released.Tick(Step, GestureSample.Press(Vector2.zero));
            Wait(released, 0.79f, GestureSample.Hold(Vector2.zero));
            released.Tick(Step, GestureSample.Release(Vector2.zero));

            var held = gesture.Start(Judge);
            held.Tick(Step, GestureSample.Press(Vector2.zero));
            Wait(held, 2f, GestureSample.Hold(Vector2.zero));

            Assert.AreEqual(TimingResult.Perfect, released.Result);
            Assert.AreEqual(TimingResult.Miss, held.Result);
        }

        [Test]
        public void Trace_FollowingTheLineIsPerfectAndStrayingIsAMiss()
        {
            var path = GestureShapes.Points(GestureShape.Thorns, 300f);
            var close = new TraceRun(path, 3f, 55f);
            Drag(close, path, 1f);
            var off = new TraceRun(path, 3f, 55f);
            Drag(off, path.Select((p, i) => p + new Vector2(0f, i % 2 == 0 ? 0f : 160f)).ToList(), 1f);

            Assert.AreEqual(TimingResult.Perfect, close.Result);
            Assert.AreEqual(TimingResult.Miss, off.Result);
        }

        [Test]
        public void Trace_MustStartAtTheStartDot()
        {
            var path = GestureShapes.Points(GestureShape.Thorns, 300f);
            var run = new TraceRun(path, 1f, 55f);
            Drag(run, Enumerable.Reverse(path).ToList(), 0.5f);
            Idle(run, 2f);

            Assert.AreEqual(TimingResult.Miss, run.Result);
        }

        [Test]
        public void Guide_FollowingTheLightIsPerfectAndLettingGoIsAMiss()
        {
            var gesture = new GuideGesture { shape = GestureShape.Wave, size = 320f, travelTime = 1f, tolerance = 70f };
            var followed = (GuideRun)gesture.Start(Judge);
            followed.Tick(Step, GestureSample.Press(followed.Path[0]));
            for (int i = 0; i < 200 && !followed.Finished; i++)
                followed.Tick(Step, GestureSample.Hold(followed.Ball));

            var dropped = (GuideRun)gesture.Start(Judge);
            dropped.Tick(Step, GestureSample.Press(dropped.Path[0]));
            Wait(dropped, 0.3f, GestureSample.Hold(dropped.Path[0]));
            dropped.Tick(Step, GestureSample.Release(dropped.Path[0]));

            Assert.AreEqual(TimingResult.Perfect, followed.Result);
            Assert.AreEqual(TimingResult.Miss, dropped.Result);
        }

        [TestCase(1f, TimingResult.Perfect)]
        [TestCase(1.8f, TimingResult.Good)]
        public void Spin_TurnsFastEnoughArePerfect(float seconds, TimingResult expected)
        {
            var run = new SpinGesture { turns = 2, timeLimit = 2f }.Start(Judge);
            int steps = Mathf.RoundToInt(seconds / Step);
            run.Tick(Step, GestureSample.Press(new Vector2(150f, 0f)));
            for (int i = 1; i <= steps + 5 && !run.Finished; i++)
            {
                float angle = i / (float)steps * Mathf.PI * 4f;
                run.Tick(Step, GestureSample.Hold(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 150f));
            }

            Assert.AreEqual(expected, run.Result);
        }

        [Test]
        public void Sign_RecognisesTheShownShape()
        {
            var circle = (SignRun)new SignGesture { shape = GestureShape.Circle }.Start(Judge);
            var drawnCircle = Enumerable.Range(0, 40).Select(i => new Vector2(Mathf.Cos(i / 39f * Mathf.PI * 2f + 1f), Mathf.Sin(i / 39f * Mathf.PI * 2f + 1f)) * 180f).ToList();
            var line = Enumerable.Range(0, 40).Select(i => new Vector2(i * 8f, i * 2f)).ToList();

            Assert.AreEqual(TimingResult.Perfect, circle.Judge(drawnCircle));
            Assert.AreEqual(TimingResult.Miss, circle.Judge(line));
        }

        [Test]
        public void Sign_LightningIsNotACircle()
        {
            var lightning = (SignRun)new SignGesture { shape = GestureShape.Lightning, size = 260f }.Start(Judge);
            var drawnCircle = Enumerable.Range(0, 40).Select(i => new Vector2(Mathf.Cos(i / 39f * Mathf.PI * 2f), Mathf.Sin(i / 39f * Mathf.PI * 2f)) * 180f).ToList();
            var drawnLightning = StrokeMatcher.Resample(GestureShapes.Points(GestureShape.Lightning, 200f), 40).Select(p => p + new Vector2(30f, -10f)).ToList();

            Assert.AreEqual(TimingResult.Miss, lightning.Judge(drawnCircle));
            Assert.AreEqual(TimingResult.Perfect, lightning.Judge(drawnLightning));
        }

        [Test]
        public void Sign_IsDrawnAfterTheShowTime()
        {
            var run = new SignGesture { shape = GestureShape.Circle, size = 200f, showTime = 0.5f, drawTime = 2f }.Start(Judge);
            Idle(run, 0.6f);
            var circle = GestureShapes.Points(GestureShape.Circle, 200f);
            Drag(run, circle, 1f);

            Assert.AreEqual(TimingResult.Perfect, run.Result);
        }

        [Test]
        public void Sequence_AggregatesItsSteps()
        {
            var run = new SequenceGesture
            {
                steps = new TimingGesture[] { new RingGesture { closeTime = 0.5f }, new RingGesture { closeTime = 0.5f } },
            }.Start(Judge);
            Idle(run, 0.5f - Step);
            run.Tick(Step, GestureSample.Key());
            Idle(run, SequenceRun.Pause + 0.5f - Step);
            run.Tick(Step, GestureSample.Key());

            Assert.IsTrue(run.Finished);
            Assert.AreEqual(TimingResult.Perfect, run.Result);
        }

        [Test]
        public void Aggregate_NeedsEveryPartForPerfectAndMostPartsForGood()
        {
            Assert.AreEqual(TimingResult.Perfect, GestureRun.Aggregate(new[] { TimingResult.Perfect, TimingResult.Perfect }));
            Assert.AreEqual(TimingResult.Good, GestureRun.Aggregate(new[] { TimingResult.Perfect, TimingResult.Good, TimingResult.Miss }));
            Assert.AreEqual(TimingResult.Miss, GestureRun.Aggregate(new[] { TimingResult.Perfect, TimingResult.Miss, TimingResult.Miss }));
            Assert.AreEqual(TimingResult.Miss, GestureRun.Aggregate(new TimingResult[0]));
        }
    }
}
