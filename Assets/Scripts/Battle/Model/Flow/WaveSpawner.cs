using System.Linq;

namespace Featherguard.Battle
{
    public sealed class WaveSpawner
    {
        readonly Combat combat;
        readonly BattleSetup setup;

        public int WaveIndex { get; private set; } = -1;
        public int WaveCount => setup.Encounter.waves.Length;
        public bool HasNextWave => WaveIndex + 1 < WaveCount;

        public WaveSpawner(Combat combat, BattleSetup setup)
        {
            this.combat = combat;
            this.setup = setup;
        }

        public void SpawnNext()
        {
            WaveIndex++;
            combat.Log.Emit(new WaveStarted
            {
                Encounter = setup.Encounter,
                Wave = WaveIndex,
                Waves = WaveCount,
                EncounterNumber = setup.EncounterNumber,
                EncounterCount = setup.EncounterCount,
                Elite = setup.Elite,
            });
            foreach (var fallen in combat.Enemies.Snapshot())
                combat.Remove(fallen);

            var wave = setup.Encounter.waves[WaveIndex].enemies;
            for (int i = 0; i < wave.Length && i < combat.Rules.enemySlotCount; i++)
                Spawn(wave[i], i, false);
            combat.Log.Emit(new WaveSpawned());
        }

        public bool TrySummon(UnitDefinition definition)
        {
            int slot = Enumerable.Range(0, combat.Rules.enemySlotCount).Select(i => (int?)i)
                .FirstOrDefault(s => !combat.Enemies.Any(e => e.IsAlive && e.Slot == s)) ?? -1;
            if (slot < 0) return false;
            foreach (var fallen in combat.Enemies.Where(e => !e.IsAlive && e.Slot == slot).ToList())
                combat.Remove(fallen);
            Spawn(definition, slot, true);
            return true;
        }

        void Spawn(UnitDefinition definition, int slot, bool summoned)
        {
            var scaling = new[]
            {
                new StatModifier(StatType.MaxHealth, multiply: setup.EnemyHealthScale - 1f),
                new StatModifier(StatType.Damage, multiply: setup.EnemyDamageScale - 1f),
            };
            combat.Spawn(definition, slot, scaling, null, summoned);
        }
    }
}
