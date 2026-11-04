using System.Collections;
using System.Linq;
using Featherguard.Feel;
using UnityEngine;

namespace Featherguard.Battle
{
    public sealed class BossPhases
    {
        readonly Combat combat;
        readonly WaveSpawner spawner;

        public BossPhases(Combat combat, WaveSpawner spawner)
        {
            this.combat = combat;
            this.spawner = spawner;
        }

        public IEnumerator Check()
        {
            foreach (var boss in combat.Enemies.Living.Where(ShouldEnrage).ToList())
                yield return Enrage(boss);
        }

        static bool ShouldEnrage(BattleUnit unit)
        {
            var profile = unit.Definition.boss;
            return !unit.Enraged && profile.enrageAt > 0f && unit.HealthFraction <= profile.enrageAt;
        }

        IEnumerator Enrage(BattleUnit boss)
        {
            var profile = boss.Definition.boss;
            boss.Enraged = true;
            combat.View.Callout($"{boss.Definition.displayName} wpada w szał!");
            combat.Feedback.Play(SfxId.Rage, 1f, 0f);
            combat.Feedback.Shake(0.25f, 0.6f);
            combat.Feedback.Burst(boss.Center, new Color(1f, 0.3f, 0.1f), 40, 7f, 0.45f);
            boss.AddModifier(new StatModifier(StatType.Damage, multiply: profile.enrageDamageBonus));
            boss.Animator.Play("Victory", 0.1f);
            yield return new WaitForSeconds(0.9f);
            foreach (var summon in profile.enrageSummons)
            {
                if (!spawner.TrySummon(summon)) break;
                yield return new WaitForSeconds(0.25f);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
}
