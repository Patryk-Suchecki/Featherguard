using System.Linq;

namespace Featherguard.Battle
{
    public sealed class BossPhases
    {
        readonly BattleState state;
        readonly WaveSpawner spawner;

        public BossPhases(BattleState state, WaveSpawner spawner)
        {
            this.state = state;
            this.spawner = spawner;
        }

        public void Check()
        {
            foreach (var boss in state.Enemies.Living.Where(ShouldEnrage).ToList())
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
            state.Log.Emit(new Enraged { Boss = boss });
            boss.AddModifier(new StatModifier(StatType.Damage, multiply: profile.enrageDamageBonus));
            foreach (var summon in profile.enrageSummons)
                if (!spawner.TrySummon(summon))
                    break;
        }
    }
}
