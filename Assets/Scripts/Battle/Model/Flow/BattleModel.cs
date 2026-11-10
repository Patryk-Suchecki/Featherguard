using System;
using System.Collections.Generic;
using System.Linq;
using Featherguard.Core;

namespace Featherguard.Battle
{
    public enum BattlePhase { NotStarted, AwaitingInput, Finished }

    public sealed class BattleModel
    {
        const int MaxSteps = 10000;

        readonly BattleSetup setup;
        readonly BattleLog log = new();
        readonly ActionResolver resolver;
        readonly StatusTicker ticker;
        readonly WaveSpawner waves;
        readonly BossPhases bosses;
        readonly IEnemyBrain brain;

        public BattleState State { get; }
        public RageMeter Rage => State.Rage;
        public Squad Heroes => State.Heroes;
        public Squad Enemies => State.Enemies;
        public BattlePhase Phase { get; private set; }
        public bool AwaitingInput => Phase == BattlePhase.AwaitingInput;
        public bool IsFinished => Phase == BattlePhase.Finished;
        public int Turn { get; private set; }
        public BattleResult Result { get; private set; }

        public event Action<BattleUnit> EnemyKilled;

        public BattleModel(BattleSetup setup, BattleRules rules, IRandomSource random, IEnemyBrain brain = null)
        {
            this.setup = setup;
            State = new BattleState(rules, random, log, setup.StartRage);
            State.UnitKilled += unit =>
            {
                if (unit.Team == Team.Enemies) EnemyKilled?.Invoke(unit);
            };
            resolver = new ActionResolver(State);
            ticker = new StatusTicker(State);
            waves = new WaveSpawner(State, setup);
            bosses = new BossPhases(State, waves);
            this.brain = brain ?? new DefaultEnemyBrain(random);
        }

        public IReadOnlyList<BattleEvent> Start()
        {
            if (Phase != BattlePhase.NotStarted) throw new InvalidOperationException("Battle already started.");
            for (int i = 0; i < setup.Heroes.Count && i < State.Rules.heroSlotCount; i++)
            {
                var loadout = setup.Heroes[i];
                State.Spawn(loadout.Definition, i, loadout.Modifiers, loadout.Health);
            }
            waves.SpawnNext();
            BeginPlayerTurn();
            Advance();
            return log.Flush();
        }

        public IEnumerable<AbilityDefinition> AbilitiesOf(BattleUnit hero)
        {
            var definition = hero.Definition;
            if (definition.attack != null) yield return definition.attack;
            if (definition.support != null) yield return definition.support;
            if (definition.special != null && Rage.Ready) yield return definition.special;
        }

        public bool CanCommand(BattleUnit hero) => AwaitingInput && hero != null && hero.Team == Team.Heroes && hero.CanAct;

        public bool CanUse(BattleUnit hero, AbilityDefinition ability, BattleUnit target)
        {
            if (!CanCommand(hero) || ability == null || !AbilitiesOf(hero).Contains(ability)) return false;
            return ability.TargetsAll || Targeting.IsValid(ability, target, State.AlliesOf(hero), State.OpponentsOf(hero));
        }

        public IEnumerable<BattleUnit> Candidates(BattleUnit hero, AbilityDefinition ability) =>
            Targeting.Candidates(ability, State.AlliesOf(hero), State.OpponentsOf(hero));

        public IReadOnlyList<BattleEvent> Act(BattleUnit hero, AbilityDefinition ability, BattleUnit target)
        {
            if (!CanUse(hero, ability, target)) return Array.Empty<BattleEvent>();
            if (ability == hero.Definition.special)
                State.ConsumeRage();
            hero.HasActed = true;
            resolver.Execute(hero, ability, target);
            bosses.Check();
            Advance();
            return log.Flush();
        }

        void Advance()
        {
            for (int step = 0; step < MaxSteps; step++)
            {
                if (Enemies.AnyAlive && Heroes.Any(h => h.CanAct))
                {
                    Phase = BattlePhase.AwaitingInput;
                    return;
                }

                ticker.EndTurn(Heroes);
                if (CheckFinished()) return;
                if (!Enemies.AnyAlive)
                {
                    waves.SpawnNext();
                    BeginPlayerTurn();
                    continue;
                }

                EnemyTurn();
                if (CheckFinished()) return;
                if (!Enemies.AnyAlive)
                    waves.SpawnNext();
                BeginPlayerTurn();
            }
            throw new InvalidOperationException("Battle did not reach a decision point.");
        }

        void BeginPlayerTurn()
        {
            Turn++;
            log.Emit(new TurnStarted { Team = Team.Heroes });
            ticker.BeginTurn(Heroes);
            foreach (var hero in Heroes)
                hero.HasActed = hero.SkipsTurn;
        }

        void EnemyTurn()
        {
            log.Emit(new TurnStarted { Team = Team.Enemies });
            ticker.BeginTurn(Enemies);
            foreach (var enemy in Enemies.Snapshot())
            {
                if (!enemy.IsAlive || enemy.SkipsTurn) continue;
                if (!Heroes.AnyAlive) break;
                int actions = Math.Max(1, enemy.Definition.ai.actionsPerTurn);
                for (int i = 0; i < actions && enemy.IsAlive && Heroes.AnyAlive; i++)
                {
                    enemy.ActionsLeftThisTurn = actions - 1 - i;
                    var decision = brain.Decide(enemy, State);
                    resolver.Execute(enemy, decision.Ability, decision.Target);
                    bosses.Check();
                }
                enemy.ActionsLeftThisTurn = 0;
                enemy.TurnsTaken++;
            }
            ticker.EndTurn(Enemies);
        }

        bool CheckFinished()
        {
            bool defeat = !Heroes.AnyAlive;
            bool victory = !defeat && !Enemies.AnyAlive && !waves.HasNextWave;
            if (!defeat && !victory) return false;
            Phase = BattlePhase.Finished;
            Result = new BattleResult { Victory = victory, Heroes = Heroes.Snapshot() };
            log.Emit(new BattleEnded { Victory = victory });
            return true;
        }
    }
}
