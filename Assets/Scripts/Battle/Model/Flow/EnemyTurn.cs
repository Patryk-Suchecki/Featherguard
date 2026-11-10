using System;
using System.Collections.Generic;

namespace Featherguard.Battle
{
    public sealed class EnemyTurn
    {
        readonly BattleState state;
        readonly ActionResolver resolver;
        readonly StatusTicker ticker;
        readonly BossPhases bosses;
        readonly IEnemyBrain brain;
        readonly BattleLog log;
        readonly bool awaitBlocks;
        readonly Queue<BattleUnit> queue = new();
        BattleUnit acting;
        int actionsLeft;
        EnemyDecision? pending;

        public EnemyTurn(BattleState state, ActionResolver resolver, StatusTicker ticker, BossPhases bosses, IEnemyBrain brain, BattleLog log, bool awaitBlocks)
        {
            this.state = state;
            this.resolver = resolver;
            this.ticker = ticker;
            this.bosses = bosses;
            this.brain = brain;
            this.log = log;
            this.awaitBlocks = awaitBlocks;
        }

        public bool Active { get; private set; }

        public void Begin()
        {
            log.Emit(new TurnStarted { Team = Team.Enemies });
            ticker.BeginTurn(state.Enemies);
            queue.Clear();
            foreach (var enemy in state.Enemies.Snapshot())
                queue.Enqueue(enemy);
            acting = null;
            Active = true;
        }

        public bool Continue()
        {
            while (true)
            {
                if (acting != null && (actionsLeft == 0 || !acting.IsAlive || !state.Heroes.AnyAlive))
                    FinishActing();
                if (acting == null && (!state.Heroes.AnyAlive || !NextActing()))
                {
                    ticker.EndTurn(state.Enemies);
                    Active = false;
                    return false;
                }

                actionsLeft--;
                acting.ActionsLeftThisTurn = actionsLeft;
                var decision = brain.Decide(acting, state);
                bool hasTarget = decision.Ability.TargetsAll || decision.Target != null;
                if (awaitBlocks && decision.Ability.IsDamaging && hasTarget)
                {
                    pending = decision;
                    log.Emit(new EnemyWindup { Actor = acting, Ability = decision.Ability, Target = decision.Target });
                    return true;
                }
                resolver.Execute(acting, decision.Ability, decision.Target);
                bosses.Check();
            }
        }

        public void Block(TimingResult timing)
        {
            if (pending == null) throw new InvalidOperationException("No enemy action awaits a block.");
            var decision = pending.Value;
            pending = null;
            resolver.Execute(acting, decision.Ability, decision.Target, ActionTiming.ForBlock(timing));
            bosses.Check();
        }

        bool NextActing()
        {
            while (queue.Count > 0)
            {
                var enemy = queue.Dequeue();
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
    }
}
