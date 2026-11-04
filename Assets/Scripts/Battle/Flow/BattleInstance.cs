using System;
using System.Collections;
using System.Linq;
using Featherguard.Core;
using Featherguard.Feel;
using UnityEngine;

namespace Featherguard.Battle
{
    public sealed class BattleServices
    {
        public BattleRules Rules;
        public UnitFactory Factory;
        public IBattleView View;
        public ICombatFeedback Feedback;
        public MonoBehaviour Host;
        public Camera Camera;
        public IRandomSource Random = UnityRandomSource.Shared;
        public IEnemyBrain Brain;
    }

    public sealed class BattleInstance
    {
        const string PlayerTurnText = "Twoja tura";
        const string EnemyTurnText = "Tura wroga";

        readonly BattleSetup setup;
        readonly BattleServices services;
        readonly ActionExecutor executor;
        readonly StatusTicker ticker;
        readonly WaveSpawner waves;
        readonly BossPhases bosses;
        readonly IEnemyBrain brain;
        readonly PlayerCommands commands;

        public Combat Combat { get; }
        public IPlayerCommands Commands => commands;
        public RageMeter Rage => Combat.Rage;
        public BattleResult Result { get; private set; }

        public event Action<BattleUnit> EnemyKilled;

        public BattleInstance(BattleSetup setup, BattleServices services)
        {
            this.setup = setup;
            this.services = services;
            Combat = new Combat(services.Rules, services.Random, services.Feedback, services.View, services.Host, setup.StartRage);
            Combat.UnitKilled += unit =>
            {
                if (unit.Team == Team.Enemies) EnemyKilled?.Invoke(unit);
            };
            executor = new ActionExecutor(Combat);
            ticker = new StatusTicker(Combat);
            waves = new WaveSpawner(Combat, services.Factory, setup);
            bosses = new BossPhases(Combat, waves);
            brain = services.Brain ?? new DefaultEnemyBrain(services.Random);
            var picker = new PointerUnitPicker(services.Camera, () => Combat.Heroes.Concat(Combat.Enemies), services.Rules.tapTolerance);
            commands = new PlayerCommands(Combat, executor, picker);
        }

        public void Tick() => commands.Tick();

        public IEnumerator Run()
        {
            SpawnHeroes();
            yield return waves.SpawnNext();

            while (true)
            {
                yield return PlayerTurn();
                if (Finished(out bool victory)) { yield return Finish(victory); yield break; }
                if (!Combat.Enemies.AnyAlive)
                {
                    yield return NextWave();
                    continue;
                }

                yield return EnemyTurn();
                if (Finished(out victory)) { yield return Finish(victory); yield break; }
                if (!Combat.Enemies.AnyAlive)
                    yield return NextWave();
            }
        }

        void SpawnHeroes()
        {
            var slots = services.Rules.heroSlots;
            for (int i = 0; i < setup.Heroes.Count && i < slots.Length; i++)
            {
                var loadout = setup.Heroes[i];
                var hero = services.Factory.Spawn(loadout.Definition, slots[i]);
                foreach (var modifier in loadout.Modifiers)
                    hero.AddModifier(modifier);
                hero.SetHealth(loadout.Health ?? hero.MaxHealth);
                Combat.Heroes.Add(hero);
                services.View.UnitAdded(hero);
            }
            services.View.HeroesReady(Combat.Heroes.Snapshot());
        }

        bool Finished(out bool victory)
        {
            victory = false;
            if (!Combat.Heroes.AnyAlive) return true;
            victory = !Combat.Enemies.AnyAlive && !waves.HasNextWave;
            return victory;
        }

        IEnumerator NextWave()
        {
            yield return new WaitForSeconds(services.Rules.waveDelay);
            yield return waves.SpawnNext();
        }

        IEnumerator PlayerTurn()
        {
            services.View.ShowBanner(PlayerTurnText);
            yield return ticker.BeginTurn(Combat.Heroes);
            foreach (var hero in Combat.Heroes)
            {
                hero.HasActed = hero.SkipsTurn;
                hero.NotifyChanged();
            }
            commands.Open();

            while (Combat.Enemies.AnyAlive && Combat.Heroes.Any(h => h.IsAlive && !h.HasActed))
            {
                if (commands.Pending != null)
                {
                    commands.Suspend();
                    yield return commands.Pending;
                    commands.ClearPending();
                    yield return bosses.Check();
                    if (!Combat.Enemies.AnyAlive) break;
                    commands.Open();
                }
                yield return null;
            }

            commands.Close();
            ticker.EndTurn(Combat.Heroes);
        }

        IEnumerator EnemyTurn()
        {
            services.View.ShowBanner(EnemyTurnText);
            yield return new WaitForSeconds(services.Rules.enemyTurnDelay);
            yield return ticker.BeginTurn(Combat.Enemies);
            foreach (var enemy in Combat.Enemies.Snapshot())
            {
                if (!enemy.IsAlive || enemy.SkipsTurn) continue;
                if (!Combat.Heroes.AnyAlive) break;
                var decision = brain.Decide(enemy, Combat);
                enemy.TurnsTaken++;
                yield return executor.Execute(enemy, decision.Ability, decision.Target);
                yield return bosses.Check();
                yield return new WaitForSeconds(services.Rules.betweenEnemyActions);
            }
            ticker.EndTurn(Combat.Enemies);
        }

        IEnumerator Finish(bool victory)
        {
            yield return new WaitForSeconds(0.6f);
            foreach (var unit in victory ? Combat.Heroes.Living : Combat.Enemies.Living)
                unit.Animator.Play("Victory", 0.15f);
            services.Feedback.Play(victory ? SfxId.Victory : SfxId.Defeat, 1f, 0f);
            Result = new BattleResult { Victory = victory, Heroes = Combat.Heroes.Snapshot() };
        }
    }
}
