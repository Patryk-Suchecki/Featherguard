using System;
using Featherguard.Core;
using Featherguard.Feel;
using UnityEngine;

namespace Featherguard.Battle
{
    public sealed class Combat
    {
        public BattleRules Rules { get; }
        public IRandomSource Random { get; }
        public ICombatFeedback Feedback { get; }
        public IBattleView View { get; }
        public RageMeter Rage { get; }
        public Squad Heroes { get; } = new(Team.Heroes);
        public Squad Enemies { get; } = new(Team.Enemies);
        public MonoBehaviour Host { get; }

        public event Action<BattleUnit> UnitKilled;

        public Combat(BattleRules rules, IRandomSource random, ICombatFeedback feedback, IBattleView view, MonoBehaviour host, float startRage)
        {
            Rules = rules;
            Random = random;
            Feedback = feedback;
            View = view;
            Host = host;
            Rage = new RageMeter(rules.maxRage, startRage);
            Rage.BecameReady += () => Feedback.Play(SfxId.Buff, 0.8f, 0f);
        }

        public Squad AlliesOf(BattleUnit unit) => unit.Team == Team.Heroes ? Heroes : Enemies;
        public Squad OpponentsOf(BattleUnit unit) => unit.Team == Team.Heroes ? Enemies : Heroes;

        public void DealDamage(BattleUnit actor, BattleUnit target, int power, AbilityDefinition ability)
        {
            var roll = DamageCalculator.Roll(power, actor.Stat(StatType.Damage), actor.Stat(StatType.CritChance), actor.Stat(StatType.CritDamage), Rules.damageVariance, Random);
            int dealt = target.ApplyDamage(roll.Amount);
            bool heavy = ability.heavy || roll.Critical;

            var contact = target.Center - actor.Facing * target.Radius * 0.7f + Vector3.back * target.Radius;
            Feedback.Impact(contact, ability.fxColor, heavy);
            Feedback.Play(ability.impactSound);
            target.Reaction.Flash();
            target.Reaction.Knockback(actor.Facing, heavy ? Rules.heavyKnockback : Rules.lightKnockback);
            View.Popup(target.Top, roll.Critical ? $"{dealt}!" : dealt.ToString(), roll.Critical ? PopupKind.Crit : PopupKind.Damage);
            Rage.Add(dealt * (actor.Team == Team.Heroes ? Rules.heroRagePerDamage * actor.Stat(StatType.RageGain) : Rules.enemyRagePerDamage));

            float lifesteal = actor.Stat(StatType.Lifesteal);
            if (lifesteal > 0f && actor.IsAlive)
            {
                int stolen = actor.ApplyHeal(Mathf.RoundToInt(dealt * lifesteal));
                if (stolen > 0) View.Popup(actor.Top, $"+{stolen}", PopupKind.Heal);
            }
            ReactToHit(target);
        }

        public void Heal(BattleUnit actor, BattleUnit target, int power, AbilityDefinition ability)
        {
            int healed = target.ApplyHeal(DamageCalculator.Heal(power, actor.Stat(StatType.Healing)));
            Feedback.Rise(target.transform.position, ability.fxColor, target.Radius);
            Feedback.Play(ability.impactSound);
            View.Popup(target.Top, $"+{healed}", PopupKind.Heal);
        }

        public void TryApplyStatus(BattleUnit actor, BattleUnit target, StatusApplication application)
        {
            if (application.status == null || !target.IsAlive || Random.Value > application.chance) return;
            target.Statuses.Apply(application, target.Team == actor.Team);
            View.Popup(target.Top + Vector3.up * 0.35f, application.status.announcement, PopupKind.Buff);
        }

        public void DamageOverTime(BattleUnit unit, int amount, Color color)
        {
            int dealt = unit.ApplyDamage(amount);
            unit.Reaction.Flash();
            Feedback.Rise(unit.transform.position, color, unit.Radius);
            Feedback.Play(SfxId.Hit, 0.5f);
            View.Popup(unit.Top, dealt.ToString(), PopupKind.Damage);
            ReactToHit(unit);
        }

        void ReactToHit(BattleUnit unit)
        {
            if (unit.IsAlive)
                unit.Animator.Play("Hit");
            else
                Kill(unit);
        }

        void Kill(BattleUnit unit)
        {
            unit.Animator.Play("Death", 0.04f);
            unit.Ring.Show(null);
            unit.Statuses.Clear();
            Feedback.Play(SfxId.Death);
            UnitKilled?.Invoke(unit);
        }
    }
}
