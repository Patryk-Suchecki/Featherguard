using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Featherguard.Feel;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public sealed class BattlePresenter
    {
        const string PlayerTurnText = "Twoja tura";
        const string EnemyTurnText = "Tura wroga";
        const string StunnedText = "Ogłuszony";
        static readonly Color EnrageColor = new(1f, 0.3f, 0.1f);

        readonly BattlePresentationConfig config;
        readonly UnitViewFactory factory;
        readonly IBattleView view;
        readonly ICombatFeedback feedback;
        readonly MonoBehaviour host;
        readonly Dictionary<BattleUnit, UnitView> views = new();

        public RageMeter Rage { get; }
        public IEnumerable<UnitView> Views => views.Values;

        public event Action<UnitView> UnitDefeated;

        public BattlePresenter(BattlePresentationConfig config, UnitViewFactory factory, IBattleView view, ICombatFeedback feedback, MonoBehaviour host, RageMeter rage)
        {
            this.config = config;
            this.factory = factory;
            this.view = view;
            this.feedback = feedback;
            this.host = host;
            Rage = new RageMeter(rage.Max, rage.Value);
        }

        public UnitView ViewOf(BattleUnit unit) => unit != null && views.TryGetValue(unit, out var unitView) ? unitView : null;

        public IEnumerator Play(IReadOnlyList<BattleEvent> events)
        {
            foreach (var battleEvent in events)
            {
                var routine = Play(battleEvent);
                if (routine != null) yield return routine;
            }
        }

        IEnumerator Play(BattleEvent battleEvent)
        {
            switch (battleEvent)
            {
                case UnitSpawned e: return Spawn(e);
                case UnitRemoved e: Remove(e); return null;
                case UnitChanged e: ViewOf(e.Unit)?.Show(e.State); return null;
                case WaveStarted e: return StartWave(e);
                case WaveSpawned: return Wait(config.afterSpawn);
                case TurnStarted e: return StartTurn(e);
                case ActionPerformed e: return Perform(e);
                case Damaged e: ShowDamage(e); return null;
                case Healed e: ShowHeal(e); return null;
                case StatusApplied e: ShowStatus(e); return null;
                case StatusTicked e: return Tick(e);
                case Stunned e: return Stun(e);
                case Died e: ShowDeath(e); return null;
                case Enraged e: return Enrage(e);
                case RageChanged e: ShowRage(e); return null;
                case BattleEnded e: return End(e);
                default: return null;
            }
        }

        void PlayInstant(IEnumerable<BattleEvent> events)
        {
            foreach (var battleEvent in events)
            {
                var routine = Play(battleEvent);
                if (routine != null) host.StartCoroutine(routine);
            }
        }

        static IEnumerator Wait(float seconds)
        {
            if (seconds > 0f) yield return new WaitForSeconds(seconds);
        }

        IEnumerator Spawn(UnitSpawned e)
        {
            var home = config.SlotPosition(e.Unit.Team, e.Unit.Slot);
            bool hero = e.Unit.Team == Team.Heroes;
            var unitView = factory.Create(e.Unit, e.State, hero ? home : home + Vector3.right * config.spawnOffset);
            unitView.HomePosition = home;
            views[e.Unit] = unitView;
            view.UnitAdded(unitView);
            if (hero) yield break;
            feedback.Play(SfxId.Whoosh, 0.5f);
            host.StartCoroutine(UnitMotion.HopTo(unitView.transform, home, 0.55f, 1.1f));
            yield return Wait(e.Summoned ? config.summonInterval : config.spawnInterval);
        }

        void Remove(UnitRemoved e)
        {
            var unitView = ViewOf(e.Unit);
            if (unitView == null) return;
            views.Remove(e.Unit);
            view.UnitRemoved(unitView);
            host.StartCoroutine(UnitMotion.SinkAndDestroy(unitView.transform));
        }

        IEnumerator StartWave(WaveStarted e)
        {
            if (e.Wave > 0) yield return Wait(config.waveDelay);
            view.SetProgress(e.EncounterNumber, e.EncounterCount, e.Wave + 1, e.Waves);
            view.ShowBanner(WaveBanner(e));
        }

        static string WaveBanner(WaveStarted e)
        {
            if (e.Wave > 0)
                return e.Wave == e.Waves - 1 ? "Ostatnia fala!" : $"Fala {e.Wave + 1}";
            if (e.Encounter.boss) return $"BOSS: {e.Encounter.displayName}";
            return e.Elite ? $"ELITA: {e.Encounter.displayName}" : e.Encounter.displayName;
        }

        IEnumerator StartTurn(TurnStarted e)
        {
            bool heroes = e.Team == Team.Heroes;
            view.ShowBanner(heroes ? PlayerTurnText : EnemyTurnText);
            if (!heroes) yield return Wait(config.enemyTurnDelay);
        }

        IEnumerator Perform(ActionPerformed e)
        {
            var actor = ViewOf(e.Actor);
            var ability = e.Ability;
            if (e.Special)
            {
                view.Callout(ability.displayName);
                feedback.Play(SfxId.Rage);
                yield return Wait(config.specialWindup);
            }

            var pending = e.Impacts.ToList();
            void Land(int hit, UnitView target)
            {
                if (target == null) return;
                int index = pending.FindIndex(i => i.Hit == hit && ViewOf(i.Target) == target);
                if (index < 0) return;
                var impact = pending[index];
                pending.RemoveAt(index);
                if (!ability.HasDirectImpact)
                {
                    feedback.Rise(target.transform.position, ability.fxColor, target.Radius);
                    feedback.Play(ability.impactSound);
                }
                PlayInstant(impact.Outcomes);
            }

            if (actor != null && ability.delivery != null)
            {
                yield return ability.delivery.Deliver(new DeliveryContext
                {
                    Actor = actor,
                    Targets = e.Targets.Select(ViewOf).Where(v => v != null).ToList(),
                    Ability = ability,
                    Feedback = feedback,
                    Host = host,
                    Impact = Land,
                });
            }
            foreach (var impact in pending.ToList())
            {
                pending.Remove(impact);
                PlayInstant(impact.Outcomes);
            }
            PlayInstant(e.Aftermath);

            yield return Wait(config.afterAction);
            if (e.Actor.Team == Team.Enemies)
                yield return Wait(config.betweenEnemyActions);
        }

        void ShowDamage(Damaged e)
        {
            var target = ViewOf(e.Target);
            var source = ViewOf(e.Source);
            if (target == null) return;
            bool heavy = e.Ability.heavy || e.Critical;
            var facing = source != null ? source.Facing : -target.Facing;
            var contact = target.Center - facing * target.Radius * 0.7f + Vector3.back * target.Radius;
            feedback.Impact(contact, e.Ability.fxColor, heavy);
            feedback.Play(e.Ability.impactSound);
            target.Reaction.Flash();
            target.Reaction.Knockback(facing, heavy ? config.heavyKnockback : config.lightKnockback);
            view.Popup(target.Top, e.Critical ? $"{e.Amount}!" : e.Amount.ToString(), e.Critical ? PopupKind.Crit : PopupKind.Damage);
            if (target.IsAlive) target.Animator.Play("Hit");
        }

        void ShowHeal(Healed e)
        {
            var target = ViewOf(e.Target);
            if (target == null) return;
            if (e.Ability != null)
            {
                feedback.Rise(target.transform.position, e.Ability.fxColor, target.Radius);
                feedback.Play(e.Ability.impactSound);
            }
            view.Popup(target.Top, $"+{e.Amount}", PopupKind.Heal);
        }

        void ShowStatus(StatusApplied e)
        {
            var target = ViewOf(e.Target);
            if (target != null)
                view.Popup(target.Top + Vector3.up * 0.35f, e.Status.announcement, PopupKind.Buff);
        }

        IEnumerator Tick(StatusTicked e)
        {
            var unit = ViewOf(e.Unit);
            if (unit == null) yield break;
            unit.Reaction.Flash();
            feedback.Rise(unit.transform.position, e.Status.tint, unit.Radius);
            feedback.Play(SfxId.Hit, 0.5f);
            view.Popup(unit.Top, e.Amount.ToString(), PopupKind.Damage);
            if (unit.IsAlive) unit.Animator.Play("Hit");
            yield return Wait(config.statusTickInterval);
        }

        IEnumerator Stun(Stunned e)
        {
            var unit = ViewOf(e.Unit);
            if (unit == null) yield break;
            view.Popup(unit.Top, StunnedText, PopupKind.Buff);
            yield return Wait(config.stunnedInterval);
        }

        void ShowDeath(Died e)
        {
            var unit = ViewOf(e.Unit);
            if (unit == null) return;
            unit.Animator.Play("Death", 0.04f);
            unit.Ring.Show(null);
            feedback.Play(SfxId.Death);
            UnitDefeated?.Invoke(unit);
        }

        IEnumerator Enrage(Enraged e)
        {
            var boss = ViewOf(e.Boss);
            view.Callout($"{e.Boss.Definition.displayName} wpada w szał!");
            feedback.Play(SfxId.Rage, 1f, 0f);
            feedback.Shake(0.25f, 0.6f);
            if (boss != null)
            {
                feedback.Burst(boss.Center, EnrageColor, 40, 7f, 0.45f);
                boss.Animator.Play("Victory", 0.1f);
            }
            yield return Wait(config.enrageHold);
        }

        void ShowRage(RageChanged e)
        {
            Rage.Set(e.Value);
            if (e.BecameReady) feedback.Play(SfxId.Buff, 0.8f, 0f);
        }

        IEnumerator End(BattleEnded e)
        {
            yield return Wait(config.endDelay);
            var winners = e.Victory ? Team.Heroes : Team.Enemies;
            foreach (var unit in Views.Where(v => v.Team == winners && v.IsAlive))
                unit.Animator.Play("Victory", 0.15f);
            feedback.Play(e.Victory ? SfxId.Victory : SfxId.Defeat, 1f, 0f);
        }
    }
}
