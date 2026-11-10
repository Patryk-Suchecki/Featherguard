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
        readonly List<ActiveStatus> effects = new();

        public event Action Changed;

        public IReadOnlyList<ActiveStatus> All => effects;

        public bool Has(StatusDefinition definition) => effects.Any(e => e.Definition == definition);
        public bool Any(Func<StatusDefinition, bool> predicate) => effects.Any(e => predicate(e.Definition));
        public int Potency(Func<StatusDefinition, bool> predicate) => effects.Where(e => predicate(e.Definition)).Sum(e => e.Potency);

        public IEnumerable<StatModifier> Modifiers => effects.SelectMany(e => e.Definition.modifiers);

        public void Apply(StatusApplication application, bool duringOwnTurn)
        {
            var existing = effects.FirstOrDefault(e => e.Definition == application.status);
            if (existing != null)
            {
                existing.Turns = Math.Max(existing.Turns, application.turns);
                existing.Potency = Math.Max(existing.Potency, application.potency);
                existing.Fresh |= duringOwnTurn;
            }
            else
            {
                effects.Add(new ActiveStatus(application.status, application.turns, application.potency, duringOwnTurn));
            }
            Changed?.Invoke();
        }

        public void RemoveWhere(Func<StatusDefinition, bool> predicate)
        {
            if (effects.RemoveAll(e => predicate(e.Definition)) > 0)
                Changed?.Invoke();
        }

        public void EndTurn()
        {
            foreach (var effect in effects)
            {
                if (effect.Fresh)
                    effect.Fresh = false;
                else
                    effect.Turns--;
            }
            effects.RemoveAll(e => e.Turns <= 0);
            Changed?.Invoke();
        }

        public void Clear()
        {
            effects.Clear();
            Changed?.Invoke();
        }
    }
}
