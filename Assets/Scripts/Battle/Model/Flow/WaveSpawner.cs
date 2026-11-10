using System.Linq;

namespace Featherguard.Battle
{
    public sealed class WaveSpawner
    {
        readonly BattleState state;
        readonly BattleSetup setup;

        public int WaveIndex { get; private set; } = -1;
        public int WaveCount => setup.Encounter.waves.Length;
        public bool HasNextWave => WaveIndex + 1 < WaveCount;

        public WaveSpawner(BattleState state, BattleSetup setup)
        {
            this.state = state;
            this.setup = setup;
        }

        public void SpawnNext()
        {
            WaveIndex++;
            state.Log.Emit(new WaveStarted
            {
                Encounter = setup.Encounter,
                Wave = WaveIndex,
                Waves = WaveCount,
                EncounterNumber = setup.EncounterNumber,
                EncounterCount = setup.EncounterCount,
                Elite = setup.Elite,
            });
            foreach (var fallen in state.Enemies.Snapshot())
                state.Remove(fallen);

            var wave = setup.Encounter.waves[WaveIndex].enemies;
            for (int i = 0; i < wave.Length && i < state.Rules.enemySlotCount; i++)
                Spawn(wave[i], i, false);
            state.Log.Emit(new WaveSpawned());
        }

        public bool TrySummon(UnitDefinition definition)
        {
            int slot = Enumerable.Range(0, state.Rules.enemySlotCount).Select(i => (int?)i)
                .FirstOrDefault(s => !state.Enemies.Any(e => e.IsAlive && e.Slot == s)) ?? -1;
            if (slot < 0) return false;
            foreach (var fallen in state.Enemies.Where(e => !e.IsAlive && e.Slot == slot).ToList())
                state.Remove(fallen);
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
            state.Spawn(definition, slot, scaling, null, summoned);
        }
    }
}
