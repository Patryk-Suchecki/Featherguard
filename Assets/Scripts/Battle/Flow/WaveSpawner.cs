using System;
using System.Collections;
using System.Linq;
using Featherguard.Feel;
using UnityEngine;

namespace Featherguard.Battle
{
    public sealed class WaveSpawner
    {
        readonly Combat combat;
        readonly UnitFactory factory;
        readonly BattleSetup setup;

        public int WaveIndex { get; private set; } = -1;
        public int WaveCount => setup.Encounter.waves.Length;
        public bool HasNextWave => WaveIndex + 1 < WaveCount;

        public WaveSpawner(Combat combat, UnitFactory factory, BattleSetup setup)
        {
            this.combat = combat;
            this.factory = factory;
            this.setup = setup;
        }

        public IEnumerator SpawnNext()
        {
            WaveIndex++;
            foreach (var fallen in combat.Enemies)
                Remove(fallen);
            combat.Enemies.Clear();

            var encounter = setup.Encounter;
            combat.View.SetProgress(setup.EncounterNumber, setup.EncounterCount, WaveIndex + 1, WaveCount);
            combat.View.ShowBanner(Banner(encounter));

            var wave = encounter.waves[WaveIndex].enemies;
            var slots = combat.Rules.enemySlots;
            for (int i = 0; i < wave.Length && i < slots.Length; i++)
            {
                Spawn(wave[i], slots[i]);
                yield return new WaitForSeconds(combat.Rules.spawnInterval);
            }
            yield return new WaitForSeconds(combat.Rules.afterSpawn);
        }

        public bool TrySummon(UnitDefinition definition)
        {
            var slots = combat.Rules.enemySlots;
            int slot = Array.FindIndex(slots, s => !combat.Enemies.Any(e => e.IsAlive && Occupies(e, s)));
            if (slot < 0) return false;
            foreach (var fallen in combat.Enemies.Where(e => !e.IsAlive && Occupies(e, slots[slot])).ToList())
            {
                combat.Enemies.Remove(fallen);
                Remove(fallen);
            }
            Spawn(definition, slots[slot]);
            return true;
        }

        void Spawn(UnitDefinition definition, Vector3 slot)
        {
            var unit = factory.Spawn(definition, slot + Vector3.right * combat.Rules.spawnOffset);
            unit.HomePosition = slot;
            unit.AddModifier(new StatModifier(StatType.MaxHealth, multiply: setup.EnemyHealthScale - 1f));
            unit.AddModifier(new StatModifier(StatType.Damage, multiply: setup.EnemyDamageScale - 1f));
            unit.SetHealth(unit.MaxHealth);
            combat.Enemies.Add(unit);
            combat.View.UnitAdded(unit);
            combat.Feedback.Play(SfxId.Whoosh, 0.5f);
            combat.Host.StartCoroutine(UnitMotion.HopTo(unit.transform, slot, 0.55f, 1.1f));
        }

        void Remove(BattleUnit unit)
        {
            combat.View.UnitRemoved(unit);
            combat.Host.StartCoroutine(UnitMotion.SinkAndDestroy(unit.transform));
        }

        string Banner(EncounterDefinition encounter)
        {
            if (WaveIndex > 0)
                return WaveIndex == WaveCount - 1 ? "Ostatnia fala!" : $"Fala {WaveIndex + 1}";
            if (encounter.boss) return $"BOSS: {encounter.displayName}";
            return setup.Elite ? $"ELITA: {encounter.displayName}" : encounter.displayName;
        }

        static bool Occupies(BattleUnit unit, Vector3 slot) => Vector3.Distance(unit.HomePosition, slot) < 0.1f;
    }
}
