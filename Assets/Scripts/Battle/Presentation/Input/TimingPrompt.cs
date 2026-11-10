using System;
using System.Collections;
using Featherguard.Core;
using Featherguard.Feel;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Featherguard.Battle.Presentation
{
    public sealed class TimingPrompt
    {
        readonly BattlePresentationConfig config;
        readonly IBattleView view;
        readonly ICombatFeedback feedback;
        readonly IRandomSource random;

        public TimingPrompt(BattlePresentationConfig config, IBattleView view, ICombatFeedback feedback, IRandomSource random)
        {
            this.config = config;
            this.view = view;
            this.feedback = feedback;
            this.random = random;
        }

        static float Step => Time.unscaledDeltaTime * GameClock.Scale;

        public IEnumerator Attack(Vector3 world, Action<TimingResult> done)
        {
            var result = TimingResult.Miss;
            float target = config.attackRingTime;
            yield return null;
            for (float t = 0f; t <= target + config.goodWindow; t += Step)
            {
                view.ShowAttackRing(world, Mathf.Clamp01(1f - t / target));
                if (Pressed())
                {
                    result = Judge(t - target);
                    break;
                }
                yield return null;
            }
            view.HideTimingCue();
            Announce(world, result, "PERFECT!", "GOOD");
            done(result);
        }

        public IEnumerator Block(Vector3 world, Action<TimingResult> done)
        {
            bool wasted = false;
            float windup = random.Range(config.windupMin, config.windupMax);
            yield return null;
            for (float t = 0f; t < windup; t += Step)
            {
                wasted |= Pressed();
                yield return null;
            }

            var result = TimingResult.Miss;
            for (float t = 0f; t <= config.blockLead + config.goodWindow; t += Step)
            {
                view.ShowBlockCue(world);
                if (!wasted && Pressed())
                {
                    result = Judge(t - config.blockLead);
                    break;
                }
                yield return null;
            }
            view.HideTimingCue();
            Announce(world, result, "PERFECT BLOCK!", "BLOCK");
            done(result);
        }

        TimingResult Judge(float offset)
        {
            float distance = Mathf.Abs(offset);
            if (distance <= config.perfectWindow) return TimingResult.Perfect;
            return distance <= config.goodWindow ? TimingResult.Good : TimingResult.Miss;
        }

        void Announce(Vector3 world, TimingResult result, string perfect, string good)
        {
            if (result == TimingResult.Miss) return;
            bool isPerfect = result == TimingResult.Perfect;
            view.Popup(world, isPerfect ? perfect : good, isPerfect ? PopupKind.Crit : PopupKind.Buff);
            feedback.Play(isPerfect ? SfxId.Buff : SfxId.Click);
        }

        static bool Pressed()
        {
            if (GameClock.Paused) return false;
            bool tap = Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
            bool space = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
            return tap || space;
        }
    }
}
