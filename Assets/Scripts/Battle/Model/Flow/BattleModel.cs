using System;
using System.Collections.Generic;
using System.Linq;
using Featherguard.Core;

namespace Featherguard.Battle
{
    public enum BattlePhase { NotStarted, AwaitingInput, AwaitingBlock, Finished }

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
        readonly Queue<BattleUnit> enemyQueue = new();
        bool enemyTurn;
        BattleUnit acting;
        int actionsLeft;
        EnemyDecision? pending;

        public Combat Combat { get; }
        public RageMeter Rage => Combat.Rage;
        public Squad Heroes => Combat.Heroes;
        public Squad Enemies => Combat.Enemies;
        public BattlePhase Phase { get; private set; }
        public bool AwaitingInput => Phase == BattlePhase.AwaitingInput;
        public bool AwaitingBlock => Phase == BattlePhase.AwaitingBlock;
        public bool IsFinished => Phase == BattlePhase.Finished;
        public int Turn { get; private set; }
        public BattleResult Result { get; private set; }

        public event Action<BattleUnit> EnemyKilled;

        public BattleModel(BattleSetup setup, BattleRules rules, IRandomSource random = null, IEnemyBrain brain = null)
        {
            this.setup = setup;
            random ??= UnityRandomSource.Shared;
            Combat = new Combat(rules, random, log, setup.StartRage);
            Combat.UnitKilled += unit =>
            {
                if (unit.Team == Team.Enemies) EnemyKilled?.Invoke(unit);
            };
            resolver = new ActionResolver(Combat);
            ticker = new StatusTicker(Combat);
            waves = new WaveSpawner(Combat, setup);
            bosses = new BossPhases(Combat, waves);
            this.brain = brain ?? new DefaultEnemyBrain(random);
        }

        public IReadOnlyList<BattleEvent> Start()
        {
            if (Phase != BattlePhase.NotStarted) throw new InvalidOperationException("Battle already started.");
            for (int i = 0; i < setup.Heroes.Count && i < Combat.Rules.heroSlotCount; i++)
            {
                var loadout = setup.Heroes[i];
                Combat.Spawn(loadout.Definition, i, loadout.Modifiers, loadout.Health);
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
            return ability.TargetsAll || Targeting.IsValid(ability, target, Combat.AlliesOf(hero), Combat.OpponentsOf(hero));
        }

        public IEnumerable<BattleUnit> Candidates(BattleUnit hero, AbilityDefinition ability) =>
            Targeting.Candidates(ability, Combat.AlliesOf(hero), Combat.OpponentsOf(hero));

        public IReadOnlyList<BattleEvent> Act(BattleUnit hero, AbilityDefinition ability, BattleUnit target, TimingResult timing = TimingResult.Miss)
        {
            if (!CanUse(hero, ability, target)) return Array.Empty<BattleEvent>();
            bool special = ability == hero.Definition.special;
            if (special)
                Combat.ConsumeRage();
            hero.HasActed = true;
            resolver.Execute(hero, ability, target, special ? ActionTiming.None : ActionTiming.ForAttack(timing));
            bosses.Check();
            Advance();
            return log.Flush();
        }

        public IReadOnlyList<BattleEvent> Block(TimingResult timing)
        {
            if (!AwaitingBlock || pending == null) return Array.Empty<BattleEvent>();
            var decision = pending.Value;
            pending = null;
            resolver.Execute(acting, decision.Ability, decision.Target, ActionTiming.ForBlock(timing));
            bosses.Check();
            Advance();
            return log.Flush();
        }

        void Advance()
        {
            for (int step = 0; step < MaxSteps; step++)
            {
                if (enemyTurn)
                {
                    if (ContinueEnemyTurn())
                    {
                        Phase = BattlePhase.AwaitingBlock;
                        return;
                    }
                    enemyTurn = false;
                    if (CheckFinished()) return;
                    if (!Enemies.AnyAlive)
                        waves.SpawnNext();
                    BeginPlayerTurn();
                    continue;
                }

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

                StartEnemyTurn();
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

        void StartEnemyTurn()
        {
            log.Emit(new TurnStarted { Team = Team.Enemies });
            ticker.BeginTurn(Enemies);
            enemyQueue.Clear();
            foreach (var enemy in Enemies.Snapshot())
                enemyQueue.Enqueue(enemy);
            acting = null;
            enemyTurn = true;
        }

        bool ContinueEnemyTurn()
        {
            while (true)
            {
                if (acting != null && (actionsLeft == 0 || !acting.IsAlive || !Heroes.AnyAlive))
                    FinishActing();
                if (acting == null && (!Heroes.AnyAlive || !NextActing()))
                {
                    ticker.EndTurn(Enemies);
                    return false;
                }

                actionsLeft--;
                acting.ActionsLeftThisTurn = actionsLeft;
                var decision = brain.Decide(acting, Combat);
                bool hasTarget = decision.Ability.TargetsAll || decision.Target != null;
                if (setup.AwaitBlocks && decision.Ability.IsDamaging && hasTarget)
                {
                    pending = decision;
                    log.Emit(new EnemyWindup { Actor = acting, Ability = decision.Ability, Target = decision.Target });
                    return true;
                }
                resolver.Execute(acting, decision.Ability, decision.Target);
                bosses.Check();
            }
        }

        bool NextActing()
        {
            while (enemyQueue.Count > 0)
            {
                var enemy = enemyQueue.Dequeue();
                if (!enemy.IsAlive || enemy.SkipsTurn) continue;
                acting = enemy;
                actionsLeft = Math.Max(1, enemy.Definition.ai.actionsPerTurn);
                return true;
            }
            return false;
        }

        void FinishActing()
        {
            acting.ActionsLeftThisTurn = 0;
            acting.TurnsTaken++;
            acting = null;
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
