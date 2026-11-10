using System.Collections;
using System.Linq;
using Featherguard.Feedback;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public sealed class ActionPlayback
    {
        const string StunnedText = "Stunned";

        readonly BattlePresentationConfig config;
        readonly AbilityVisualsLibrary visuals;
        readonly IBattleView view;
        readonly ICombatFeedback feedback;
        readonly MonoBehaviour host;
        readonly UnitStage stage;
        readonly BattleEventRouter router;

        public ActionPlayback(BattlePresentationConfig config, AbilityVisualsLibrary visuals, IBattleView view, ICombatFeedback feedback, MonoBehaviour host, UnitStage stage, BattleEventRouter router)
        {
            this.config = config;
            this.visuals = visuals;
            this.view = view;
            this.feedback = feedback;
            this.host = host;
            this.stage = stage;
            this.router = router;
        }

        public void Register()
        {
            router.On<ActionPerformed>(Perform);
            router.OnInstant<Damaged>(ShowDamage);
            router.OnInstant<Healed>(ShowHeal);
            router.OnInstant<StatusApplied>(ShowStatus);
            router.On<StatusTicked>(Tick);
            router.On<Stunned>(Stun);
        }

        IEnumerator Perform(ActionPerformed e)
        {
            var actor = stage.ViewOf(e.Actor);
            var ability = e.Ability;
            var look = visuals.Of(ability);
            if (e.Special)
            {
                view.Callout(ability.displayName);
                feedback.Play(SfxId.Rage);
                yield return BattleEventRouter.Wait(config.specialWindup);
            }

            var pending = e.Impacts.ToList();
            void Land(int hit, UnitView target)
            {
                if (target == null) return;
                int index = pending.FindIndex(i => i.Hit == hit && stage.ViewOf(i.Target) == target);
                if (index < 0) return;
                var impact = pending[index];
                pending.RemoveAt(index);
                if (!ability.HasDirectImpact)
                {
                    feedback.Rise(target.transform.position, look.fxColor, target.Radius);
                    feedback.Play(look.impactSound);
                }
                router.PlayInstant(impact.Outcomes);
            }

            if (actor != null && look.delivery != null)
            {
                yield return look.delivery.Deliver(new DeliveryContext
                {
                    Actor = actor,
                    Targets = e.Targets.Select(stage.ViewOf).Where(v => v != null).ToList(),
                    Ability = ability,
                    Visuals = look,
                    Feedback = feedback,
                    Host = host,
                    Impact = Land,
                });
            }
            foreach (var impact in pending.ToList())
            {
                pending.Remove(impact);
                router.PlayInstant(impact.Outcomes);
            }
            router.PlayInstant(e.Aftermath);

            yield return BattleEventRouter.Wait(config.afterAction);
            if (e.Actor.Team == Team.Enemies)
                yield return BattleEventRouter.Wait(config.betweenEnemyActions);
        }

        void ShowDamage(Damaged e)
        {
            var target = stage.ViewOf(e.Target);
            var source = stage.ViewOf(e.Source);
            if (target == null) return;
            var look = visuals.Of(e.Ability);
            bool heavy = look.heavy || e.Critical;
            var facing = source != null ? source.Facing : -target.Facing;
            var contact = target.Center - facing * target.Radius * 0.7f + Vector3.back * target.Radius;
            feedback.Impact(contact, look.fxColor, heavy);
            feedback.Play(look.impactSound);
            target.Reaction.Flash();
            target.Reaction.Knockback(facing, heavy ? config.heavyKnockback : config.lightKnockback);
            view.Popup(target.Top, e.Critical ? $"{e.Amount}!" : e.Amount.ToString(), e.Critical ? PopupKind.Crit : PopupKind.Damage);
            if (target.IsAlive) target.Animator.Play("Hit");
        }

        void ShowHeal(Healed e)
        {
            var target = stage.ViewOf(e.Target);
            if (target == null) return;
            if (e.Ability != null)
            {
                var look = visuals.Of(e.Ability);
                feedback.Rise(target.transform.position, look.fxColor, target.Radius);
                feedback.Play(look.impactSound);
            }
            view.Popup(target.Top, $"+{e.Amount}", PopupKind.Heal);
        }

        void ShowStatus(StatusApplied e)
        {
            var target = stage.ViewOf(e.Target);
            if (target != null)
                view.Popup(target.Top + Vector3.up * 0.35f, e.Status.announcement, PopupKind.Buff);
        }

        IEnumerator Tick(StatusTicked e)
        {
            var unit = stage.ViewOf(e.Unit);
            if (unit == null) yield break;
            unit.Reaction.Flash();
            feedback.Rise(unit.transform.position, e.Status.tint, unit.Radius);
            feedback.Play(SfxId.Hit, 0.5f);
            view.Popup(unit.Top, e.Amount.ToString(), PopupKind.Damage);
            if (unit.IsAlive) unit.Animator.Play("Hit");
            yield return BattleEventRouter.Wait(config.statusTickInterval);
        }

        IEnumerator Stun(Stunned e)
        {
            var unit = stage.ViewOf(e.Unit);
            if (unit == null) yield break;
            view.Popup(unit.Top, StunnedText, PopupKind.Buff);
            yield return BattleEventRouter.Wait(config.stunnedInterval);
        }
    }
}
