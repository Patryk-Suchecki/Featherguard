using System;
using System.Collections;
using Featherguard.Core;
using Featherguard.Feedback;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public sealed class TimingPrompt
    {
        readonly BattlePresentationConfig config;
        readonly ITimingCueView cues;
        readonly IPopupView view;
        readonly ISoundPlayer sounds;
        readonly ITimingInput input;
        readonly IRandomSource random;
        readonly TimingJudge judge;

        public TimingPrompt(BattlePresentationConfig config, TimingJudge judge, ITimingCueView cues, IPopupView view, ISoundPlayer sounds, ITimingInput input, IRandomSource random)
        {
            this.config = config;
            this.judge = judge;
            this.cues = cues;
            this.view = view;
            this.sounds = sounds;
            this.input = input;
            this.random = random;
        }

        public IEnumerator Perform(TimingGesture gesture, Vector3 world, Action<TimingResult> done)
        {
            var run = (gesture ?? new RingGesture { closeTime = config.attackRingTime }).Start(judge);
            var sketch = new GestureSketch();
            yield return null;
            while (!run.Finished)
            {
                float frame = input.FrameTime;
                if (frame > 0f) run.Tick(frame, input.Sample());
                while (run.TryTakeCue(out var part))
                    if (!run.Finished) PlayPart(part);
                sketch.Clear(run.AtTarget ? world : null);
                run.Draw(sketch);
                cues.ShowGesture(sketch);
                yield return null;
            }
            cues.HideTimingCue();
            Announce(world, run.Result, "PERFECT!", "GOOD", SfxId.Click);
            done(run.Result);
        }

        void PlayPart(TimingResult part)
        {
            if (part != TimingResult.Miss) sounds.Play(part == TimingResult.Perfect ? SfxId.Buff : SfxId.Click);
        }

        public IEnumerator Block(Vector3 world, Action<TimingResult> done)
        {
            bool wasted = false;
            float windup = random.Range(config.windupMin, config.windupMax);
            yield return null;
            for (float t = 0f; t < windup; t += input.FrameTime)
            {
                wasted |= input.Pressed();
                yield return null;
            }

            var result = TimingResult.Miss;
            for (float t = 0f; t <= config.blockLead + judge.GoodWindow; t += input.FrameTime)
            {
                cues.ShowBlockCue(world);
                if (!wasted && input.Pressed())
                {
                    result = judge.Judge(t - config.blockLead);
                    break;
                }
                yield return null;
            }
            cues.HideTimingCue();
            Announce(world, result, "PERFECT BLOCK!", "BLOCK", SfxId.Block);
            done(result);
        }

        void Announce(Vector3 world, TimingResult result, string perfect, string good, SfxId success)
        {
            if (result == TimingResult.Miss) return;
            bool isPerfect = result == TimingResult.Perfect;
            view.Popup(world, isPerfect ? perfect : good, isPerfect ? PopupKind.Crit : PopupKind.Buff);
            sounds.Play(success);
            if (isPerfect) sounds.Play(SfxId.Perfect, 1f, 0f);
        }
    }
}
