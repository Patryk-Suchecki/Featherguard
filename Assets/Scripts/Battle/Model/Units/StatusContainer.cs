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
        internal bool Fresh { get; set; }

        public ActiveStatus(StatusDefinition definition, int turns, int potency, bool fresh)
        {
            Definition = definition;
            Turns = turns;
            Potency = potency;
            Fresh = fresh;
        }
    }

    public sealed class StatusContainer
    {
        readonly List<ActiveStatus> statuses = new();

        public event Action Changed;

        public IReadOnlyList<ActiveStatus> All => statuses;

        public bool Has(StatusDefinition definition) => statuses.Any(s => s.Definition == definition);
        public bool Any(Func<StatusDefinition, bool> predicate) => statuses.Any(s => predicate(s.Definition));
        public int Potency(Func<StatusDefinition, bool> predicate) => statuses.Where(s => predicate(s.Definition)).Sum(s => s.Potency);

        public IEnumerable<StatModifier> Modifiers => statuses.SelectMany(s => s.Definition.modifiers);

        public void Apply(StatusApplication application, bool duringOwnTurn)
        {
            var existing = statuses.FirstOrDefault(s => s.Definition == application.status);
            if (existing != null)
            {
                existing.Turns = Math.Max(existing.Turns, application.turns);
                existing.Potency = Math.Max(existing.Potency, application.potency);
                existing.Fresh |= duringOwnTurn;
            }
            else
            {
                statuses.Add(new ActiveStatus(application.status, application.turns, application.potency, duringOwnTurn));
            }
            Changed?.Invoke();
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
