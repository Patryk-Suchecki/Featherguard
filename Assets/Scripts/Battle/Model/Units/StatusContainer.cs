using System;
using System.Collections.Generic;
using System.Linq;

namespace Featherguard.Battle
{
    public sealed class ActiveStatus
    {
        public StatusDefinition Definition { get; }
        public int Turns { get; internal set; }
        public int Potency { get; internal set; }
        public int Stacks { get; internal set; } = 1;
        public int Points { get; internal set; }
        public Team Source { get; internal set; }
        internal bool Fresh { get; set; }

        public ActiveStatus(StatusDefinition definition, int turns, int potency, bool fresh)
        {
            Definition = definition;
            Turns = turns;
            Potency = potency;
            Fresh = fresh;
        }

        public int DamagePerTurn => Potency * Stacks;
    }

    public sealed class StatusContainer
    {
        readonly List<ActiveStatus> statuses = new();

        public event Action Changed;

        public IReadOnlyList<ActiveStatus> All => statuses;

        public bool Has(StatusDefinition definition) => statuses.Any(s => s.Definition == definition);
        public bool Any(Func<StatusDefinition, bool> predicate) => statuses.Any(s => predicate(s.Definition));
        public int Potency(Func<StatusDefinition, bool> predicate) => statuses.Where(s => predicate(s.Definition)).Sum(s => s.DamagePerTurn);

        public IEnumerable<StatModifier> Modifiers => statuses.SelectMany(s => s.Definition.Modifiers);

        public void Apply(StatusApplication application, bool duringOwnTurn, Team source = Team.Heroes, int points = 0)
        {
            var definition = application.status;
            var existing = statuses.FirstOrDefault(s => s.Definition == definition);
            if (existing != null)
            {
                existing.Turns = Math.Max(existing.Turns, application.turns);
                existing.Potency = Math.Max(existing.Potency, application.potency);
                existing.Stacks = Math.Min(definition.StackLimit, existing.Stacks + (definition.stacking == StatusStacking.Stack ? 1 : 0));
                existing.Points = Math.Max(existing.Points, points);
                existing.Fresh |= duringOwnTurn;
                existing.Source = source;
            }
            else
            {
                statuses.Add(new ActiveStatus(definition, application.turns, application.potency, duringOwnTurn) { Points = points, Source = source });
            }
            Changed?.Invoke();
        }

        public int Absorb(int damage)
        {
            int absorbed = 0;
            foreach (var status in statuses.Where(s => s.Definition.AbsorbsDamage))
            {
                int soaked = Math.Min(status.Points, damage - absorbed);
                status.Points -= soaked;
                absorbed += soaked;
                if (absorbed >= damage) break;
            }
            int removed = statuses.RemoveAll(s => s.Definition.AbsorbsDamage && s.Points <= 0);
            if (absorbed > 0 || removed > 0)
                Changed?.Invoke();
            return absorbed;
        }

        public void RemoveWhere(Func<StatusDefinition, bool> predicate)
        {
            if (statuses.RemoveAll(s => predicate(s.Definition)) > 0)
                Changed?.Invoke();
        }

        public void EndTurn()
        {
            foreach (var status in statuses)
            {
                if (status.Fresh)
                    status.Fresh = false;
                else
                    status.Turns--;
            }
            statuses.RemoveAll(s => s.Turns <= 0);
            Changed?.Invoke();
        }

        public void Clear()
        {
            statuses.Clear();
            Changed?.Invoke();
        }
    }
}
