using System.Linq;

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

        public void Check()
        {
            foreach (var boss in combat.Enemies.Living.Where(ShouldEnrage).ToList())
                Enrage(boss);
        }

        static bool ShouldEnrage(BattleUnit unit)
        {
            var profile = unit.Definition.boss;
            return !unit.Enraged && profile.enrageAt > 0f && unit.HealthFraction <= profile.enrageAt;
        }

        void Enrage(BattleUnit boss)
        {
            var profile = boss.Definition.boss;
            boss.Enraged = true;
            combat.Log.Emit(new Enraged { Boss = boss });
            boss.AddModifier(new StatModifier(StatType.Damage, multiply: profile.enrageDamageBonus));
            foreach (var summon in profile.enrageSummons)
                if (!spawner.TrySummon(summon))
                    break;
        }
    }
}
