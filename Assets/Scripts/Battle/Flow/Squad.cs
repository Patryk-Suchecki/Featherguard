using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Featherguard.Battle
{
    public sealed class Squad : IEnumerable<BattleUnit>
    {
        readonly List<BattleUnit> units = new();

        public Team Team { get; }

        public Squad(Team team) => Team = team;

        public int Count => units.Count;
        public BattleUnit this[int index] => units[index];
        public bool AnyAlive => units.Any(u => u.IsAlive);
        public IEnumerable<BattleUnit> Living => units.Where(u => u.IsAlive);
        public IEnumerable<BattleUnit> Taunting => Living.Where(u => u.ForcesTargeting);

        public void Add(BattleUnit unit) => units.Add(unit);
        public bool Remove(BattleUnit unit) => units.Remove(unit);
        public void Clear() => units.Clear();
        public List<BattleUnit> Snapshot() => units.ToList();

        public IEnumerator<BattleUnit> GetEnumerator() => units.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
